#ifndef common_h
#define common_h

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

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

#define PRINT_ERROR_HEADER(type)                                            \
  fprintf(stderr, ##ANSI_RED_STR(type) " at "##ANSI_YELLOW_STR("%s(%u): "), \
          __FILE__, __LINE__);

#define INTERNAL_ERROR(format_literal, ...)              \
  do {                                                   \
    ##PRINT_ERROR_HEADER("INTERNAL ERROR");              \
    fprintf(stderr, format_literal "\n", ##__VA_ARGS__); \
  } while (0)

#define WINTERNAL_ERROR(wformat_literal, ...)               \
  do {                                                      \
    ##PRINT_ERROR_HEADER("INTERNAL ERROR");                 \
    fwprintf(stderr, wformat_literal L"\n", ##__VA_ARGS__); \
  } while (0)

#define NOT_IMPLEMENTED(what)                       \
  do {                                              \
    ##PRINT_ERROR_HEADER("NOT IMPLEMENTED ERROR");  \
    fprintf(stderr, "Not implemented: " what "\n"); \
    exit(1);                                        \
  } while (0)

// #define DEBUG_PRINT_TOKENS
// #define DEBUG_TRACE_EXECUTION
// #define DEBUG_PRINT_CODE

// #define DEBUG_STRESS_GC
// #define DEBUG_LOG_GC
// #define DEBUG_LOG_GC_ALLOCATIONS

#define UINT8_COUNT (UINT8_MAX + 1)

#endif
