#include <stdarg.h>
#include <stdio.h>

#define LOG_FATAL_ERROR -1
#define LOG_EXIT_ERROR 0
#define LOG_NORMAL 1
#define LOG_VERBOSE 2

// log levels:
//
// -1 -- fatal error (stderr)
// 0 -- fatal error 
// 1 -- normal output
// 2 -- verbose output

// sets the default output file. If filename is empty or '-' than
// stdout is used.
void set_output_file(char* filename);

// sets the global log level. Messages above the log level will be ignored.
void set_log_level(int level);

// Prints a log message. Fatal errors are sent to stderr and the program exits.
void elog(int level, char* str, ...);

// Prints a log message to a file. Fatal error messages are sent to stderr and the program exits.
void felog(FILE* f, int level, char* str, ...);

// "felog_default" (shortened). Same as felog, but sends output to default file.
void felog_d(int level, char* str, ...);

// returns true if output is going to a file, false otherwise (stdout)
char is_using_file();