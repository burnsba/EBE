#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <argp.h>

#include "helper.h"
#include "A193023.h"

// user guide for uthash at
// http://troydhanson.github.io/uthash/userguide.html
#include "uthash.h"

// max paren depth is implicitly 127 as the paren_state array uses signed char

// buffer output size
#define MAX_OUTPUT_STR_LEN 1024

// grammar is something like
// open SYM close [op open SYM close [...]]

#define POSITION_STATE_OPEN 0
#define POSITION_STATE_SYM 1
#define POSITION_STATE_CLOSE 2

#define POSITION_STATE_OP 3

// symbols are 'a', 'b', 'c', ... 'z', 'aa', 'ab', ... 'zy', 'zz'
#define MAX_SYMBOLS 677

/// hrrrm, need to work this out ...
char* available_symbols[MAX_SYMBOLS];

/* Used by main to communicate with parse_opt. */
struct arguments
{
	int log_level;
	char *output_file;
	uint64_t starting_id_a;
        uint64_t starting_id_b;
        uint64_t starting_id_c;
        uint64_t starting_id_d;
        uint32_t number_variable_slots;
};

// id will be given as 
// n.a.b.c.d
// where
// n is the number of variable slots
// a is the interation for parentheses
// b is the iteration for variables
// c is the iteration for unary operators
// d is the iteration for binary operators
// 
// at this point, iteration is arbitrarily defined, as per this file.

uint64_t id_a; // global variables are initialized to zero
uint64_t id_b;
uint64_t id_c;
uint64_t id_d;

uint64_t starting_id_a;
uint64_t starting_id_b;
uint64_t starting_id_c;
uint64_t starting_id_d;

uint32_t number_variable_slots;

uint32_t phase_1_first;
uint32_t phase_2_first;
uint32_t phase_3_first;
uint32_t phase_4_first;

// d is the iteration for binary operators
void phase_4()
{
}

// c is the iteration for unary operators
void phase_3()
{
}

// b is the iteration for variables
// number of possible combinations is described by the catalan numbers
// the variableto use in each position is described by the sequence https://oeis.org/A071159
void phase_2(char * expression)
{
        // overflow for calculating max values
        uint32_t overflow = 0;
        
        // output buffer
        uint8_t output_string[MAX_OUTPUT_STR_LEN];
        
        // position in output buffer
        size_t output_string_position = 0;
        
        // position in input buffer
        size_t input_string_position = 0;
        
        uint64_t variable_position;
        
        // iterating over input
        char ch;
        
        // this is going to be an array which holds the count (current value) for the variable
        // in each position it may occur in. This will be resolved to a position in the
        // available_symbols
        uint32_t* variable_state = (uint32_t*)malloc(sizeof(uint32_t)* number_variable_slots);
        memset(variable_state, 0, sizeof(uint32_t) * number_variable_slots);
        
        uint64_t max_n = get_bell_sum((uint64_t)number_variable_slots);
        max_n++;
        
        uint64_t i,j;
        
        if (phase_2_first == 0)
        {
                phase_2_first = 1;

                id_b = starting_id_b;
                
                if (starting_id_b > 0)
                {
                        id_b = starting_id_b;
                }
                else
                {
                        id_b = get_bell_sum(number_variable_slots - 1);
                }
        }
        else
        {
                // starts at one before the first item
                id_b = get_bell_sum(number_variable_slots - 1);
        }
        
        node_description* node = 0;

        id_b++;
        
        node = A193023((uint64_t)id_b);
        
        while (id_b < max_n)
        {      
                memset(output_string, 0, MAX_OUTPUT_STR_LEN);
                output_string_position = 0;
                input_string_position = 0;
                
                variable_position = 0;
                
                for(i=0; i < node->len + 1; i++)
                {
                        variable_state[i] = node->values[i] + 1;
                }
                
                do
                {
                        ch = expression[input_string_position++];
                        
                        if (ch != 's')
                        {
                                output_string[output_string_position++] = ch;
                        }
                        else
                        {
                                 strcat((char*)(output_string + output_string_position), available_symbols[variable_state[variable_position]]);
                                 output_string_position += strlen(available_symbols[variable_state[variable_position]]);
                                 
                                 variable_position++;
                        }
                }
                while (ch != 0);
                
                printf("phase_2: %d.%d.%d: %s\n", number_variable_slots, id_a, id_b, output_string);
                
                id_b++;
                node_next(node);
        }
        
        node_free(node);
}

