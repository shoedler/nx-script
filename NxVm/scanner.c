#include <stdio.h>
#include <string.h>

#include "common.h"
#include "scanner.h"

typedef struct {
	const char* start;
	const char* current;
	int line;
} Scanner;

Scanner scanner;

void init_scanner(const char* source) {
	scanner.start = source;
	scanner.current = source;
	scanner.line = 1;
}

static bool is_digit(char c) {
  return c >= '0' && c <= '9';
}

static bool is_alpha(char c) {
	return (c >= 'a' && c <= 'z') ||
				 (c >= 'A' && c <= 'Z') ||
				 c == '_';
}

static bool is_at_end() {
	return *scanner.current == '\0';
}

static char advance() {
	scanner.current++;
	return scanner.current[-1];
}

static char peek() {
  return *scanner.current;
}

static char peek_next() {
  if (is_at_end()) {
		return '\0';
	}
  return scanner.current[1];
}

static bool match(char expected) {
  if (is_at_end()) {
		return false;
	}

  if (*scanner.current != expected) {
		return false;
	}
  scanner.current++;
  return true;
}

static Token make_token(TokenType type) {
	Token token;
	token.type = type;
	token.start = scanner.start;
	token.length = (int)(scanner.current - scanner.start);
	token.line = scanner.line;
	return token;
}

static Token error_token(const char* message) {
	Token token;
	token.type = TOKEN_ERROR;
	token.start = message;
	token.length = (int)strlen(message);
	token.line = scanner.line;
	return token;
}

static void skip_whitespace() {
  for (;;) {
    char c = peek();
    switch (c) {
      case ' ':
      case '\r':
      case '\t':
        advance();
        break;
      case '\n':
        scanner.line++;
        advance();
        break;
			case '/':
        if (peek_next() == '/') {
          // A comment goes until the end of the line.
          while (peek() != '\n' && !is_at_end()) {
						advance();
					}
        } else {
          return;
        }
        break;
      default:
        return;
    }
  }
}

static TokenType check_keyword(int start, int length, const char* rest, TokenType type) {
  if (scanner.current - scanner.start == start + length &&
      memcmp(scanner.start + start, rest, length) == 0) {
    return type;
  }

  return TOKEN_ID;
}

static TokenType identifier_type() {
	switch (scanner.start[0]) {
    case 'a': return check_keyword(1, 2, "nd", TOKEN_AND);
		case 'b': return check_keyword(1, 4, "reak", TOKEN_BREAK);
    case 'c': {
			if (scanner.current - scanner.start > 1) {
				switch (scanner.start[1]) {
					case 'l': return check_keyword(1, 3, "ass", TOKEN_CLASS);
					case 'o': return check_keyword(1, 6, "ntinue", TOKEN_CONTINUE);
				}
			}
		}
    case 'e': return check_keyword(1, 3, "lse", TOKEN_ELSE);
		case 'f':
      if (scanner.current - scanner.start > 1) {
        switch (scanner.start[1]) {
          case 'a': return check_keyword(2, 3, "lse", TOKEN_FALSE);
          case 'o': return check_keyword(2, 1, "r", TOKEN_FOR);
          case 'n': return TOKEN_FN;
        }
      }
      break;
    case 'i': return check_keyword(1, 1, "f", TOKEN_IF);
    case 'n': return check_keyword(1, 2, "il", TOKEN_NIL);
    case 'o': return check_keyword(1, 1, "r", TOKEN_OR);
    case 'p': return check_keyword(1, 4, "rint", TOKEN_PRINT);
    case 'r': return check_keyword(1, 2, "et", TOKEN_RETURN);
    case 's': return check_keyword(1, 4, "uper", TOKEN_SUPER);
    case 't':
      if (scanner.current - scanner.start > 1) {
        switch (scanner.start[1]) {
          case 'h': return check_keyword(2, 2, "is", TOKEN_THIS);
          case 'r': return check_keyword(2, 2, "ue", TOKEN_TRUE);
        }
      }
      break;
    case 'l': return check_keyword(1, 2, "et", TOKEN_LET);
    case 'w': return check_keyword(1, 4, "hile", TOKEN_WHILE);
  }

  return TOKEN_ID;
}

static Token identifier() {
	while (is_alpha(peek()) || is_digit(peek())) {
		advance();
	}

	return make_token(identifier_type());
}

static Token number() {
  while (is_digit(peek())) {
		advance();
  }

  // Look for a fractional part.
  if (peek() == '.' && is_digit(peek_next())) {
    advance(); // Consume the ".".

    while (is_digit(peek())) {
			advance();
		}
  }

  return make_token(TOKEN_NUMBER);
}

static Token string() {
  while (peek() != '"' && !is_at_end()) {
    if (peek() == '\n') {
			scanner.line++;
		}
    advance();
  }

  if (is_at_end()) {
		return error_token("Unterminated string.");
	}

  advance(); // Consume the closing ".
  return make_token(TOKEN_STRING);
}

Token scan_token() {
	skip_whitespace();
	scanner.start = scanner.current;

	if (is_at_end()) {
		return make_token(TOKEN_EOF);
	}

	char c = advance();

	if (is_digit(c)) {
		return number();
	}

	if (is_alpha(c)) {
		return identifier();
	}

	switch (c) {
		case '(': return make_token(TOKEN_OPAR);
		case ')': return make_token(TOKEN_CPAR);
		case '{': return make_token(TOKEN_OBRACE);
		case '}': return make_token(TOKEN_CBRACE);
		case '[': return make_token(TOKEN_OBRACK);
		case ']': return make_token(TOKEN_CBRACK);
		case '.': return make_token(TOKEN_DOT);
		case ':': return make_token(TOKEN_COLON);
		case ',': return make_token(TOKEN_COMMA);
		case '+': return make_token(TOKEN_PLUS);
		case '/': return make_token(TOKEN_DIV);
		case '*': return make_token(TOKEN_MULT);
		case '%': return make_token(TOKEN_MOD);
		case '^': return make_token(TOKEN_POW);

		case '-': return make_token( match('>') ? TOKEN_LAMBDA : TOKEN_MINUS);
		case '=': return make_token( match('=') ? TOKEN_EQ : TOKEN_ASSIGN);
		case '!': return make_token( match('=') ? TOKEN_NEQ : TOKEN_NOT);
		case '<': return make_token( match('=') ? TOKEN_LTEQ : TOKEN_LT);
		case '>': return make_token( match('=') ? TOKEN_GTEQ : TOKEN_GT);

		case '"': return string();
	}

	return error_token("Unexpected character.");
}

