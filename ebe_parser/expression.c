#include <stdlib.h>
#include <string.h>
#include <stdio.h>

#include "expression.h"
#include "helper.h"
#include "log.h"

#define MAX_EXPRESSION_LENGTH 1024

////////////////////////////////////////////////////////////////////////////////

#define EXPR_WELL_FORMED_INITIAL 0
#define EXPR_WELL_FORMED_OPEN_BRACKET 2
#define EXPR_WELL_FORMED_CLOSE_BRACKET 4
#define EXPR_WELL_FORMED_OPERATOR 6
#define EXPR_WELL_FORMED_OP_NOT 12
#define EXPR_WELL_FORMED_TOKEN 8
#define EXPR_WELL_FORMED_TOKEN_WHITESPACE 10
#define EXPR_WELL_FORMED_INVALID -1

#define EXPR_PARSE_READ_TOKEN 2
#define EXPR_PARSE_READ_OPERATOR 4
#define EXPR_PARSE_READ_PAREN 8

static char symbol_read_buffer[MAX_SYMBOL_SIZE];

// mallocs a new node and inits the values to zero
static expression_node* init_expression_node()
{
	expression_node* e = (expression_node*)malloc(sizeof(expression_node));
	
	if (e == 0)
	{
		elog(LOG_VERBOSE, "init_expression_node: out of memory\n");
		exit(1);
	}
	
	e->parent = 0;
	e->left = 0;
	e->right = 0;
	e->sym = 0;
	
	e->unary_bitwise_negate = 0;
	e->unary_minus = 0;
	
	return e;
}

// given any node in an expression tree, finds the root node
static expression_node* find_root(expression_node* e)
{
	if (e == 0)
	{
		return 0;
	}
	
	expression_node* node = e;
	
	while(node->parent)
	{
		if (node->parent != 0)
		{
			node = node->parent;
		}
	}
	
	return node;
}

// Converts a single character into a token type
static token char_to_token_type(char c)
{
	if (is_alphanumeric(c))
	{
		return tk_term;
	}
	else if (is_operator(c))
	{
		return tk_operator;
	}
	else if (is_paren(c))
	{
		return tk_paren;
	}
	else if (is_whitespace(c))
	{
		return tk_whitespace;
	}
	else
	{
		return tk_unknown;
	}
}

// Converts token into a string for output
static char* token_to_string(token t)
{
	switch (t)
	{
		case tk_term: return "term"; break;
		case tk_operator: return "operator"; break;
		case tk_paren: return "paren"; break;
		case tk_whitespace: return "whitespace"; break;
		case tk_unknown: /* fall through */
		default:
			return "term"; break;
	}
}

// initialize and malloc a new symbol, setting
// the contents to the token argument,
// and setting the contents of the value
// to the string val described by length len.
// A null character is appended to the end of the string.
static symbol* init_symbol(token t, char* val, size_t len)
{
	symbol* sym = malloc(sizeof(symbol));
	char* cp;
	
	if (sym == 0)
	{
		elog(LOG_FATAL_ERROR, "init_symbol: out of memory\n");
		exit(1);
	}
	
	memset(sym, 0, sizeof(symbol));
	
	sym->symbol_type = t;

	sym->value = (char**)malloc(sizeof(char**));
	sym->allocated = 1;
	
	cp = (char*)malloc(sizeof(char) * len + 1);

	if (cp == 0)
	{
		elog(LOG_FATAL_ERROR, "init_symbol: out of memory\n");
		exit(1);
	}
	
	memset(cp, 0, sizeof(char) * len + 1);

	strncpy(cp, val, len);
	
	*(sym->value) = cp;
	
	//printf("init_symbol: sym->value: '%s'\n", *(sym->value));fflush(stdout);
	//printf("init_symbolp: sym->value: '%p'\n", sym->value);fflush(stdout);
	
	return sym;
}