// a is the iteration for parentheses
void phase_1()
{
        // overflow for calculating max values
        uint32_t overflow = 0;
        
        // output buffer
        uint8_t output_string[MAX_OUTPUT_STR_LEN];
        
        // position in output buffer
        size_t output_string_position = 0;
        
        // max consecutive parentheses in a single slot
        size_t max_paren_in_slot = number_variable_slots - 1;
        
        // max number of slots in the expression to place a paren
        size_t num_paren_slots = number_variable_slots * 2;

        // hard-coded number of states, described by the grammar
        size_t max_states_in_iteration = 3;
        if (number_variable_slots > 1)
        {
                max_states_in_iteration += 4 * (number_variable_slots - 1);
        }

        size_t state = 0;
        size_t state_count = 0;
        size_t i;
        size_t j;

        // this is going to be an array which holds the count of parentheses in each "slot."
        // A slot would be a valid location for a paren to show up, which is to the left
        // or right of any symbol
        uint8_t* paren_state = (uint8_t*)malloc(sizeof(uint8_t)* num_paren_slots);
        memset(paren_state, 0, sizeof(uint8_t) * num_paren_slots);

        size_t max_combinations = lazy_pow(number_variable_slots, num_paren_slots, &overflow);
        
        if (overflow)
        {
                printf("too many compbinations (phase_1).\n");
                exit(1);
        }

        if (starting_id_a > 0)
        {
                id_a = starting_id_a;

                // convert starting a into base max_paren_in_slot
                // and set initial values
                uint64_t t = starting_id_a;
                j=0;
                while (t != 0)
                {
                        paren_state[j] = t % (max_paren_in_slot + 1);
                        t /= (max_paren_in_slot + 1);
                        j++;
                }
        }
        else
        {
                id_a = 0;
        }

        if (overflow)
        {
                printf("Can't find begining id (too many combinations).\n");
                exit(1);
        }

        size_t iter_count = 0;
        size_t open_count, close_count;
        size_t double_open, double_close;

        while (id_a < max_combinations)
        {
                id_a++;

                open_count = 0;
                close_count = 0;

                double_open = 0;
                double_close = 0;

                // a couple quick checks
                for (i=0; i<num_paren_slots; i += 2)
                {
                        // ignore open sym close, such as: (SYM), ((SYM)), etc
                        // and more generally, all cases where there are both open and close parentheses around a symbol
                        if (paren_state[i] > 0 && paren_state[i+1] > 0)
                                goto inc_paren_state;

                        // Count number of open and close totals to make sure they match
                        open_count += paren_state[i];
                        close_count += paren_state[i+1];

                        if (paren_state[i] == 2)
                                double_open++;

                        if (paren_state[i+1] == 2)
                                double_close++;

                        // can't have a closing paren before an open
                        if (close_count > open_count)
                                goto inc_paren_state;
                }

                // ignore mismatch totals
                if (close_count != open_count)
                        goto inc_paren_state;
 
                // ignore simple double paren groups, it will be covered by a single paren group
                // Note: this only skips if there are no single parentheses
                if (double_open > 0 && double_open == double_close && open_count == double_open * 2)
                        goto inc_paren_state;
                
                
                // alright, onto to displaying the current item

                iter_count = num_paren_slots - 1;

                memset(output_string, 0, MAX_OUTPUT_STR_LEN);
                output_string_position = 0;

                state = POSITION_STATE_OPEN;
                for (state_count = 0; state_count < max_states_in_iteration; state_count++)
                {
                        switch(state)
                        {
                                case POSITION_STATE_OPEN :
                                        for(i=0; i<paren_state[iter_count]; i++)
                                        {
                                                strcat((char*)(output_string + output_string_position), "(");
                                                output_string_position++;
                                        }
                                        iter_count--;
                                break;
                                case POSITION_STATE_SYM :
                                        // s for symbol
                                        strcat((char*)(output_string + output_string_position), " s ");
                                        output_string_position += 3;
                                break;
                                case POSITION_STATE_CLOSE :
                                        for(i=0; i<paren_state[iter_count]; i++)
                                        {
                                                strcat((char*)(output_string + output_string_position), ")");
                                                output_string_position++;
                                        }
                                        iter_count--;
                                break;
                                case POSITION_STATE_OP :
                                        // : for operator
                                        strcat((char*)(output_string + output_string_position), " : ");
                                        output_string_position += 3;
                                break;
                        }

                        state = state + 1;
                        state = state % 4;    
                }

                printf("phase_1: %d.%d: %s\n", number_variable_slots, id_a, output_string);
                phase_2(output_string);

inc_paren_state:
                j=0;
                while(1)
                {
                        paren_state[j] = paren_state[j] + 1;
                        if (paren_state[j] > max_paren_in_slot)
                                paren_state[j] = 0;
                        else
                                break;
                        j++;
                        if (j > num_paren_slots)
                                break;
                }
        }
}

