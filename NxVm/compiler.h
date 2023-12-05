#ifndef nxs_compiler_h
#define nxs_compiler_h

#include "object.h"
#include "vm.h"

#define MAX_FN_ARGS 255

ObjFunction* compile(const char* source);

#endif