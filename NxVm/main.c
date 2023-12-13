#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "common.h"
#include "test.h"
#include "vm.h"

static void repl() {
  init_vm();

  // TODO: Improve this, don't just use a fixed line length - lol
  char line[1024];
  for (;;) {
    printf("nxs > ");

    if (!fgets(line, sizeof(line), stdin)) {
      printf("\n");
      break;
    }

    interpret(line);
  }

  free_vm();
}

static void run_file(const wchar_t* path) {
  init_vm();

  char* source = read_file(path);
  InterpretResult result = interpret(source);
  free(source);

  switch (result) {
    case INTERPRET_OK:
      break;
    case INTERPRET_COMPILE_ERROR:
      exit(65);
      break;
    case INTERPRET_RUNTIME_ERROR:
      exit(70);
      break;
    default:
      printf(ANSI_RED_STR("INTERNAL ERROR\n"));
      INTERNAL_ERROR("Unhandled InterpretResult");
      break;
  }

  free_vm();
}

void usage() {
  printf("Usage: nx <args>\n");
  printf("  run  <path> Run script at <path>\n");
  printf("  test <path> Run tests at <path> entry dir\n");
  printf("  repl        Run REPL\n");
  exit(64);
}

int wmain(int argc, wchar_t* argv[]) {
  // TODO: Remove this
  argc = 3;
  // argv[1] = L"test";
  // argv[2] = L"C:\\Projects\\nx-script\\NxVm\\test";
  argv[1] = L"run";
  argv[2] = L"C:\\Projects\\nx-script\\NxVm\\script_sample.nx";

  if (argc == 2 && wcscmp(argv[1], L"repl") == 0) {
    repl();
  } else if (argc == 3 && wcscmp(argv[1], L"test") == 0) {
    run_tests(argv[2]);
  } else if (argc == 3 && wcscmp(argv[1], L"run") == 0) {
    run_file(argv[2]);
  } else {
    usage();
  }

  return 0;
}
