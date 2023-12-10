#include "compiler.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "common.h"
#include "debug.h"
#include "memory.h"
#include "scanner.h"

typedef struct {
  Token current;
  Token previous;
  bool had_error;
  bool panic_mode;
} Parser;

typedef enum {
  PREC_NONE,
  PREC_ASSIGN,      // =
  PREC_OR,          // or
  PREC_AND,         // and
  PREC_EQUALITY,    // == !=
  PREC_COMPARISON,  // < > <= >=
  PREC_TERM,        // + -
  PREC_FACTOR,      // * / %
  PREC_UNARY,       // ! -
  PREC_CALL,        // . ()
  PREC_PRIMARY
} Precedence;

typedef void (*ParseFn)(bool can_assign);

typedef struct {
  ParseFn prefix;
  ParseFn infix;
  Precedence precedence;
} ParseRule;

typedef struct {
  Token name;
  int depth;
  bool is_captured;
} Local;

typedef struct {
  uint8_t index;
  bool is_local;
} Upvalue;

typedef enum { TYPE_FUNCTION, TYPE_TOPLEVEL } FunctionType;

typedef struct Compiler {
  struct Compiler* enclosing;
  ObjFunction* function;
  FunctionType type;

  Local locals[UINT8_COUNT];
  int local_count;
  Upvalue upvalues[UINT8_COUNT];
  int scope_depth;
} Compiler;

Parser parser;
Compiler* current = NULL;
Chunk* compiling_chunk;

static Chunk* current_chunk() {
  return &current->function->chunk;
}

static void error_at(Token* token, const char* message) {
  if (parser.panic_mode) {
    return;
  }

  parser.panic_mode = true;
  fprintf(stderr, "[line %d] ERROR", token->line);

  if (token->type == TOKEN_EOF) {
    fprintf(stderr, " at end");
  } else if (token->type == TOKEN_ERROR) {
    // Nothing.
  } else {
    fprintf(stderr, " at '%.*s'", token->length, token->start);
  }

  fprintf(stderr, ": %s\n", message);
  parser.had_error = true;
}

static void error(const char* message) {
  error_at(&parser.previous, message);
}

static void error_at_current(const char* message) {
  error_at(&parser.current, message);
}

static void advance() {
  parser.previous = parser.current;

  for (;;) {
    parser.current = scan_token();

    if (parser.current.type != TOKEN_ERROR) {
      break;
    }

    error_at_current(parser.current.start);
  }
}

static void consume(TokenType type, const char* message) {
  if (parser.current.type == type) {
    advance();
    return;
  }

  error_at_current(message);
}

static bool check(TokenType type) {
  return parser.current.type == type;
}

static bool match(TokenType type) {
  if (!check(type)) {
    return false;
  }
  advance();
  return true;
}

static void emit_byte(uint8_t byte) {
  write_chunk(current_chunk(), byte, parser.previous.line);
}

static void emit_bytes(uint8_t byte1, uint8_t byte2) {
  emit_byte(byte1);
  emit_byte(byte2);
}

static void emit_loop(int loop_start) {
  emit_byte(OP_LOOP);

  int offset = current_chunk()->count - loop_start + 2;
  if (offset > UINT16_MAX) {
    error("Loop body too large.");
  }

  emit_byte((offset >> 8) & 0xff);
  emit_byte(offset & 0xff);
}

static int emit_jump(uint8_t instruction) {
  emit_byte(instruction);
  emit_byte(0xff);
  emit_byte(0xff);
  return current_chunk()->count - 2;
}

static void patch_jump(int offset) {
  // -2 to adjust for the bytecode for the jump offset itself.
  int jump = current_chunk()->count - offset - 2;

  if (jump > UINT16_MAX) {
    error("Too much code to jump over.");
  }

  current_chunk()->code[offset] = (jump >> 8) & 0xff;
  current_chunk()->code[offset + 1] = jump & 0xff;
}

static uint8_t make_constant(Value value) {
  int constant = add_constant(current_chunk(), value);
  if (constant > UINT8_MAX) {
    error("Too many constants in one chunk.");
    return 0;
  }

  return (uint8_t)constant;
}

static void emit_constant(Value value) {
  emit_bytes(OP_CONSTANT, make_constant(value));
}

static void emit_return() {
  emit_byte(OP_NIL);
  emit_byte(OP_RETURN);
}

