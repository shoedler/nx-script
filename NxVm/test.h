#ifndef test_h
#define test_h

#include <stdio.h>
#include <string.h>
#include <tchar.h>
#include <windows.h>
#include "common.h"

#define MAX_SPEC_FILES 100

void scan_tests_dir(const wchar_t* path,
                    wchar_t** file_paths,
                    int* count,
                    int max_files);

#endif