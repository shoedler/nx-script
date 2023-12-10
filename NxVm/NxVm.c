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
const char** compare_strings_by_line(const char* a,
                                     const char* b,
                                     int* num_diff) {
  // Temporary variables for processing
  char* temp_a = strdup(a);
  char* temp_b = strdup(b);
  char* line_a;
  char* line_b;
  char* context_a = NULL;
  char* context_b = NULL;

  // Count the number of lines in a for allocating the array
  int line_count_a = 0;
  for (int i = 0; temp_a[i]; i++) {
    if (temp_a[i] == '\n') {
      line_count_a++;
    }
  }

  const char** diff = malloc(sizeof(char*) * (line_count_a + 1));
  if (diff == NULL) {
    INTERNAL_ERROR("Not enough memory to allocate diff array");
    exit(74);
  }

  // Start tokenizing and comparing
  int diff_count = 0;
  line_a = strtok_s(temp_a, "\r\n", &context_a);
  line_b = strtok_s(temp_b, "\r\n", &context_b);
  int line_no = 0;

  // TODO: Add warning if b has more lines than a
  // TODO: Add warning a has no lines

  while (line_a != NULL) {
    line_no++;
    // If line_b is NULL, it means b has fewer lines than a
    if (line_b == NULL) {
      break;
    }

    if (strcmp(line_a, line_b) != 0) {
      // Lines are different, format the string and add to diff
      char* diff_line = malloc(
          1024);  // Allocating a fixed size, should be enough for most lines
      if (diff_line == NULL) {
        INTERNAL_ERROR("Not enough memory to allocate diff line");
        exit(74);
      }
      sprintf(diff_line,
              "on line %d " ANSI_GREEN_STR("%s") " was " ANSI_RED_STR("%s"),
              line_no, line_a, line_b);
      diff[diff_count++] = diff_line;
    }

    // Move to the next line
    line_a = strtok_s(NULL, "\r\n", &context_a);
    line_b = strtok_s(NULL, "\r\n", &context_b);
  }

  // Finalize the array
  diff[diff_count] = NULL;
  *num_diff = diff_count;

  // Clean up
  free(temp_a);
  free(temp_b);

  return diff;
}

// TODO: Move to test.c
static bool run_test(const wchar_t* path) {
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

  // Compare output with expect
  wchar_t expect_filepath[MAX_PATH];
  _stprintf(expect_filepath, _T("%s%s"), path, L".expect");
  wchar_t* out_filepath[MAX_PATH];
  _stprintf(out_filepath, _T("%s%s"), path, L".out");

  char* expect = read_file(expect_filepath);
  char* out = read_file(out_filepath);

  bool passed = true;
  int num_diff = 0;

  const char** differences = compare_strings_by_line(expect, out, &num_diff);
  if (num_diff > 0) {
    passed = false;
    printf(ANSI_RED_STR("Failed: %d lines differ!\n"), num_diff);
    for (int i = 0; i < num_diff; i++) {
      printf("        #%d: %s", i + 1, differences[i]);
      wprintf(L" (in %s)\n", out_filepath);
      free((void*)differences[i]);
    }
  }

  free(expect);
  free(out);
  free(differences);

  if (!passed) {
    return passed;
  }

  switch (result) {
    case INTERPRET_OK:
      printf(ANSI_GREEN_STR("Passed!\n"));
      break;
    case INTERPRET_COMPILE_ERROR:
      passed = false;
      printf(ANSI_RED_STR("Failed: Compile Error!\n"));
      break;
    case INTERPRET_RUNTIME_ERROR:
      passed = false;
      printf(ANSI_RED_STR("Failed: Runtime Error!\n"));
      break;
    default:
      printf(ANSI_RED_STR("Failed: Internal Error!\n"));
      INTERNAL_ERROR("Unhandled InterpretResult");
      break;
  }

  return passed;
}

// TODO: Move to test.c
static void run_tests(const wchar_t* path) {
  wchar_t* test_file_paths[100];   // Found test-file-paths. (*.spec.nx)
  int count = 0;                   // Number of files found
  int max_files = MAX_SPEC_FILES;  // Maximum number of files to find

  for (int i = 0; i < max_files; i++) {
    test_file_paths[i] = (wchar_t*)malloc(MAX_PATH * sizeof(wchar_t));
  }

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
    bool result = run_test(test_file_paths[i]);
    if (!result) {
      failed++;
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