static void init_compiler(Compiler* compiler,
                          FunctionType type,
                          ObjString* name) {
  compiler->enclosing = current;
  compiler->function = NULL;
  compiler->type = type;
  compiler->local_count = 0;
  compiler->scope_depth = 0;
  compiler->function = new_function();
  current = compiler;

  if (type != TYPE_TOPLEVEL) {
    current->function->name = name;
  }

  Local* local = &current->locals[current->local_count++];
  local->depth = 0;
  local->is_captured = false;
  local->name.start = "";  // Not accessible.
  local->name.length = 0;
}

static ObjFunction* end_compiler() {
  emit_return();
  ObjFunction* function = current->function;

#ifdef DEBUG_PRINT_CODE
  if (!parser.had_error) {
    disassemble_chunk(current_chunk(), function->name != NULL
                                           ? function->name->chars
                                           : "[Toplevel]");

    if (current->enclosing == NULL) {
      printf("\n== End of compilation ==\n\n");
    }
  }
#endif
  current = current->enclosing;

  return function;
}

static void begin_scope() {
  current->scope_depth++;
}

static void end_scope() {
  current->scope_depth--;

  while (current->local_count > 0 &&
         current->locals[current->local_count - 1].depth >
             current->scope_depth) {
    // Since we're leaving the scope, we don't need the local variables anymore.
    // The ones who got captured by a closure are still needed, and are
    // going to need to live on the heap.
    if (current->locals[current->local_count - 1].is_captured) {
      emit_byte(OP_CLOSE_UPVALUE);
    } else {
      emit_byte(OP_POP);
    }
    current->local_count--;
  }
}

static void expression();
static void statement();

static uint8_t argument_list();
static ParseRule* get_rule(TokenType type);
static void parse_precedence(Precedence precedence);
static uint8_t string_constant(Token* name);
static int resolve_local(Compiler* compiler, Token* name);
static int resolve_upvalue(Compiler* compiler, Token* name);
static int add_upvalue(Compiler* compiler, uint8_t index, bool is_local);
static uint8_t parse_variable(const char* error_message);
static void define_variable(uint8_t global);

static void binary(bool can_assign) {
  TokenType op_type = parser.previous.type;
  ParseRule* rule = get_rule(op_type);
  parse_precedence((Precedence)(rule->precedence + 1));

  switch (op_type) {
    case TOKEN_NEQ:
      emit_byte(OP_NEQ);
      break;
    case TOKEN_EQ:
      emit_byte(OP_EQ);
      break;
    case TOKEN_GT:
      emit_byte(OP_GT);
      break;
    case TOKEN_GTEQ:
      emit_byte(OP_GTEQ);
      break;
    case TOKEN_LT:
      emit_byte(OP_LT);
      break;
    case TOKEN_LTEQ:
      emit_byte(OP_LTEQ);
      break;
    case TOKEN_PLUS:
      emit_byte(OP_ADD);
      break;
    case TOKEN_MINUS:
      emit_byte(OP_SUBTRACT);
      break;
    case TOKEN_MULT:
      emit_byte(OP_MULTIPLY);
      break;
    case TOKEN_DIV:
      emit_byte(OP_DIVIDE);
      break;
    default:
      INTERNAL_ERROR("Unhandled binary operator type: %d", op_type);
      break;
  }
}

static void call(bool can_assign) {
  uint8_t arg_count = argument_list();
  emit_bytes(OP_CALL, arg_count);
}

static void dot(bool can_assign) {
  consume(TOKEN_ID, "Expecting property name after '.'.");
  uint8_t name = string_constant(&parser.previous);

  if (can_assign && match(TOKEN_ASSIGN)) {
    expression();
    emit_bytes(OP_SET_PROPERTY, name);
  } else {
    emit_bytes(OP_GET_PROPERTY, name);
  }
}

static void literal(bool can_assign) {
  TokenType op_type = parser.previous.type;

  switch (op_type) {
    case TOKEN_FALSE:
      emit_byte(OP_FALSE);
      break;
    case TOKEN_NIL:
      emit_byte(OP_NIL);
      break;
    case TOKEN_TRUE:
      emit_byte(OP_TRUE);
      break;
    default:
      INTERNAL_ERROR("Unhandled literal: %d", op_type);
      return;
  }
}

static void grouping(bool can_assign) {
  expression();
  consume(TOKEN_CPAR, "Expecting ')' after expression.");
}

