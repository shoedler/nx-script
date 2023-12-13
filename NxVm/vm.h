#ifndef vm_h
#define vm_h

#include "chunk.h"
#include "hashtable.h"
#include "object.h"
#include "value.h"

#define FRAMES_MAX 64
#define STACK_MAX (FRAMES_MAX * UINT8_COUNT)
#define CLASS_CONSTRUCTOR_KEYWORD "ctor"
#define CLASS_CONSTRUCTOR_KEYWORD_LENGTH (sizeof(CLASS_CONSTRUCTOR_KEYWORD) - 1)
#define BASE_CLASS_KEYWORD "base"

typedef struct {
  ObjClosure* closure;
  uint8_t* ip;
  Value* slots;
} CallFrame;

typedef struct {
  CallFrame frames[FRAMES_MAX];
  int frame_count;

  Chunk* chunk;
  uint8_t* ip;
  Value stack[STACK_MAX];
  Value* stack_top;
  HashTable globals;
  HashTable strings;
  ObjString* init_string;
  ObjUpvalue* open_upvalues;
  Obj* objects;

  size_t bytes_allocated;
  size_t next_gc;
  int gray_count;
  int gray_capacity;
  Obj** gray_stack;
} Vm;

typedef enum {
  INTERPRET_OK,
  INTERPRET_COMPILE_ERROR,
  INTERPRET_RUNTIME_ERROR
} InterpretResult;

extern Vm vm;

void init_vm();
void free_vm();

InterpretResult interpret(const char* source);
void push(Value value);
Value pop();

#endif
