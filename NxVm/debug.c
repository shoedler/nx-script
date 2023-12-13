#include "debug.h"

#include <stdio.h>
#include "object.h"
#include "value.h"

// 5 chars wide
#define PRINT_OFFSET(_offset) printf(ANSI_MAGENTA_STR("%04d "), _offset)
// 5 chars wide
#define PRINT_LINE(_line) printf(ANSI_BLUE_STR("%04d "), _line)
#define PRINT_SAME_LINE() printf(ANSI_BLUE_STR("   | "))
// 16 chars wide
#define PRINT_OPCODE(_name) printf(ANSI_GREEN_STR("%-16.16s"), _name)
// 6 chars wide
#define PRINT_INT(_int) printf(ANSI_RED_STR(" %4d "), _int)
#define PRINT_NO_INT() printf(ANSI_RED_STR("      "))
// 36 chars wide
#define PRINT_VALUE_STR(_str) printf("%-36.36s", _str)

COMBAK: We need to be able to be able to get the value string via e.g. "value_to_string".
Then use this function within print_value, _object, _function etc. to get a nice readout for the vms internal print statement.
Then use value_to_string in here in combination with PRINT_VALUE_STR to get a nicely indented and truncated readout for the debugger

void disassemble_chunk(Chunk* chunk, const char* name) {
  printf("\n== Chunk: %s ==\n", name);

  for (int offset = 0; offset < chunk->count;) {
    offset = disassemble_instruction(chunk, offset);
    printf("\n");
  }

  printf("== End of chunk: %s ==\n", name);
}

static int simple_instruction(const char* name, int offset) {
  PRINT_OPCODE(name);
  PRINT_NO_INT();
  PRINT_VALUE_STR("");
  return offset + 1;
}

static int byte_instruction(const char* name, Chunk* chunk, int offset) {
  uint8_t slot = chunk->code[offset + 1];
  PRINT_OPCODE(name);
  PRINT_INT(slot);
  PRINT_VALUE_STR("");
  return offset + 2;
}

static int jump_instruction(const char* name,
                            int sign,
                            Chunk* chunk,
                            int offset) {
  uint16_t jump = (uint16_t)(chunk->code[offset + 1] << 8);
  jump |= chunk->code[offset + 2];
  char* jmp_str[13];
  sprintf(jmp_str, "%04d -> %04d", offset, offset + 3 + sign * jump);
  PRINT_OPCODE(name);
  PRINT_NO_INT();
  PRINT_VALUE_STR(jmp_str);
}

static int constant_instruction(const char* name, Chunk* chunk, int offset) {
  uint8_t constant = chunk->code[offset + 1];
  PRINT_OPCODE(name);
  PRINT_INT(constant);
  print_value(chunk->constants.values[constant]);
  return offset + 2;
}

static int closure_instruction(const char* name, Chunk* chunk, int offset) {
  offset++;
  uint8_t constant = chunk->code[offset++];
  PRINT_OPCODE(name);
  PRINT_INT(constant);
  print_value(chunk->constants.values[constant]);

  ObjFunction* function = AS_FUNCTION(chunk->constants.values[constant]);
  for (int j = 0; j < function->upvalue_count; j++) {
    int is_local = chunk->code[offset++];
    int index = chunk->code[offset++];
    char* upval_str[20];
    sprintf(upval_str, "%s %d", is_local ? "local" : "upvalue", index);

    printf("\n");
    PRINT_OFFSET(offset - 2);
    PRINT_SAME_LINE();
    PRINT_OPCODE("");
    PRINT_NO_INT();
    PRINT_VALUE_STR(upval_str);
  }

  return offset;
}

static int invoke_instruction(const char* name, Chunk* chunk, int offset) {
  uint8_t constant = chunk->code[offset + 1];
  uint8_t arg_count = chunk->code[offset + 2];
  PRINT_OPCODE(name);
  PRINT_INT(constant);
  print_value(chunk->constants.values[constant]);
  printf(", %d args", arg_count);

  return offset + 3;
}

