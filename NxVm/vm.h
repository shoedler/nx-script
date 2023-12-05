#ifndef nxs_vm_h
#define nxs_vm_h

#include "chunk.h"
#include "hashtable.h"
#include "value.h"

#define STACK_MAX 256

typedef struct
{
	Chunk *chunk;
	uint8_t *ip;
	Value stack[STACK_MAX];
	Value *stack_top;
	HashTable globals;
	HashTable strings;
	Obj *objects;
} Vm;

typedef enum
{
	INTERPRET_OK,
	INTERPRET_COMPILE_ERROR,
	INTERPRET_RUNTIME_ERROR
} InterpretResult;

extern Vm vm;

void init_vm();
void free_vm();

InterpretResult interpret(const char *source);
void push(Value value);
Value pop();

#endif