void printf_symbol(symbol* s)
{
	char** cpp = s->value;
	char* cp = *cpp;
	elog(LOG_NORMAL, "symbol->symbol_type = '%s', symbol->value = '%s'\n",
		token_to_string(s->symbol_type),
		cp);
}

// recursive helper to print expression tree
static void printf_expression_tree_rhelp(FILE* f, expression_node* e, size_t depth)
{
	//printf("Inside printf_expression_tree\n");
	//printf_symbol(e->sym);
	//printf("\n");
	
	expression_node* node = e;
	char** cpp;
	char* cp;
	
	if (node->left == 0 && node->right == 0)
	{
		if (node->unary_minus)
		{
			if (f == stdout)
			{
				elog(LOG_NORMAL, "`");
			}
			else
			{
				felog_d(LOG_NORMAL, "`");
			}
		}
		else if (node->unary_bitwise_negate)
		{
			if (f == stdout)
			{
				elog(LOG_NORMAL, "~");
			}
			else
			{
				felog_d(LOG_NORMAL, "~");
			}
		}
		
		if (node->sym != 0 && node->sym->value != 0)
		{
			//printf("here2\n");fflush(stdout);
			//printf("sym->value: %p\n", node->sym->value);fflush(stdout);
			
			cpp = node->sym->value;
			cp = *cpp;
			
			//printf("sym->value2: %p\n", cp);fflush(stdout);

			if (f == stdout)
			{
				elog(LOG_NORMAL, "%s", cp);
			}
			else
			{
				felog_d(LOG_NORMAL, "%s", cp);
			}
			
			//printf("here3\n");fflush(stdout);
		}
		return;
	}
	
	if (node->left != 0)
	{
		if (node->unary_minus)
		{
			if (f == stdout)
			{
				elog(LOG_NORMAL, "`");
			}
			else
			{
				felog_d(LOG_NORMAL, "`");
			}
		}
		
		else if (node->unary_bitwise_negate)
		{
			if (f == stdout)
			{
				elog(LOG_NORMAL, "~");
			}
			else
			{
				felog_d(LOG_NORMAL, "~");
			}
		}

		if (f == stdout)
		{
			elog(LOG_NORMAL, "(");
		}
		else
		{
			felog_d(LOG_NORMAL, "(");
		}
		
		printf_expression_tree_rhelp(f, node->left, depth + 1);
		
		node = node->left;
	}
	/*
	printf("printing expression tree\n");
	printf_symbol(node->sym);
	printf_symbol(node->parent->sym);
	printf_symbol(node->parent->right->sym);
	*/
	
	if (node->parent != 0)
	{
		node = node->parent;
	}
	
	if (node->sym != 0 && node->sym->value != 0)
	{
		cpp = node->sym->value;
		cp = *cpp;
		
		if (f == stdout)
		{
			elog(LOG_NORMAL, "%s", cp);
		}
		else
		{
			felog_d(LOG_NORMAL, "%s", cp);
		}
	}
	
	if (node->right != 0)
	{
		printf_expression_tree_rhelp(f, node->right, depth + 1);
		
		if (f == stdout)
		{
			elog(LOG_NORMAL, ")");
		}
		else
		{
			felog_d(LOG_NORMAL, ")");
		}
	}
	else if (depth == 0)
	{
		if (f == stdout)
		{
			elog(LOG_NORMAL, ")");
		}
		else
		{
			felog_d(LOG_NORMAL, ")");
		}
	}
}

// prints an expression tree. While a file pointer is the first argument,
// it is expected to be 0 for "default" or stdout.
void printf_expression_tree(FILE* f, expression_node* e)
{
	printf_expression_tree_rhelp(f, e, 0);
	
	if (f == stdout)
	{
		elog(LOG_NORMAL, "\n");
	}
	else
	{
		felog_d(LOG_NORMAL, "\n");
	}
}

