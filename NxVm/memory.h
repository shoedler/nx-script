#ifndef memory_h
#define memory_h

#include "common.h"
#include "object.h"

#define ALLOCATE(type, count) (type*)reallocate(NULL, 0, sizeof(type) * (count))

#define FREE(type, pointer) reallocate(pointer, sizeof(type), 0)

#define GROW_CAPACITY(capacity) ((capacity) < 8 ? 8 : (capacity) * 2)

#define GROW_ARRAY(type, pointer, old_count, new_count)  \
  (type*)reallocate(pointer, sizeof(type) * (old_count), \
                    sizeof(type) * (new_count))

#define FREE_ARRAY(type, pointer, old_count) \
  reallocate(pointer, sizeof(type) * (old_count), 0)

#define GC_HEAP_GROW_FACTOR 2
#define GC_DEFAULT_THRESHOLD 1024 * 1024

/**
 * @brief Reallocates memory.
 * If old_size == 0 and new_size != 0, then the function allocates memory.
 * If old_size != 0 and new_size == 0, then the function frees memory.
 * If old_size != 0 and new_size <old_size, then the function shrinks memory.
 * If old_size != 0 and new_size > old_size, then the function expands memory.
 * @param pointer Pointer to the memory to reallocate.
 * @param old_size Old size of the memory.
 * @param new_size New size of the memory.
 * @return void* Pointer to the reallocated memory.
 */
void* reallocate(void* pointer, size_t old_size, size_t new_size);
void collect_garbage();
void mark_value(Value value);
void mark_obj(Obj* object);
void free_objects();

#endif