static void number(bool can_assign) {
  double value = strtod(parser.previous.start, NULL);
  emit_constant(NUMBER_VAL(value));
}

static void string(bool can_assign) {
  emit_constant(OBJ_VAL(
      copy_string(parser.previous.start + 1, parser.previous.length - 2)));
}

static void unary(bool can_assign) {
  TokenType operator_type = parser.previous.type;

  // Compile the operand.
  expression();

  // Emit the operator instruction.
  switch (operator_type) {
    case TOKEN_NOT:
      emit_byte(OP_NOT);
      break;
    case TOKEN_MINUS:
      emit_byte(OP_NEGATE);
      break;
    default:
      INTERNAL_ERROR("Unhandled unary operator type: %d", operator_type);
      return;
  }
}

/// @brief Generates Bytecode to load a variable with the given name onto the
/// stack.
static void named_variable(Token name, bool can_assign) {
  uint8_t get_op, set_op;
  int arg = resolve_local(current, &name);
  if (arg != -1) {
    get_op = OP_GET_LOCAL;
    set_op = OP_SET_LOCAL;
  } else if ((arg = resolve_upvalue(current, &name)) != -1) {
    get_op = OP_GET_UPVALUE;
    set_op = OP_SET_UPVALUE;
  } else {
    arg = string_constant(&name);
    get_op = OP_GET_GLOBAL;
    set_op = OP_SET_GLOBAL;
  }

  if (can_assign && match(TOKEN_ASSIGN)) {
    expression();
    emit_bytes(set_op, (uint8_t)arg);
  } else {
    emit_bytes(get_op, (uint8_t)arg);
  }
}

static void variable(bool can_assign) {
  named_variable(parser.previous, can_assign);
}

static void function(bool can_assign, ObjString* name) {
  Compiler compiler;
  init_compiler(&compiler, TYPE_FUNCTION, name);
  begin_scope();

  if (!check(TOKEN_LAMBDA)) {
    do {
      current->function->arity++;
      if (current->function->arity > MAX_FN_ARGS) {
        error_at_current("Can't have more than MAX_FN_ARGS parameters.");
      }
      uint8_t constant = parse_variable("Expecting parameter name.");
      define_variable(constant);
    } while (match(TOKEN_COMMA));
  }

  consume(TOKEN_LAMBDA, "Expecting '->' after parameters.");

  statement();

  ObjFunction* function =
      end_compiler();  // Also handles end of scope. (end_scope())

  emit_bytes(OP_CLOSURE, make_constant(OBJ_VAL(function)));
  for (int i = 0; i < function->upvalue_count; i++) {
    emit_byte(compiler.upvalues[i].is_local ? 1 : 0);
    emit_byte(compiler.upvalues[i].index);
  }
}

static void anonymous_function(bool can_assign) {
  function(can_assign, copy_string("<Anon>", 9));
}

static void method() {
  consume(TOKEN_FN, "Expecting method initializer.");
  consume(TOKEN_ID, "Expecting method name.");
  uint8_t constant = string_constant(&parser.previous);
  ObjString* method_name = AS_STRING(
      current_chunk()
          ->constants.values[constant]);  // TODO: Optimize to directly get it
                                          // from string_constant()
  match(TOKEN_ASSIGN);                    // Just ignore it.
  function(true /* does not matter */, method_name);
  emit_bytes(OP_METHOD, constant);
}

static void and_(bool can_assign) {
  int end_jump = emit_jump(OP_JUMP_IF_FALSE);
  emit_byte(OP_POP);

  parse_precedence(PREC_AND);

  patch_jump(end_jump);
}

static void or_(bool can_assign) {
  int else_jump = emit_jump(OP_JUMP_IF_FALSE);
  int end_jump = emit_jump(OP_JUMP);

  patch_jump(else_jump);
  emit_byte(OP_POP);

  parse_precedence(PREC_OR);
  patch_jump(end_jump);
}