// frees the memory associated with a symbol
static void free_symbol(symbol* s)
{
	if (s == 0)
	{
		return;
	}
	
	if (s->value != 0)
	{
		if (s->allocated == 1 && *(s->value) != 0)
		{
			free(*(s->value));
			s->allocated = 0;
		}
		
		free(s->value);
	}
	
	free(s);
}

void free_expression_node(expression_node* e)
{
	if (e == 0)
	{
		return;
	}
	
	if (e->left != 0)
	{
		free_expression_node(e->left);
	}
	
	if (e->right != 0)
	{
		free_expression_node(e->right);
	}
	
	free_symbol(e->sym);
	free(e);
}

// returns true if the expression is well formed, false otherwise
static int is_well_formed(char* expression)
{
	if (expression == 0)
	{
		return 0;
	}
	if (strlen(expression) == 0)
	{
		return 0;
	}

	int state = EXPR_WELL_FORMED_INITIAL;
	
	size_t i, len;
	char ch;
	
	size_t open_count = 0, close_count = 0;
	
	len = strlen(expression);
	
	for(i=0; i<len; i++)
	{
		ch = expression[i];
		
		if (is_open_paren(ch))
		{
			open_count++;
			switch(state)
			{
				case EXPR_WELL_FORMED_INITIAL:		state = EXPR_WELL_FORMED_OPEN_BRACKET;	break;
				case EXPR_WELL_FORMED_OPEN_BRACKET:	state = EXPR_WELL_FORMED_OPEN_BRACKET;	break;
				case EXPR_WELL_FORMED_CLOSE_BRACKET:	state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_OPERATOR:		state = EXPR_WELL_FORMED_OPEN_BRACKET;	break;
				case EXPR_WELL_FORMED_OP_NOT:		state = EXPR_WELL_FORMED_OPEN_BRACKET;	break;
				case EXPR_WELL_FORMED_TOKEN:		state = EXPR_WELL_FORMED_OPEN_BRACKET;	break;
				case EXPR_WELL_FORMED_TOKEN_WHITESPACE:	state = EXPR_WELL_FORMED_OPEN_BRACKET;	break;
				default:				state = EXPR_WELL_FORMED_INVALID;	break;
			}
		}
		else if (is_close_paren(ch))
		{
			close_count++;
			switch(state)
			{
				case EXPR_WELL_FORMED_INITIAL:		state = EXPR_WELL_FORMED_INVALID;	break;
				// think the following was a bug in the original javascript, changed the next to invalid:
				case EXPR_WELL_FORMED_OPEN_BRACKET:	state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_CLOSE_BRACKET:	state = EXPR_WELL_FORMED_CLOSE_BRACKET;	break;
				case EXPR_WELL_FORMED_OPERATOR:		state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_OP_NOT:		state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_TOKEN:		state = EXPR_WELL_FORMED_CLOSE_BRACKET;	break;
				case EXPR_WELL_FORMED_TOKEN_WHITESPACE:	state = EXPR_WELL_FORMED_CLOSE_BRACKET;	break;
				default:		state = EXPR_WELL_FORMED_INVALID;	break;
			}
		}
		else if (is_binary_operator(ch))
		{
			switch(state)
			{
				case EXPR_WELL_FORMED_INITIAL:		state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_OPEN_BRACKET:	state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_CLOSE_BRACKET:	state = EXPR_WELL_FORMED_OPERATOR;	break;
				case EXPR_WELL_FORMED_OPERATOR:		state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_OP_NOT:		state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_TOKEN:		state = EXPR_WELL_FORMED_OPERATOR;	break;
				case EXPR_WELL_FORMED_TOKEN_WHITESPACE:	state = EXPR_WELL_FORMED_OPERATOR;	break;
				default:		state = EXPR_WELL_FORMED_INVALID;	break;
			}
		}
		else if (is_alphanumeric(ch))
		{
			switch(state)
			{
				case EXPR_WELL_FORMED_INITIAL:		state = EXPR_WELL_FORMED_TOKEN;		break;
				case EXPR_WELL_FORMED_OPEN_BRACKET:	state = EXPR_WELL_FORMED_TOKEN;		break;
				case EXPR_WELL_FORMED_CLOSE_BRACKET:	state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_OPERATOR:		state = EXPR_WELL_FORMED_TOKEN;		break;
				case EXPR_WELL_FORMED_OP_NOT:		state = EXPR_WELL_FORMED_TOKEN;		break;
				case EXPR_WELL_FORMED_TOKEN:		state = EXPR_WELL_FORMED_TOKEN;		break;
				case EXPR_WELL_FORMED_TOKEN_WHITESPACE:	state = EXPR_WELL_FORMED_INVALID;	break;
				default:		state = EXPR_WELL_FORMED_INVALID;	break;
			}
		}
		else if (is_whitespace(ch))
		{
			// note that this case only matters for separation of tokens;
			// all other states do not care about whitespace
			switch(state)
			{
				case EXPR_WELL_FORMED_INITIAL:		state = EXPR_WELL_FORMED_INITIAL;		break;
				case EXPR_WELL_FORMED_OPEN_BRACKET:	state = EXPR_WELL_FORMED_OPEN_BRACKET;		break;
				case EXPR_WELL_FORMED_CLOSE_BRACKET:	state = EXPR_WELL_FORMED_CLOSE_BRACKET;		break;
				case EXPR_WELL_FORMED_OPERATOR:		state = EXPR_WELL_FORMED_OPERATOR;		break;
				case EXPR_WELL_FORMED_OP_NOT:		state = EXPR_WELL_FORMED_OP_NOT;			break;
				case EXPR_WELL_FORMED_TOKEN:		state = EXPR_WELL_FORMED_TOKEN_WHITESPACE;	break;
				case EXPR_WELL_FORMED_TOKEN_WHITESPACE:	state = EXPR_WELL_FORMED_TOKEN_WHITESPACE;	break;
				default:		state = EXPR_WELL_FORMED_INVALID;		break;
			}
		}
		else if (is_unary_operator(ch))
		{
			// double nots (~~) are not ok
			switch(state)
			{
				case EXPR_WELL_FORMED_INITIAL:		state = EXPR_WELL_FORMED_OP_NOT;		break;
				case EXPR_WELL_FORMED_OPEN_BRACKET:	state = EXPR_WELL_FORMED_OP_NOT;		break;
				case EXPR_WELL_FORMED_CLOSE_BRACKET:	state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_OPERATOR:		state = EXPR_WELL_FORMED_OP_NOT;		break;
				case EXPR_WELL_FORMED_OP_NOT:		state = EXPR_WELL_FORMED_INVALID;		break;
				case EXPR_WELL_FORMED_TOKEN:		state = EXPR_WELL_FORMED_INVALID;	break;
				case EXPR_WELL_FORMED_TOKEN_WHITESPACE:	state = EXPR_WELL_FORMED_OP_NOT;		break;
				default:		state = EXPR_WELL_FORMED_INVALID;	break;
			}
		}
		else if (ch == 0)
		{
			// end of string
			break;
		}
		else
		{
			state = EXPR_WELL_FORMED_INVALID;
		}
		
		if (state == EXPR_WELL_FORMED_INVALID)
		{
			return 0;
		}
	}
	
	// check brackets
	if (open_count != close_count)
		return 0;

	// the only valid characters to end on are close bracket,
	// token, or token whitespace 
	if (state == EXPR_WELL_FORMED_TOKEN ||
		state == EXPR_WELL_FORMED_TOKEN_WHITESPACE ||
		state == EXPR_WELL_FORMED_CLOSE_BRACKET)
		return 1;

	return 0;
}

