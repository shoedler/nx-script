#ifndef debug_h
#define debug_h

#include "chunk.h"

void disassemble_chunk(Chunk* chunk, const char* name);
int disassemble_instruction(Chunk* chunk, int offset);

#define INTERNAL_ERROR(FormatLiteral, ...)                                   \
  fprintf(stderr, "INTERNAL_ERROR at %s(%u): " FormatLiteral "\n", __FILE__, \
          __LINE__, ##__VA_ARGS__)

#endif