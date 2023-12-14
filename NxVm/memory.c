#include "memory.h"
#include <stdlib.h>
#include "compiler.h"
#include "vm.h"

#ifdef DEBUG_LOG_GC
#include <stdio.h>
#include "debug.h"
#endif

void* reallocate(void* pointer, size_t old_size, size_t new_size) {
  vm.bytes_allocated += new_size - old_size;

  if (new_size > old_size) {
    // Only collect_garbage if we're not freeing memory
#ifdef DEBUG_STRESS_GC
    collect_garbage();
#endif
    if (vm.bytes_allocated > vm.next_gc) {
      collect_garbage();
    }
  }

  if (new_size == 0) {
    free(pointer);
    return NULL;
  }

  void* result = realloc(pointer, new_size);
  if (result == NULL) {
    // TODO: Handle out of memory
    INTERNAL_ERROR("Not enough memory to reallocate");
    exit(70);
  }

  return result;
}

void mark_obj(Obj* object) {
  if (object == NULL) {
    return;
  }
  if (object->is_marked) {
    return;
  }

#ifdef DEBUG_LOG_GC
  printf(ANSI_RED_STR("[GC] ") ANSI_YELLOW_STR("[MARK] ") "%p, ",
         (void*)object);
  print_value(OBJ_VAL(object));
  printf("\n");
#endif

  object->is_marked = true;

  if (vm.gray_capacity < vm.gray_count + 1) {
    vm.gray_capacity = GROW_CAPACITY(vm.gray_capacity);
    vm.gray_stack =
        (Obj**)realloc(vm.gray_stack, sizeof(Obj*) * vm.gray_capacity);

    // TODO: Handle out of memory
    if (vm.gray_stack == NULL) {
      INTERNAL_ERROR("Not enough memory to reallocate gray stack");
      exit(70);
    }
  }

  vm.gray_stack[vm.gray_count++] = object;
}

void mark_value(Value value) {
  if (IS_OBJ(value)) {
    mark_obj(AS_OBJ(value));
  }
}

void mark_array(ValueArray* array) {
  for (int i = 0; i < array->count; i++) {
    mark_value(array->values[i]);
  }
}

static void blacken_object(Obj* object) {
#ifdef DEBUG_LOG_GC
  printf(ANSI_RED_STR("[GC] ") ANSI_BLUE_STR("[BLACKEN] ") "%p, ",
         (void*)object);
  print_value(OBJ_VAL(object));
  printf("\n");
#endif

  switch (object->type) {
    case OBJ_BOUND_METHOD: {
      ObjBoundMethod* bound = (ObjBoundMethod*)object;
      mark_value(bound->receiver);
      mark_obj((Obj*)bound->method);
      break;
    }
    case OBJ_CLASS: {
      ObjClass* klass = (ObjClass*)object;
      mark_obj((Obj*)klass->name);
      break;
    }
    case OBJ_CLOSURE: {
      ObjClosure* closure = (ObjClosure*)object;
      mark_obj((Obj*)closure->function);
      for (int i = 0; i < closure->upvalue_count; i++) {
        mark_obj((Obj*)closure->upvalues[i]);
      }
      break;
    }
    case OBJ_FUNCTION: {
      ObjFunction* function = (ObjFunction*)object;
      mark_obj((Obj*)function->name);
      mark_array(&function->chunk.constants);
      break;
    }
    case OBJ_INSTANCE: {
      ObjInstance* instance = (ObjInstance*)object;
      mark_obj((Obj*)instance->klass);
      mark_hashtable(&instance->fields);
      break;
    }
    case OBJ_UPVALUE:
      mark_value(((ObjUpvalue*)object)->closed);
      break;
    case OBJ_NATIVE:
    case OBJ_STRING:
      break;
    default:
      break;  // TODO: THROW? Probably yes, bc we need to mark all objects
  }
}

