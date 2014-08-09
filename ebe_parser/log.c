#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>

// log levels:
//
// -1 -- fatal error (stderr)
// 0 -- fatal error 
// 1 -- normal output
// 2 -- verbose output

static int log_level;

static FILE* output_file;

char is_using_file()
{
	return output_file != stdout;
}

void set_output_file(char* filename)
{
	if (filename == 0)
	{
		output_file = stdout;
	}
	else if (filename != 0 &&
		filename[0] != 0 && filename[0] == '-' &&
		filename[1] == 0)
	{
		output_file = stdout;
	}
	else
	{
		output_file = fopen(filename, "w");
		
		if (output_file == 0)
		{
			fprintf(stderr, "Could not open '%s'\n", filename);
			exit(1);
		}
	}
}

void set_log_level(int level)
{
	log_level = level;
}

void elog(int level, char* str, ...)
{
	if (level > log_level && level > 0)
	{
		return;
	}
	
	va_list(args);
	va_start(args, str);
	
	if (level == -1)
	{
		vfprintf(stderr, str, args);
		exit(1);
	}
	
	vprintf(str, args);
}

void felog(FILE* f, int level, char* str, ...)
{
	if (level > log_level && level > 0)
	{
		return;
	}
	
	va_list(args);
	va_start(args, str);
	
	if (level == -1)
	{
		vfprintf(stderr, str, args);
		exit(1);
	}
	
	vfprintf(f, str, args);
}

void felog_d(int level, char* str, ...)
{
	if (level > log_level && level > 0)
	{
		return;
	}
	
	va_list(args);
	va_start(args, str);
	
	if (level == -1)
	{
		vfprintf(stderr, str, args);
		exit(1);
	}
	
	vfprintf(output_file, str, args);
}