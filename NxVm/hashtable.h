#ifndef table_h
#define table_h

#include "common.h"
#include "value.h"

#define TABLE_MAX_LOAD 0.75

typedef struct {
  ObjString* key;
  Value value;
} Entry;

typedef struct {
  int count;
  int capacity;
  Entry* entries;
} HashTable;

void init_hashtable(HashTable* table);
void free_hashtable(HashTable* table);
bool hashtable_get(HashTable* table, ObjString* key, Value* value);
bool hashtable_set(HashTable* table, ObjString* key, Value value);
bool hashtable_delete(HashTable* table, ObjString* key);
void hashtable_add_all(HashTable* from, HashTable* to);
ObjString* hashtable_find_string(HashTable* table,
                                 const char* chars,
                                 int length,
                                 uint32_t hash);
void hashtable_remove_white(HashTable* table);
void mark_hashtable(HashTable* table);

#endif