#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "common.h"
#include "chunk.h"
#include "debug.h"
#include "vm.h"

static void repl()
{
	// TODO: Improve this, don't just use a fixed line length - lol
	char line[1024];
	for (;;)
	{
		printf("nxs > ");

		if (!fgets(line, sizeof(line), stdin))
		{
			printf("\n");
			break;
		}

		interpret(line);
	}
}

static char *read_file(const char *path)
{
	FILE *file = fopen(path, "rb");
	if (file == NULL)
	{
		fprintf(stderr, "Could not open file \"%s\".\n", path);
		exit(74);
	}

	fseek(file, 0L, SEEK_END);
	size_t file_size = ftell(file);
	rewind(file);

	char *buffer = (char *)malloc(file_size + 1);
	if (buffer == NULL)
	{
		fprintf(stderr, "Not enough memory to read \"%s\".\n", path);
		exit(74);
	}

	size_t bytes_read = fread(buffer, sizeof(char), file_size, file);
	if (bytes_read < file_size)
	{
		fprintf(stderr, "Could not read file \"%s\".\n", path);
		exit(74);
	}

	buffer[bytes_read] = '\0';

	fclose(file);
	return buffer;
}

static void run_file(const char *path)
{
	char *source = read_file(path);
	InterpretResult result = interpret(source);
	free(source);

	if (result == INTERPRET_COMPILE_ERROR)
	{
		exit(65);
	}

	if (result == INTERPRET_RUNTIME_ERROR)
	{
		exit(70);
	}
}

int main(int argc, const char *argv[])
{
	init_vm();

	if (argc == 1)
	{
		repl();
	}
	else if (argc == 2)
	{
		run_file(argv[1]);
	}
	else
	{
		fprintf(stderr, "Usage: clox [path]\n");
		exit(64);
	}

	Chunk chunk;

	init_chunk(&chunk);

	int constant = add_constant(&chunk, 1.2);
	write_chunk(&chunk, OP_CONSTANT, 123);
	write_chunk(&chunk, constant, 123);

	constant = add_constant(&chunk, 3.4);
	write_chunk(&chunk, OP_CONSTANT, 123);
	write_chunk(&chunk, constant, 123);

	write_chunk(&chunk, OP_ADD, 123);

	constant = add_constant(&chunk, 5.6);
	write_chunk(&chunk, OP_CONSTANT, 123);
	write_chunk(&chunk, constant, 123);

	write_chunk(&chunk, OP_DIVIDE, 123);
	write_chunk(&chunk, OP_NEGATE, 123);

	write_chunk(&chunk, OP_RETURN, 123);

	disassemble_chunk(&chunk, "test chunk");
	interpret(&chunk);
	free_vm();
	free_chunk(&chunk);

	return 0;
}