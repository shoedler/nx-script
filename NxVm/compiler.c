#include <stdio.h>
#include <stdlib.h>

#include "common.h"
#include "compiler.h"
#include "scanner.h"

#ifdef DEBUG_PRINT_CODE
#include "debug.h"
#endif

typedef struct
{
    Token current;
    Token previous;
    bool had_error;
    bool panic_mode;
} Parser;

typedef enum
{
    PREC_NONE,
    PREC_ASSIGN,     // =
    PREC_OR,         // or
    PREC_AND,        // and
    PREC_EQUALITY,   // == !=
    PREC_COMPARISON, // < > <= >=
    PREC_TERM,       // + -
    PREC_FACTOR,     // * / %
    PREC_UNARY,      // ! -
    PREC_CALL,       // . ()
    PREC_PRIMARY
} Precedence;

typedef void (*ParseFn)();

typedef struct
{
    ParseFn prefix;
    ParseFn infix;
    Precedence precedence;
} ParseRule;

Parser parser;
Chunk *compiling_chunk;

static Chunk *current_chunk()
{
    return compiling_chunk;
}

static void error_at(Token *token, const char *message)
{
    if (parser.panic_mode)
    {
        return;
    }

    parser.panic_mode = true;
    fprintf(stderr, "[line %d] ERROR", token->line);

    if (token->type == TOKEN_EOF)
    {
        fprintf(stderr, " at end");
    }
    else if (token->type == TOKEN_ERROR)
    {
        // Nothing.
    }
    else
    {
        fprintf(stderr, " at '%.*s'", token->length, token->start);
    }

    fprintf(stderr, ": %s\n", message);
    parser.had_error = true;
}

static void error(const char *message)
{
    error_at(&parser.previous, message);
}

static void error_at_current(const char *message)
{
    error_at(&parser.current, message);
}

static void advance()
{
    parser.previous = parser.current;

    for (;;)
    {
        parser.current = scan_token();

        if (parser.current.type != TOKEN_ERROR)
        {
            break;
        }

        error_at_current(parser.current.start);
    }
}

static void consume(TokenType type, const char *message)
{
    if (parser.current.type == type)
    {
        advance();
        return;
    }

    error_at_current(message);
}

static void emit_byte(uint8_t byte)
{
    write_chunk(current_chunk(), byte, parser.previous.line);
}

static void emit_bytes(uint8_t byte1, uint8_t byte2)
{
    emit_byte(byte1);
    emit_byte(byte2);
}

static uint8_t make_constant(Value value)
{
    int constant = add_constant(current_chunk(), value);
    if (constant > UINT8_MAX)
    {
        error("Too many constants in one chunk.");
        return 0;
    }

    return (uint8_t)constant;
}

static void emit_constant(Value value)
{
    emit_bytes(OP_CONSTANT, make_constant(value));
}

static void emit_return()
{
    emit_byte(OP_RETURN);
}

static void end_compiler()
{
    emit_return();
#ifdef DEBUG_PRINT_CODE
    if (!parser.had_error)
    {
        disassemble_chunk(current_chunk(), "code");
    }
#endif
}

static void expression();
static ParseRule *get_rule(TokenType type);
static void parse_precedence(Precedence precedence);