int disassemble_instruction(Chunk* chunk, int offset) {
  PRINT_OFFSET(offset);

  if (offset > 0 && chunk->lines[offset] == chunk->lines[offset - 1]) {
    PRINT_SAME_LINE();
  } else {
    PRINT_LINE(chunk->lines[offset]);
  }

  uint8_t instruction = chunk->code[offset];
  switch (instruction) {
    case OP_CONSTANT:
      return constant_instruction("OP_CONSTANT", chunk, offset);
    case OP_NIL:
      return simple_instruction("OP_NIL", offset);
    case OP_TRUE:
      return simple_instruction("OP_TRUE", offset);
    case OP_FALSE:
      return simple_instruction("OP_FALSE", offset);
    case OP_POP:
      return simple_instruction("OP_POP", offset);
    case OP_GET_LOCAL:
      return byte_instruction("OP_GET_LOCAL", chunk, offset);
    case OP_SET_LOCAL:
      return byte_instruction("OP_SET_LOCAL", chunk, offset);
    case OP_GET_GLOBAL:
      return constant_instruction("OP_GET_GLOBAL", chunk, offset);
    case OP_DEFINE_GLOBAL:
      return constant_instruction("OP_DEFINE_GLOBAL", chunk, offset);
    case OP_SET_GLOBAL:
      return constant_instruction("OP_SET_GLOBAL", chunk, offset);
    case OP_GET_UPVALUE:
      return byte_instruction("OP_GET_UPVALUE", chunk, offset);
    case OP_SET_UPVALUE:
      return byte_instruction("OP_SET_UPVALUE", chunk, offset);
    case OP_GET_PROPERTY:
      return constant_instruction("OP_GET_PROPERTY", chunk, offset);
    case OP_SET_PROPERTY:
      return constant_instruction("OP_SET_PROPERTY", chunk, offset);
    case OP_GET_BASE_CLASS:
      return constant_instruction("OP_GET_BASE_CLASS", chunk, offset);
    case OP_EQ:
      return simple_instruction("OP_EQ", offset);
    case OP_NEQ:
      return simple_instruction("OP_NEQ", offset);
    case OP_GT:
      return simple_instruction("OP_GT", offset);
    case OP_LT:
      return simple_instruction("OP_LT", offset);
    case OP_GTEQ:
      return simple_instruction("OP_GTEQ", offset);
    case OP_LTEQ:
      return simple_instruction("OP_LTEQ", offset);
    case OP_NEGATE:
      return simple_instruction("OP_NEGATE", offset);
    case OP_PRINT:
      return simple_instruction("OP_PRINT", offset);
    case OP_JUMP:
      return jump_instruction("OP_JUMP", 1, chunk, offset);
    case OP_JUMP_IF_FALSE:
      return jump_instruction("OP_JUMP_IF_FALSE", 1, chunk, offset);
    case OP_LOOP:
      return jump_instruction("OP_LOOP", -1, chunk, offset);
    case OP_CALL:
      return byte_instruction("OP_CALL", chunk, offset);
    case OP_INVOKE:
      return invoke_instruction("OP_INVOKE", chunk, offset);
    case OP_BASE_INVOKE:
      return invoke_instruction("OP_BASE_INVOKE", chunk, offset);
    case OP_CLOSURE:
      return closure_instruction("OP_CLOSURE", chunk, offset);
    case OP_CLOSE_UPVALUE:
      return simple_instruction("OP_CLOSE_UPVALUE", offset);
    case OP_RETURN:
      return simple_instruction("OP_RETURN", offset);
    case OP_CLASS:
      return constant_instruction("OP_CLASS", chunk, offset);
    case OP_INHERIT:
      return simple_instruction("OP_INHERIT", offset);
    case OP_METHOD:
      return constant_instruction("OP_METHOD", chunk, offset);
    case OP_ADD:
      return simple_instruction("OP_ADD", offset);
    case OP_SUBTRACT:
      return simple_instruction("OP_SUBTRACT", offset);
    case OP_MULTIPLY:
      return simple_instruction("OP_MULTIPLY", offset);
    case OP_DIVIDE:
      return simple_instruction("OP_DIVIDE", offset);
    case OP_NOT:
      return simple_instruction("OP_NOT", offset);
    default:
      INTERNAL_ERROR("Unhandled opcode: %d\n", instruction);
      return offset + 1;
  }
}

#undef PRINT_OFFSET
#undef PRINT_LINE
#undef PRINT_SAME_LINE
#undef PRINT_OPCODE
#undef PRINT_INT
#undef PRINT_JUMP