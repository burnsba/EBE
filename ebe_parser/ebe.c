#include <stdio.h>
#include <stdlib.h>
#include <argp.h>

#include "log.h"

const char *argp_program_version =
	"ebe 0.1";
const char *argp_program_bug_address =
	"<not_used@tld>";

/* Program documentation. */
static char doc[] =
	"encyclopedia (of) binary expressions\n"
	"Last command line argument is parsed as expression. If "
	"no variables are found (a,b,x, etc.) and the expression contains "
	"only constants the expression is evaluated immediately. Otherwise, "
	"the variables are initialized to zero and then incremented over "
	"every possible combination (e.g., a=0,b=0; a=0,b=1; a=1,b=0; a=1,b=1)";
	
/* A description of the arguments we accept. */
static char args_doc[] = "EXPR";
     
/* The options we understand. */
static struct argp_option options[] = {
	{"verbose",  'v', 0,      0,  "Produce verbose output" },
	{"quiet",    'q', 0,      0,  "Don't produce any output" },
	{"silent",   's', 0,      OPTION_ALIAS },
	{"output",   'o', "FILE", 0,
	"Output to FILE instead of standard output" },
	{"bits",  'b', "MAX_BITS",      0,  "Max number of bits for each variable" },
	{ 0 }
};
     
/* Used by main to communicate with parse_opt. */
struct arguments
{
	int log_level;
	char *output_file;
	int max_bits;
	
	char* expression;
};
     
/* Parse a single option. */
static error_t parse_opt (int key, char *arg, struct argp_state *state)
{
	/* Get the input argument from argp_parse, which we
	  know is a pointer to our arguments structure. */
	struct arguments *arguments = state->input;

	switch (key)
	{
		case 'q':
		case 's':
			arguments->log_level = 0;
			break;
		case 'v':
			arguments->log_level = 2;
			break;
		case 'o':
			arguments->output_file = arg;
			break;
		case 'b':
			arguments->max_bits = arg ? atoi (arg) : 1;
			break;

		case ARGP_KEY_ARG:
			arguments->expression = arg;
			break;

		case ARGP_KEY_END:
		
			if (state->arg_num < 1)
				// Not enough arguments.
				argp_usage (state);
			break;

		default:
			return ARGP_ERR_UNKNOWN;
	}
	return 0;
}
     
/* Our argp parser. */
static struct argp argp = { options, parse_opt, args_doc, doc };
     
int main(int argc, char** argv)
{
	struct arguments arguments;

	/* Default values. */
	arguments.log_level = 1;
	arguments.output_file = "-";
	arguments.max_bits = 1;
	arguments.expression = 0;
	
	/* Parse our arguments; every option seen by parse_opt will
	be reflected in arguments. */
	argp_parse (&argp, argc, argv, 0, 0, &arguments);
	
	set_log_level(arguments.log_level);
	set_output_file(arguments.output_file);
	set_max_bits(arguments.max_bits);
	
	eval_main(arguments.expression);

	return 0;
}