static void binary()
{
    TokenType op_type = parser.previous.type;
    ParseRule *rule = get_rule(op_type);
    parse_precedence((Precedence)(rule->precedence + 1));

    switch (op_type)
    {
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

static void grouping()
{
    expression();
    consume(TOKEN_CPAR, "Expected ')' after expression.");
}

static void number()
{
    double value = strtod(parser.previous.start, NULL);
    emit_constant(value);
}

static void unary()
{
    TokenType operator_type = parser.previous.type;

    // Compile the operand.
    expression();

    // Emit the operator instruction.
    switch (operator_type)
    {
    case TOKEN_MINUS:
        emit_byte(OP_NEGATE);
        break;
    default:
        INTERNAL_ERROR("Unhandled unary operator type: %d", operator_type);
        return;
    }
}

ParseRule rules[] = {
    [TOKEN_OPAR] = {grouping, NULL, PREC_NONE},
    [TOKEN_CPAR] = {NULL, NULL, PREC_NONE},
    [TOKEN_OBRACE] = {NULL, NULL, PREC_NONE},
    [TOKEN_CBRACE] = {NULL, NULL, PREC_NONE},
    [TOKEN_COMMA] = {NULL, NULL, PREC_NONE},
    [TOKEN_DOT] = {NULL, NULL, PREC_NONE},
    [TOKEN_MINUS] = {unary, binary, PREC_TERM},
    [TOKEN_PLUS] = {NULL, binary, PREC_TERM},
    [TOKEN_DIV] = {NULL, binary, PREC_FACTOR},
    [TOKEN_MULT] = {NULL, binary, PREC_FACTOR},
    [TOKEN_NEQ] = {NULL, NULL, PREC_NONE},
    [TOKEN_ASSIGN] = {NULL, NULL, PREC_NONE},
    [TOKEN_EQ] = {NULL, NULL, PREC_NONE},
    [TOKEN_GT] = {NULL, NULL, PREC_NONE},
    [TOKEN_GTEQ] = {NULL, NULL, PREC_NONE},
    [TOKEN_LT] = {NULL, NULL, PREC_NONE},
    [TOKEN_LTEQ] = {NULL, NULL, PREC_NONE},
    [TOKEN_ID] = {NULL, NULL, PREC_NONE},
    [TOKEN_STRING] = {NULL, NULL, PREC_NONE},
    [TOKEN_NUMBER] = {number, NULL, PREC_NONE},
    [TOKEN_AND] = {NULL, NULL, PREC_NONE},
    [TOKEN_CLASS] = {NULL, NULL, PREC_NONE},
    [TOKEN_ELSE] = {NULL, NULL, PREC_NONE},
    [TOKEN_FALSE] = {NULL, NULL, PREC_NONE},
    [TOKEN_FOR] = {NULL, NULL, PREC_NONE},
    [TOKEN_FN] = {NULL, NULL, PREC_NONE},
    [TOKEN_LAMBDA] = {NULL, NULL, PREC_NONE},
    [TOKEN_IF] = {NULL, NULL, PREC_NONE},
    [TOKEN_NIL] = {NULL, NULL, PREC_NONE},
    [TOKEN_OR] = {NULL, NULL, PREC_NONE},
    [TOKEN_PRINT] = {NULL, NULL, PREC_NONE},
    [TOKEN_RETURN] = {NULL, NULL, PREC_NONE},
    [TOKEN_SUPER] = {NULL, NULL, PREC_NONE},
    [TOKEN_THIS] = {NULL, NULL, PREC_NONE},
    [TOKEN_TRUE] = {NULL, NULL, PREC_NONE},
    [TOKEN_LET] = {NULL, NULL, PREC_NONE},
    [TOKEN_WHILE] = {NULL, NULL, PREC_NONE},
    [TOKEN_BREAK] = {NULL, NULL, PREC_NONE},
    [TOKEN_CONTINUE] = {NULL, NULL, PREC_NONE},
    [TOKEN_ERROR] = {NULL, NULL, PREC_NONE},
    [TOKEN_EOF] = {NULL, NULL, PREC_NONE},
};

static void parse_precedence(Precedence precedence)
{
    advance();
    ParseFn prefix_rule = get_rule(parser.previous.type)->prefix;
    if (prefix_rule == NULL)
    {
        error("Expected expression.");
        return;
    }

    prefix_rule();

    while (precedence <= get_rule(parser.current.type)->precedence)
    {
        advance();
        ParseFn infix_rule = get_rule(parser.previous.type)->infix;
        infix_rule();
    }
}

static ParseRule *get_rule(TokenType type)
{
    return &rules[type];
}

static void expression()
{
    parse_precedence(PREC_ASSIGN);
}

bool compile(const char *source, Chunk *chunk)
{
    init_scanner(source);
    compiling_chunk = chunk;

    parser.had_error = false;
    parser.panic_mode = false;

    advance();
    expression();
    consume(TOKEN_EOF, "Expected end of expression.");
    end_compiler();

    return !parser.had_error;
    // int line = -1;
    // for (;;)
    // {
    //     Token token = scan_token();

    //     if (token.line != line)
    //     {
    //         printf("%4d ", token.line);
    //         line = token.line;
    //     }
    //     else
    //     {
    //         printf("   | ");
    //     }

    //     printf("%2d '%.*s'\n", token.type, token.length, token.start);

    //     if (token.type == TOKEN_EOF)
    //     {
    //         break;
    //     }

    // }
}