// reads a token from the given string. memory is allocated and returned
// for a new symbol which then contains the value read.
// When done, index contains the position of the last character read.
static symbol* read_next_symbol(char* s, size_t* index)
{
	elog(LOG_VERBOSE, "Reading next symbol from '%s' starting at %d\n", s, *index);

	size_t start_index = *index;
	
	size_t input_length = strlen(s);
	
	if (start_index < 0 || start_index > input_length)
	{
		elog(LOG_FATAL_ERROR, "read_next_symbol: index (%d) out of bounds exception (len=%d)\n",
			start_index, input_length);
		exit(1);
	}
	
	memset(symbol_read_buffer, 0, sizeof(char)*MAX_SYMBOL_SIZE);
	
	char ch;
	size_t position = start_index;
	size_t symbol_position = 0;
	
	token read_token_type;
	token initial_token_type = tk_unknown;
	
	for(position = start_index; position < input_length; position++)
	{		
		if (position > input_length || symbol_position > MAX_SYMBOL_SIZE )
		{
			elog(LOG_FATAL_ERROR, "read_next_symbol: index overflow exception. Position: %d, symbol_position: %d\n",
				position, symbol_position);
			exit(1);
		}
		
		ch = s[position];
		
		// discover what kind of token is being read
		read_token_type = char_to_token_type(ch);
		
		if (read_token_type == tk_unknown)
		{
			elog(LOG_FATAL_ERROR, "read_next_symbol: Encountered unexpected character: '%c' (0x%x)\n",
				ch, ch);
			exit(1);
		}
		
		if (initial_token_type == tk_unknown)
		{
			// read until some sort of starting token is found
			if (read_token_type != tk_unknown && read_token_type != tk_whitespace)
			{
				initial_token_type = read_token_type;
				//printf("Setting token type to '%s'\n", token_to_string(read_token_type));
			}
			else
			{
				continue;
			}
		}
		
		// read until the type of token changes
		if (read_token_type != initial_token_type)
		{
			// this decrement fixes a problem with whitespace tokens
			position--;
			break;
		}
		
		symbol_read_buffer[symbol_position] = ch;
		
		// end of string, nothing else to do
		if (ch == 0)
		{
			break;
		}
		
		// increment needs to come before the following one-character break
		symbol_position++;
		
		// operators and parentheses are only one character in length
		if (initial_token_type == tk_operator || initial_token_type == tk_paren)
		{
			break;
		}
	}
	
	//printf("Symbol read buffer: '%s', length: %d\n", symbol_read_buffer, symbol_position);fflush(stdout);
	
	// safety check...
	if (position < start_index)
	{
		elog(LOG_FATAL_ERROR, "read_next_symbol: final position (%d) is before initial position (%d)!\n",
			position, start_index);
	}
	
	*index = position;
	
	return init_symbol(initial_token_type, symbol_read_buffer, symbol_position);
}