static void free_object(Obj* object) {
#ifdef DEBUG_LOG_GC
  printf(ANSI_RED_STR("[GC] ") ANSI_GREEN_STR("[FREE] ") "%p, type %d\n",
         (void*)object, object->type);
#endif

  switch (object->type) {
    case OBJ_BOUND_METHOD:
      FREE(ObjBoundMethod, object);
      break;
    case OBJ_CLASS: {
      ObjClass* klass = (ObjClass*)object;
      free_hashtable(&klass->methods);
      FREE(ObjClass, object);
      break;
    }
    case OBJ_CLOSURE: {
      ObjClosure* closure = (ObjClosure*)object;
      FREE_ARRAY(ObjUpvalue*, closure->upvalues, closure->upvalue_count);
      FREE(ObjClosure, object);
      break;
    }
    case OBJ_FUNCTION: {
      ObjFunction* function = (ObjFunction*)object;
      free_chunk(&function->chunk);
      FREE(ObjFunction, object);
      break;
    }
    case OBJ_INSTANCE: {
      ObjInstance* instance = (ObjInstance*)object;
      free_hashtable(&instance->fields);
      FREE(ObjInstance, object);
      break;
    }
    case OBJ_NATIVE:
      FREE(ObjNative, object);
      break;
    case OBJ_STRING: {
      ObjString* string = (ObjString*)object;
      FREE_ARRAY(char, string->chars, string->length + 1);
      FREE(ObjString, object);
      break;
    }
    case OBJ_UPVALUE:
      FREE(ObjUpvalue, object);
      break;
      // TODO: default? THROW?
  }
}

void free_objects() {
  Obj* object = vm.objects;
  while (object != NULL) {
    Obj* next = object->next;
    free_object(object);
    object = next;
  }

  free(vm.gray_stack);
}

static void mark_roots() {
  for (Value* slot = vm.stack; slot < vm.stack_top; slot++) {
    mark_value(*slot);
  }

  for (int i = 0; i < vm.frame_count; i++) {
    mark_obj((Obj*)vm.frames[i].closure);
  }

  for (ObjUpvalue* upvalue = vm.open_upvalues; upvalue != NULL;
       upvalue = upvalue->next) {
    mark_obj((Obj*)upvalue);
  }

  mark_hashtable(&vm.globals);
  mark_compiler_roots();
  mark_obj((Obj*)vm.init_string);
}

static void trace_references() {
  while (vm.gray_count > 0) {
    Obj* object = vm.gray_stack[--vm.gray_count];
    blacken_object(object);
  }
}

static void sweep() {
  Obj* previous = NULL;
  Obj* object = vm.objects;

  while (object != NULL) {
    if (object->is_marked) {
      object->is_marked = false;  // Unmark for next gc cycle
      previous = object;
      object = object->next;
    } else {
      Obj* unreached = object;

      object = object->next;
      if (previous != NULL) {
        previous->next = object;
      } else {
        vm.objects = object;
      }

      free_object(unreached);  // 🎉
    }
  }
}

/*
Tri-color gc

░ White: At the beginning of a garbage collection, every object is
white. This color means we have not reached or processed the object at all.

▒ Gray: During marking, when we first reach an object, we darken it
gray. This color means we know the object itself is reachable and should not be
collected. But we have not yet traced through it to see what other objects it
references. In graph algorithm terms, this is the worklist—the set of objects we
know about but haven’t processed yet.

█ Black: When we take a gray object and mark all of the objects it references,
we then turn the gray object black. This color means the mark phase is done
processing that object.
*/
void collect_garbage() {
#ifdef DEBUG_LOG_GC
  printf("== Gc begin collect ==\n");
  size_t before = vm.bytes_allocated;
#endif

  mark_roots();
  trace_references();
  hashtable_remove_white(&vm.strings);
  sweep();

  vm.next_gc = vm.bytes_allocated * GC_HEAP_GROW_FACTOR;

#ifdef DEBUG_LOG_GC
  printf(
      ANSI_RED_STR(
          "[GC] ") "Done. collected %zu bytes (from %zu to %zu) next at %zu\n",
      before - vm.bytes_allocated, before, vm.bytes_allocated, vm.next_gc);
  printf("== Gc end collect ==\n");
#endif
}