ParseRule rules[] = {
    [TOKEN_OPAR] = {grouping, call, PREC_CALL},
    [TOKEN_CPAR] = {NULL, NULL, PREC_NONE},
    [TOKEN_OBRACE] = {NULL, NULL, PREC_NONE},
    [TOKEN_CBRACE] = {NULL, NULL, PREC_NONE},
    [TOKEN_COMMA] = {NULL, NULL, PREC_NONE},
    [TOKEN_DOT] = {NULL, dot, PREC_CALL},
    [TOKEN_MINUS] = {unary, binary, PREC_TERM},
    [TOKEN_PLUS] = {NULL, binary, PREC_TERM},
    [TOKEN_DIV] = {NULL, binary, PREC_FACTOR},
    [TOKEN_MULT] = {NULL, binary, PREC_FACTOR},
    [TOKEN_NOT] = {unary, NULL, PREC_NONE},
    [TOKEN_NEQ] = {NULL, binary, PREC_EQUALITY},
    [TOKEN_ASSIGN] = {NULL, NULL, PREC_NONE},
    [TOKEN_EQ] = {NULL, binary, PREC_EQUALITY},
    [TOKEN_GT] = {NULL, binary, PREC_COMPARISON},
    [TOKEN_GTEQ] = {NULL, binary, PREC_COMPARISON},
    [TOKEN_LT] = {NULL, binary, PREC_COMPARISON},
    [TOKEN_LTEQ] = {NULL, binary, PREC_COMPARISON},
    [TOKEN_ID] = {variable, NULL, PREC_NONE},
    [TOKEN_STRING] = {string, NULL, PREC_NONE},
    [TOKEN_NUMBER] = {number, NULL, PREC_NONE},
    [TOKEN_AND] = {NULL, and_, PREC_AND},
    [TOKEN_CLASS] = {NULL, NULL, PREC_NONE},
    [TOKEN_ELSE] = {NULL, NULL, PREC_NONE},
    [TOKEN_FALSE] = {literal, NULL, PREC_NONE},
    [TOKEN_FOR] = {NULL, NULL, PREC_NONE},
    [TOKEN_FN] = {anonymous_function, NULL, PREC_NONE},
    [TOKEN_LAMBDA] = {NULL, NULL, PREC_NONE},
    [TOKEN_IF] = {NULL, NULL, PREC_NONE},
    [TOKEN_NIL] = {literal, NULL, PREC_NONE},
    [TOKEN_OR] = {NULL, or_, PREC_OR},
    [TOKEN_PRINT] = {NULL, NULL, PREC_NONE},
    [TOKEN_RETURN] = {NULL, NULL, PREC_NONE},
    [TOKEN_SUPER] = {NULL, NULL, PREC_NONE},
    [TOKEN_THIS] = {NULL, NULL, PREC_NONE},
    [TOKEN_TRUE] = {literal, NULL, PREC_NONE},
    [TOKEN_LET] = {NULL, NULL, PREC_NONE},
    [TOKEN_WHILE] = {NULL, NULL, PREC_NONE},
    [TOKEN_BREAK] = {NULL, NULL, PREC_NONE},
    [TOKEN_CONTINUE] = {NULL, NULL, PREC_NONE},
    [TOKEN_ERROR] = {NULL, NULL, PREC_NONE},
    [TOKEN_EOF] = {NULL, NULL, PREC_NONE},
};

static void parse_precedence(Precedence precedence) {
  advance();
  ParseFn prefix_rule = get_rule(parser.previous.type)->prefix;
  if (prefix_rule == NULL) {
    error("Expecting expression.");
    return;
  }

  bool can_assign = precedence <= PREC_ASSIGN;
  prefix_rule(can_assign);

  while (precedence <= get_rule(parser.current.type)->precedence) {
    advance();
    ParseFn infix_rule = get_rule(parser.previous.type)->infix;
    infix_rule(can_assign);
  }

  if (can_assign && match(TOKEN_ASSIGN)) {
    error("Invalid assignment target.");
  }
}

static uint8_t string_constant(Token* name) {
  return make_constant(OBJ_VAL(copy_string(name->start, name->length)));
}

static bool identifiers_equal(Token* a, Token* b) {
  if (a->length != b->length) {
    return false;
  }

  return memcmp(a->start, b->start, a->length) == 0;
}

static int resolve_local(Compiler* compiler, Token* name) {
  for (int i = compiler->local_count - 1; i >= 0; i--) {
    Local* local = &compiler->locals[i];
    if (identifiers_equal(name, &local->name)) {
      if (local->depth == -1) {
        error("Can't read local variable in its own initializer.");
      }
      return i;
    }
  }

  return -1;
}