const char *argp_program_version =
	"ebe gen 0.1";
const char *argp_program_bug_address =
	"<not_used@tld>";

/* Program documentation. */
static char doc[] =
	"encyclopedia (of) binary expressions generator\n"
	"Deterministically generates expressions to be evaluated\n"
        "Id is defined as\n"
        "n.a.b.c.d\n"
        "where\n"
        "n is the number of variable slots\n"
        "a is the iteration for parentheses\n"
        "b is the iteration for variables\n"
        "c is the iteration for unary operators\n"
        "d is the iteration for binary operators";
	
/* A description of the arguments we accept. */
static char args_doc[] = "NUM";
     
/* The options we understand. */
static struct argp_option options[] = {
	{"verbose",  'v', 0,      0,  "Produce verbose output" },
	{"quiet",    'q', 0,      0,  "Don't produce any output" },
	{"silent",   's', 0,      OPTION_ALIAS },
	{"output",   'o', "FILE", 0,
	"Output to FILE instead of standard output" },
	{"id_a",  'a', "NUM",      0,  "Starting id for 'a' part of id" },
        {"id_b",  'b', "NUM",      0,  "Starting id for 'b' part of id (requires a)" },
        {"id_c",  'c', "NUM",      0,  "Starting id for 'c' part of id (requires a,b)" },
        {"id_d",  'd', "NUM",      0,  "Starting id for 'd' part of id (requires a,b,c)" },
	{ 0 }
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
		case 'a':
			arguments->starting_id_a = arg ? atol (arg) : 0;
			break;
                case 'b':
			arguments->starting_id_b = arg ? atol (arg) : 0;
			break;
                case 'c':
			arguments->starting_id_c = arg ? atol (arg) : 0;
			break;
                case 'd':
			arguments->starting_id_d = arg ? atol (arg) : 0;
			break;

		case ARGP_KEY_ARG:
                        arguments->number_variable_slots = atoi (arg);
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
	arguments.starting_id_a = 0;
        arguments.starting_id_b = 0;
        arguments.starting_id_c = 0;
        arguments.starting_id_d = 0;
	
	/* Parse our arguments; every option seen by parse_opt will
	be reflected in arguments. */
	argp_parse (&argp, argc, argv, 0, 0, &arguments);
        
        if (arguments.starting_id_d > 0 && (arguments.starting_id_c == 0 ||
                arguments.starting_id_b || 0 && arguments.starting_id_a == 0))
        {
                printf("Using d requires a,b, and c\n");
                exit(0);
        }
        
        if (arguments.starting_id_c > 0 &&
                (arguments.starting_id_b || 0 && arguments.starting_id_a == 0))
        {
                printf("Using c requires a and b\n");
                exit(0);
        }
        
        if (arguments.starting_id_b > 0 && arguments.starting_id_a == 0)
        {
                printf("Using b requires a\n");
                exit(0);
        }
        
        number_variable_slots = arguments.number_variable_slots;
        starting_id_a = arguments.starting_id_a;
        starting_id_b = arguments.starting_id_b;
        starting_id_c = arguments.starting_id_c;
        starting_id_d = arguments.starting_id_d;
        
        if (number_variable_slots == 0)
        {
                printf("Number of variables must be greater than zero.\n");
                exit(0);
        }
        
        if (number_variable_slots > MAX_SYMBOLS)
        {
                printf("Number of variables must be less than %d\n", MAX_SYMBOLS);
                exit(0);
        }
        
        // set up variable names
        uint32_t i;
        for (i=1; i<number_variable_slots+1; i++)
        {
                available_symbols[i] = count_to_var_name(i); 
        }
        
        // alright, done with initial parsing and setup, on to phase 1       
        phase_1();

        return 0;
}