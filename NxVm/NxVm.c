#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "chunk.h"
#include "common.h"
#include "debug.h"
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

static char* read_file(const wchar_t* path) {
  FILE* file = _wfopen(path, L"rb");
  if (file == NULL) {
    WINTERNAL_ERROR(L"Could not open file \"%s\"", path);
    exit(74);
  }

  fseek(file, 0L, SEEK_END);
  size_t file_size = ftell(file);
  rewind(file);

  char* buffer = (char*)malloc(file_size + 1);
  if (buffer == NULL) {
    WINTERNAL_ERROR(L"Not enough memory to read \"%s\"\n", path);
    exit(74);
  }

  size_t bytes_read = fread(buffer, sizeof(char), file_size, file);
  if (bytes_read < file_size) {
    WINTERNAL_ERROR(L"Could not read file \"%s\"\n", path);
    exit(74);
  }

  buffer[bytes_read] = '\0';

  fclose(file);
  return buffer;
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

// TODO: Move to test.c
static InterpretResult run_test(const wchar_t* path) {
  init_vm();

  char* source = read_file(path);

  // Redirect stdout to a file
  wchar_t wide_file_path[MAX_PATH];
  _stprintf(wide_file_path, _T("%s.out"), path);

  char narrow_file_path[MAX_PATH];
  WideCharToMultiByte(CP_UTF8, 0, wide_file_path, -1, narrow_file_path,
                      MAX_PATH, NULL, NULL);

  FILE* stream = freopen(narrow_file_path, "w", stdout);
  if (stream == NULL) {
    INTERNAL_ERROR("Could not redirect stdout to file");
    return 1;
  }

  // Run the test
  InterpretResult result = interpret(source);

  free(source);

  free_vm();

  // Restore stdout
  fclose(stream);
  freopen("CONOUT$", "w", stdout);

  return result;
}

// TODO: Move to test.c
static void run_tests(const wchar_t* path) {
  wchar_t* test_file_paths[100];   // Found test-file-paths. (*.spec.nx)
  int count = 0;                   // Number of files found
  int max_files = MAX_SPEC_FILES;  // Maximum number of files to find

  for (int i = 0; i < max_files; i++) {
    test_file_paths[i] = (wchar_t*)malloc(MAX_PATH * sizeof(wchar_t));
  }

  // Scan the directory
  scan_tests_dir(path, test_file_paths, &count, max_files);

  if (count == 0) {
    WINTERNAL_ERROR(L"No test files found in \"%s\"", path);
    exit(74);
  }

  if (count >= max_files) {
    WINTERNAL_ERROR(L"Limit of %d test files reached",
                    max_files);  // TODO: Make warning instead of error
    exit(74);
  }

  printf("Running %d tests\n", count);

  // Runner
  int failed = 0;
  for (int i = 0; i < count; i++) {
    wprintf(L"    - running %s ", test_file_paths[i]);

    InterpretResult result = run_test(test_file_paths[i]);

    switch (result) {
      case INTERPRET_OK:
        printf(ANSI_GREEN_STR("Ok\n"));
        break;
      case INTERPRET_COMPILE_ERROR:
        failed++;
        printf(ANSI_RED_STR("Compile Error\n"));
        break;
      case INTERPRET_RUNTIME_ERROR:
        failed++;
        printf(ANSI_RED_STR("Runtime Error\n"));
        break;
      default:
        printf(ANSI_RED_STR("INTERNAL ERROR\n"));
        INTERNAL_ERROR("Unhandled InterpretResult");
        break;
    }

    free(test_file_paths[i]);
  }

  printf("Result: %d/%d tests passed\n", count - failed, count);
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
  argv[1] = L"test";
  argv[2] = L"C:\\Projects\\nx-script\\NxVm\\test";

  // argv[1] = L"run";
  // argv[2] = L"C:\\Projects\\nx-script\\NxVm\\script_sample.nx";

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