static int resolve_upvalue(Compiler* compiler, Token* name) {
  if (compiler->enclosing == NULL)
    return -1;

  int local = resolve_local(compiler->enclosing, name);
  if (local != -1) {
    compiler->enclosing->locals[local].is_captured = true;
    return add_upvalue(compiler, (uint8_t)local, true);
  }

  // Recurse on outer scopes (compilers) to maybe find the variable there
  int upvalue = resolve_upvalue(compiler->enclosing, name);
  if (upvalue != -1) {
    return add_upvalue(compiler, (uint8_t)upvalue, false);
  }

  return -1;
}

static void add_local(Token name) {
  if (current->local_count == UINT8_COUNT) {
    error("Too many local variables in this scope.");
    return;
  }

  Local* local = &current->locals[current->local_count++];
  local->name = name;
  local->depth = -1;  // Means it is not initialized, bc it's value (rvalue) is
                      // not yet evaluated.
  local->is_captured = false;
}

static int add_upvalue(Compiler* compiler, uint8_t index, bool is_local) {
  int upvalue_count = compiler->function->upvalue_count;

  for (int i = 0; i < upvalue_count; i++) {
    Upvalue* upvalue = &compiler->upvalues[i];
    if (upvalue->index == index && upvalue->is_local == is_local) {
      return i;
    }
  }

  if (upvalue_count == UINT8_COUNT) {
    error("Too many closure variables in function.");
    return 0;
  }

  compiler->upvalues[upvalue_count].is_local = is_local;
  compiler->upvalues[upvalue_count].index = index;
  return compiler->function->upvalue_count++;
}

static void declare_local() {
  if (current->scope_depth == 0) {
    return;
  }

  Token* name = &parser.previous;

  for (int i = current->local_count - 1; i >= 0; i--) {
    Local* local = &current->locals[i];
    if (local->depth != -1 && local->depth < current->scope_depth) {
      break;
    }

    if (identifiers_equal(name, &local->name)) {
      error("Already a variable with this name in this scope.");
    }
  }
  add_local(*name);
}

static uint8_t parse_variable(const char* error_message) {
  consume(TOKEN_ID, error_message);

  declare_local();
  if (current->scope_depth > 0) {
    return 0;
  }

  return string_constant(&parser.previous);
}

static void mark_initialized() {
  if (current->scope_depth == 0) {
    return;
  }
  current->locals[current->local_count - 1].depth = current->scope_depth;
}

static void define_variable(uint8_t global) {
  if (current->scope_depth > 0) {
    mark_initialized();
    return;
  }

  emit_bytes(OP_DEFINE_GLOBAL, global);
}

static uint8_t argument_list() {
  uint8_t arg_count = 0;
  if (!check(TOKEN_CPAR)) {
    do {
      expression();
      if (arg_count == MAX_FN_ARGS) {
        error("Can't have more than MAX_FN_ARGS arguments.");
      }
      arg_count++;
    } while (match(TOKEN_COMMA));
  }
  consume(TOKEN_CPAR, "Expecting ')' after arguments.");
  return arg_count;
}

static ParseRule* get_rule(TokenType type) {
  return &rules[type];
}

static void synchronize() {
  parser.panic_mode = false;

  while (parser.current.type != TOKEN_EOF) {
    switch (parser.current.type) {
      case TOKEN_CLASS:
      case TOKEN_FN:
      case TOKEN_LET:
      case TOKEN_FOR:
      case TOKEN_IF:
      case TOKEN_WHILE:
      case TOKEN_PRINT:
      case TOKEN_RETURN:
        return;

      default:;  // Do nothing.
    }

    advance();
  }
}

static void expression() {
  parse_precedence(PREC_ASSIGN);
}

static void statement_declaration_let() {
  uint8_t global = parse_variable("Expecting variable name.");

  if (match(TOKEN_ASSIGN)) {
    expression();
  } else {
    emit_byte(OP_NIL);
  }

  define_variable(global);
}

static void statement_declaration_function() {
  uint8_t global = parse_variable("Expecting variable name.");
  ObjString* fn_name = AS_STRING(
      current_chunk()
          ->constants.values[global]);  // TODO: Optimize to directly get it
                                        // from string_constant()
  match(TOKEN_ASSIGN);                  // Just ignore it.
  function(false /* can't assign */, fn_name);
  define_variable(global);
}

static void statement_declaration_class() {
  consume(TOKEN_ID, "Expect class name.");
  Token class_name = parser.previous;
  uint8_t name_constant = string_constant(&parser.previous);
  declare_local();

  emit_bytes(OP_CLASS, name_constant);
  define_variable(name_constant);

  named_variable(class_name, false);
  consume(TOKEN_OBRACE, "Expecting'{' before class body.");
  while (!check(TOKEN_CBRACE) && !check(TOKEN_EOF)) {
    method();
  }
  consume(TOKEN_CBRACE, "Expecting'}' after class body.");
  emit_byte(OP_POP);
}

