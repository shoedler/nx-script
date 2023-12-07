#ifndef debug_h
#define debug_h

#include "chunk.h"

#define ANSI_COLOR_RED "\x1b[31m"
#define ANSI_COLOR_GREEN "\x1b[32m"
#define ANSI_COLOR_YELLOW "\x1b[33m"
#define ANSI_COLOR_BLUE "\x1b[34m"
#define ANSI_COLOR_MAGENTA "\x1b[35m"
#define ANSI_COLOR_CYAN "\x1b[36m"
#define ANSI_COLOR_RESET "\x1b[0m"

#define ANSI_RED_STR(str) ANSI_COLOR_RED##str##ANSI_COLOR_RESET
#define ANSI_GREEN_STR(str) ANSI_COLOR_GREEN##str##ANSI_COLOR_RESET
#define ANSI_YELLOW_STR(str) ANSI_COLOR_YELLOW##str##ANSI_COLOR_RESET
#define ANSI_BLUE_STR(str) ANSI_COLOR_BLUE##str##ANSI_COLOR_RESET
#define ANSI_MAGENTA_STR(str) ANSI_COLOR_MAGENTA##str##ANSI_COLOR_RESET
#define ANSI_CYAN_STR(str) ANSI_COLOR_CYAN##str##ANSI_COLOR_RESET

void disassemble_chunk(Chunk* chunk, const char* name);
int disassemble_instruction(Chunk* chunk, int offset);

#define INTERNAL_ERROR(format_literal, ...)                                   \
  fprintf(stderr, "INTERNAL_ERROR at %s(%u): " format_literal "\n", __FILE__, \
          __LINE__, ##__VA_ARGS__)

#endif
