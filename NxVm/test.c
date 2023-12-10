#include "test.h"

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
