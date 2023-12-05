#ifndef nxs_compiler_h
#define nxs_compiler_h

#include "object.h"
#include "vm.h"

ObjFunction *compile(const char *source);

#endif