static void statement_print() {
  expression();
  emit_byte(OP_PRINT);
}

static void statement_expression() {
  expression();
  emit_byte(OP_POP);
}

static void statement_if() {
  expression();

  int then_jump = emit_jump(OP_JUMP_IF_FALSE);
  emit_byte(OP_POP);

  statement();

  int else_jump = emit_jump(OP_JUMP);

  patch_jump(then_jump);
  emit_byte(OP_POP);

  if (match(TOKEN_ELSE)) {
    statement();
  }

  patch_jump(else_jump);
}

static void statement_return() {
  if (current->type == TYPE_TOPLEVEL) {
    error("Can't return from top-level code.");
  }

  // TODO: We don't want semicolons after return statements - but it's almost
  // impossible to get rid of them here Since there literally could come
  // anything after a return statement.
  if (match(TOKEN_SCOLON)) {
    emit_return();
  } else {
    expression();
    consume(TOKEN_SCOLON, "Expecting ';' after return value.");
    emit_byte(OP_RETURN);
  }
}

static void statement_while() {
  int loop_start = current_chunk()->count;

  expression();

  int exit_jump = emit_jump(OP_JUMP_IF_FALSE);
  emit_byte(OP_POP);

  statement();
  emit_loop(loop_start);

  patch_jump(exit_jump);
  emit_byte(OP_POP);
}

static void statement_for() {
  begin_scope();

  if (match(TOKEN_LET)) {
    statement_declaration_let();
  } else {
    statement_expression();
  }

  int loop_start = current_chunk()->count;

  int exit_jump = -1;
  if (match(TOKEN_SCOLON)) {
    expression();

    // Jump out of the loop if the condition is false.
    exit_jump = emit_jump(OP_JUMP_IF_FALSE);
    emit_byte(OP_POP);  // Discard the result of the condition expression.
  }

  if (match(TOKEN_SCOLON)) {
    int body_jump = emit_jump(OP_JUMP);
    int incrementStart = current_chunk()->count;
    expression();
    emit_byte(OP_POP);  // Discard the result of the increment expression.

    emit_loop(loop_start);
    loop_start = incrementStart;
    patch_jump(body_jump);
  }

  statement();
  emit_loop(loop_start);

  if (exit_jump != -1) {
    patch_jump(exit_jump);
    emit_byte(OP_POP);  // Discard the result of the condition expression.
  }

  end_scope();
}

static void block() {
  while (!check(TOKEN_CBRACE) && !check(TOKEN_EOF)) {
    statement();
  }

  consume(TOKEN_CBRACE, "Expecting '}' after block.");
}

static void statement() {
  if (match(TOKEN_PRINT)) {
    statement_print();
  } else if (match(TOKEN_CLASS)) {
    statement_declaration_class();
  } else if (match(TOKEN_IF)) {
    statement_if();
  } else if (match(TOKEN_RETURN)) {
    statement_return();
  } else if (match(TOKEN_WHILE)) {
    statement_while();
  } else if (match(TOKEN_FOR)) {
    statement_for();
  } else if (match(TOKEN_OBRACE)) {
    begin_scope();
    block();
    end_scope();
  } else if (match(TOKEN_FN)) {
    statement_declaration_function();
  } else if (match(TOKEN_LET)) {
    statement_declaration_let();
  } else {
    statement_expression();
  }

  if (parser.panic_mode) {
    synchronize();
  }
}

ObjFunction* compile(const char* source) {
  init_scanner(source);
  Compiler compiler;
  init_compiler(&compiler, TYPE_TOPLEVEL, NULL);

#ifdef DEBUG_PRINT_CODE
  printf("== Begin compilation ==\n");
#endif

  parser.had_error = false;
  parser.panic_mode = false;

  advance();

  while (!match(TOKEN_EOF)) {
    statement();
  }

  ObjFunction* function = end_compiler();
  return parser.had_error ? NULL : function;
}

void mark_compiler_roots() {
  Compiler* compiler = current;
  while (compiler != NULL) {
    mark_obj((Obj*)compiler->function);
    compiler = compiler->enclosing;
  }
}