// assumes sym is valid; assigns symbol to a node
static void assign_symbol(expression_node* node, symbol* sym)
{
	char** cpp = sym->value;
	char* cp = *cpp;
	
	if (is_binary_operator(cp[0]))
	{
		node->sym = sym;
	}
	else if(is_unary_operator(cp[0]))
	{
		if (cp[0] == '`')
		{
			node->unary_minus = 1;
		}
		else if (cp[0] == '~')
		{
			node->unary_bitwise_negate = 1;
		}
	}
	// variable
	else
	{
		node->sym = sym;
	}
}

expression_node* parse(char* s)
{
	elog(LOG_VERBOSE, "Parsing expression: %s\n", s);
		
	if (!is_well_formed(s))
	{
		elog(LOG_FATAL_ERROR, "Expression is not well-formed.\n");
		exit(1);
	}
	
	size_t position = 0;
	size_t len = strlen(s);
	symbol* sym;
	
	char** cpp;
	char* cp;
	
	size_t depth = 0;
	
	int pending_bitwise_negate = 0;
	int pending_unary_minus = 0;
	
	expression_node* root = init_expression_node();
	expression_node* current_node = root;
	expression_node* t;
	
	if (root == 0)
	{
		elog(LOG_FATAL_ERROR, "parse: out of memory\n");
		exit(1);
	}
	
	while(position < len)
	{
		//printf("while...\n");fflush(stdout);
		
		elog(LOG_VERBOSE, "\n");
		
		sym = read_next_symbol(s, &position);
		position++;

		printf_symbol(sym);
		
		if (sym->symbol_type == tk_term)
		{
			if (current_node != root && depth > 0 && 
				current_node->left == 0 && current_node->right == 0 && current_node->sym == 0)
			{
				// down some kind of parentheses path
				elog(LOG_VERBOSE, "assigning value to current node\n");
				current_node->sym = sym;
				
				if (pending_bitwise_negate)
				{
					current_node->unary_bitwise_negate = 1;
					pending_bitwise_negate = 0;
				}
				else if (pending_unary_minus)
				{
					current_node->unary_minus = 1;
					pending_unary_minus = 0;
				}
				
				current_node = current_node->parent;
			}
			else if (current_node->left == 0)
			{
				elog(LOG_VERBOSE, "assinging value to left node\n");
				current_node->left = init_expression_node();
				current_node->left->parent = current_node;
				current_node->left->sym = sym;
				
				if (pending_bitwise_negate)
				{
					current_node->left->unary_bitwise_negate = 1;
					pending_bitwise_negate = 0;
				}
				else if (pending_unary_minus)
				{
					current_node->left->unary_minus = 1;
					pending_unary_minus = 0;
				}
			}
			else if (current_node->right == 0)
			{
				elog(LOG_VERBOSE, "assinging value to right node\n");
				current_node->right = init_expression_node();
				current_node->right->parent = current_node;
				current_node->right->sym = sym;
				
				if (pending_bitwise_negate)
				{
					current_node->right->unary_bitwise_negate = 1;
					pending_bitwise_negate = 0;
				}
				else if (pending_unary_minus)
				{
					current_node->right->unary_minus = 1;
					pending_unary_minus = 0;
				}
			}
			// uh oh, both nodes are full
			else
			{
				elog(LOG_VERBOSE, "assinging value -- current node is full\n");
				
				// I am not sure that this is entirely correct, but 
				// it seems mostly correct.
				while(depth > 0 && current_node->left != 0 && current_node->right != 0)
				{
					depth--;
					elog(LOG_VERBOSE, "current_node = current_node->parent;\n");
					current_node = current_node->parent;
				}
				
				if (current_node->left == 0)
				{
					elog(LOG_VERBOSE, "assinging value to left node\n");
					current_node->left = init_expression_node();
					current_node->left->parent = current_node;
					current_node->left->sym = sym;
					
					if (pending_bitwise_negate)
					{
						current_node->left->unary_bitwise_negate = 1;
						pending_bitwise_negate = 0;
					}
					else if (pending_unary_minus)
					{
						current_node->left->unary_minus = 1;
						pending_unary_minus = 0;
					}
				}
				else if (current_node->right == 0)
				{
					elog(LOG_VERBOSE, "assinging value to right node\n");
					current_node->right = init_expression_node();
					current_node->right->parent = current_node;
					current_node->right->sym = sym;
					
					if (pending_bitwise_negate)
					{
						current_node->right->unary_bitwise_negate = 1;
						pending_bitwise_negate = 0;
					}
					else if (pending_unary_minus)
					{
						current_node->right->unary_minus = 1;
						pending_unary_minus = 0;
					}
				}
				else
				{
					elog(LOG_FATAL_ERROR, "parse: Could not find node to assign value\n");
				}
			}
		}
		else if (sym->symbol_type == tk_operator)
		{
			cpp = sym->value;
			cp = *cpp;
			
			if (is_unary_operator(cp[0]))
			{
				if (cp[0] == '~')
				{
					pending_bitwise_negate = 1;
					elog(LOG_VERBOSE, "found pending bitwise negate\n");
				}
				else if (cp[0] == '`')
				{
					pending_unary_minus = 1;
					elog(LOG_VERBOSE, "found pending unary minus\n");
				}
				continue;
			}
			
			if (current_node->sym == 0)
			{
				elog(LOG_VERBOSE, "assigning operator '%c' to current node\n", cp[0]);
				assign_symbol(current_node, sym);
			}
			// already have an operator assigned.
			else
			{
				char left = (*(current_node->sym->value))[0];
				char right = (*(sym->value))[0];
				
				elog(LOG_VERBOSE, "comparing operator %c to %c\n", left, right);
				
				int p = operator_precendence(left, right);
				
				// if the current operator should be evaluated first
				if (p == 0 || p == 1)
				{
					elog(LOG_VERBOSE, "adding parent\n");
					
					// backtrack to a node with no parent, or a node that has a parent
					// but not parent symbol
					while (depth > 0 && current_node->parent != 0 && current_node->parent->sym != 0)
					{
						elog(LOG_VERBOSE, "Decreasing depth.\n");
						depth--;
						current_node = current_node->parent;
					}
					
					// the node has a parent, but no parent symbol
					if (current_node->parent != 0 && current_node->parent->sym == 0)
					{
						current_node = current_node->parent;
					}
					else
					{
						// check to make sure this isn't still inside parentheses
						if (depth > 0 && current_node->left != 0 && current_node->right != 0)
						{
							elog(LOG_VERBOSE, "Depth>0, current_node full\n");
							
							t = init_expression_node();
							
							t->left = current_node->right;
							t->left->parent = t;
							t->parent = current_node;
							
							current_node->right = t;
							
							current_node = t;
						}
						else
						{
							// the node does not have a parent
							current_node->parent = init_expression_node();
							current_node->parent->left = current_node;
							current_node = current_node->parent;
						}
					}
					
					assign_symbol(current_node, sym);
				}
				// else this new operator should be evaluated first
				else
				{
					elog(LOG_VERBOSE, "inserting right child (depth: %d)\n", depth);
					
					t = init_expression_node();
					
					assign_symbol(t, sym);
					
					t->left = current_node->right;
					if (t->left != 0)
					{
						t->left->parent = t;
					}
					
					t->parent = current_node;
					current_node->right = t;
					current_node = t;
					
					depth++;
				}
			}
		}
		else if (sym->symbol_type == tk_paren)
		{
			if (is_open_paren((*(sym->value))[0]))
			{
				t = init_expression_node();
				
				if (pending_bitwise_negate)
				{
					current_node->unary_bitwise_negate = 1;
					pending_bitwise_negate = 0;
				}
				else if (pending_unary_minus)
				{
					current_node->unary_minus = 1;
					pending_unary_minus = 0;
				}
				
				if (current_node->left != 0)
				{
					elog(LOG_VERBOSE, "left paren inserting right child\n");

					if (current_node->right != 0)
					{
						t->left = current_node->right;
						t->left->parent = t;
					}
				
					t->parent = current_node;
					current_node->right = t;
				}
				else
				{
					elog(LOG_VERBOSE, "left paren inserting left child\n");
				
					t->parent = current_node;
					current_node->left = t;
				}
				
				current_node = t;
				depth++;
			}
			else
			{
				if (depth > 0 && current_node->parent != 0)
				{
					elog(LOG_VERBOSE, "Decreasing depth\n");
					depth--;
					current_node = current_node->parent;
				}
				else
				{
					elog(LOG_VERBOSE, "close paren. adding parent\n");
					
					current_node->parent = init_expression_node();
					current_node->parent->left = current_node;
					current_node = current_node->parent;
				}
			}
		}
		else
		{
		}
	
		printf_expression_tree(stdout, find_root(root));
		//free_symbol(sym);
	}
	
	elog(LOG_VERBOSE, "Parsing complete.\n\n");
	
	root = find_root(root);
	
	//printf_expression_tree(root);
	
	return root;
}

