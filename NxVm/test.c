#include "test.h"
#include "vm.h"

// Utility function to check if a file has the specified extension
bool has_extension(const wchar_t* file_name, const wchar_t* extension) {
  size_t len = _tcslen(file_name);
  size_t extLen = _tcslen(extension);
  return len > extLen && _tcscmp(file_name + len - extLen, extension) == 0;
}

// Utility function to check if a file exists
bool file_exists(const wchar_t* file_path) {
  FILE* file = _wfopen(file_path, L"rb");
  if (file == NULL) {
    return false;
  }
  fclose(file);
  return true;
}

// Recursive function to scan directory
void scan_tests_dir(const wchar_t* path,
                    wchar_t** file_paths,
                    int* count,
                    int max_files) {
  wchar_t search_path[MAX_PATH];
  WIN32_FIND_DATA find_file_data;
  HANDLE find_handle = INVALID_HANDLE_VALUE;

  // Prepare the path for search
  _stprintf(search_path, _T("%s\\*"), path);

  // Find the first file in the directory.
  find_handle = FindFirstFile(search_path, &find_file_data);

  if (find_handle == INVALID_HANDLE_VALUE) {
    return;  // Directory not found or empty
  }

  do {
    if (find_file_data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
      // Found a directory, not a file
      if (_tcscmp(find_file_data.cFileName, _T(".")) != 0 &&
          _tcscmp(find_file_data.cFileName, _T("..")) != 0) {
        // Construct new path and recursively call this function
        wchar_t new_path[MAX_PATH];
        _stprintf(new_path, _T("%s\\%s"), path, find_file_data.cFileName);
        scan_tests_dir(new_path, file_paths, count, max_files);
      }
    } else {
      // Check if file matches the criteria
      if (has_extension(find_file_data.cFileName, _T(".spec.nx"))) {
        // Check for corresponding .expect.nx file
        wchar_t expect_filename[MAX_PATH];
        _stprintf(expect_filename, _T("%s.expect"), find_file_data.cFileName);

        wchar_t expect_filepath[MAX_PATH];
        _stprintf(expect_filepath, _T("%s\\%s"), path, expect_filename);

        if (file_exists(expect_filepath)) {
          // Store the .spec.nx file path
          wchar_t full_path[MAX_PATH];
          _stprintf(full_path, _T("%s\\%s"), path, find_file_data.cFileName);

          if (*count < max_files) {
            _tcscpy(file_paths[*count], full_path);
            (*count)++;
          }
        } else {
          WINTERNAL_ERROR(
              L"Could not find corresponding .expect.nx file for \"%s\". "
              L"Ignoring test.",
              find_file_data.cFileName);
        }
      }
    }
  } while (FindNextFile(find_handle, &find_file_data) != 0 &&
           *count < max_files);

  FindClose(find_handle);
}

// Utility function to read a file
char* read_file(const wchar_t* path) {
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

// Utility to compare two strings line by line
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

// Utility to run a test
bool run_test(const wchar_t* path) {
  init_vm();

  char* source = read_file(path);

  // Create output file path based on file path
  wchar_t wout_file_path[MAX_PATH];
  _stprintf(wout_file_path, _T("%s.out"), path);

  char out_file_path[MAX_PATH];
  WideCharToMultiByte(CP_UTF8, 0, wout_file_path, -1, out_file_path, MAX_PATH,
                      NULL, NULL);

  // Redirect stdout to a file
  FILE* stream = freopen(out_file_path, "w", stdout);
  if (stream == NULL) {
    INTERNAL_ERROR("Could not redirect stdout to file");
    exit(70);
  }

  // Redirect stderr to the same file
  if (_dup2(_fileno(stream), _fileno(stderr)) != 0) {
    INTERNAL_ERROR("Could not redirect stderr to file");
    exit(70);
  }

  // Run the test
  InterpretResult result = interpret(source);

  free(source);
  free_vm();

  // Restore stdout
  fclose(stream);
  FILE* stdout_ = freopen("CONOUT$", "w", stdout);
  if (stdout_ == NULL) {
    INTERNAL_ERROR("Could not restore stdout");
    exit(70);
  }

  if (_dup2(_fileno(stdout_), _fileno(stderr)) != 0) {
    INTERNAL_ERROR("Could not restore stderr");
    exit(70);
  }

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

// Utility to run all tests in a directory
void run_tests(const wchar_t* path) {
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