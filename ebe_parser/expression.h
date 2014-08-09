#ifndef __EXPRESSION_H__
#define __EXPRESSION_H__

#define MAX_SYMBOL_SIZE 100

// type of token, either a term (a, b, c, 1, 2, 3), operator (+,-,*), parentheses or whitespace
typedef enum token
{
	tk_term,
	tk_operator,
	tk_paren,
	
	tk_whitespace,
	tk_unknown
	
} token;

// Symbol, specifies type of symbol and pointer to pointer of value (a,b,c,1,2,3,etc)
typedef struct symbol
{
	token symbol_type;
	char** value;
	
	// flag for whether or not the pointer to a pointer is malloced or free
	char allocated;
	
} symbol;

// Expressions are described by nodes. Each node can have a unary operator applied,
// as well as the regular symbol. It's possible an operator could have a unary symbol,
// but this shouldn't happen.
typedef struct expression_node
{
	struct expression_node* parent;
	struct expression_node* left;
	struct expression_node* right;
	
	symbol* sym;
	
	char unary_minus;
	char unary_bitwise_negate;
	
} expression_node;

// parses a string and returns a parsed expression tree (malloced)
expression_node* parse(char* s);

// prints a symbol to stdout, LOG_NORMAL
void printf_symbol(symbol* s);

// prints a symbol to stdout if f is 0, otherwise default file
void printf_expression_tree(FILE* f, expression_node* e);

// frees the memory from the expression tree, and the symbols
void free_expression_node(expression_node* e);

#endif