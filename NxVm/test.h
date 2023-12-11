#ifndef test_h
#define test_h

#include <fcntl.h>
#include <io.h>
#include <stdio.h>
#include <string.h>
#include <tchar.h>
#include <windows.h>
#include "common.h"

#define MAX_SPEC_FILES 100

char* read_file(const wchar_t* path);
void run_tests(const wchar_t* path);

#endif