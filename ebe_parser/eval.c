#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>

#include "expression.h"
#include "linked_list.h"
#include "helper.h"
#include "log.h"

// user guide for uthash at
// http://troydhanson.github.io/uthash/userguide.html
#include "uthash.h"

// http://openwall.info/wiki/people/solar/software/public-domain-source-code/md5
#include "md5/md5.h"

linked_list* variable_names;

// object to be hashed
typedef struct HASH_OBJECT
{
	// "name" property used as hash key
	char name[MAX_SYMBOL_SIZE];
	
	// pointer to linked list node; used to track number of
	// times variable appears, tracked inside list.
	linked_list_node* node;
	
	// count number of times variable exists inside hash
	size_t count;
	
	// internal hash handle; required
	UT_hash_handle hh;
} HASH_OBJECT;

// declare hash table
HASH_OBJECT *head = NULL;    /* important! initialize to NULL */

// number of variables discovered during consolidation
size_t variable_name_counter = 0;

// a variable fits in a slot.
// number of variables <= number of slots
size_t slot_counter = 0;

// the maximum number of bits to evaluate in a variable
size_t max_bits = 1;

uint32_t max_val = 1;
uint32_t max_val_mask = 1;

static void increment_variable_list_values(linked_list* list)
{
	linked_list_node* node = list->tail;
	uint32_t safety = list->length;
	
	while(node)
	{
		node->var_value = node->var_value + 1;
	
		if (node->var_value >= max_val)
		{
			node->var_value = 0;
		}
		else
		{
			break;
		}
		
		node = node->prev;
		
		if (safety-- + 1 == 0)
		{
			break;
		}
	}
}

static void consolidate_recursive(expression_node* e)
{
	expression_node* node = e;
	
	if (node == 0)
	{
		return;
	}
	
	char* sym_value;
	
	if (node->left == 0 && node->right == 0)
	{
		if (node->sym != 0 && node->sym->symbol_type == tk_term &&
			node->sym->value != 0 && *(node->sym->value) != 0)
		{
			sym_value = *(node->sym->value);
			
			if (is_alpha(sym_value[0]))
			{
				struct HASH_OBJECT *s;
				
				HASH_FIND_STR(head, sym_value, s);

				// search the hash table (head) for the given id (key), set pointer (s)
				// if it exists
				if (s == 0)
				{
					// discovered a new variable!
					variable_name_counter++;
					
					// doesn't exist; append item to linked list (need the
					// node pointer for the hash).
					// init_values takes (char** name, int value, int id)
					// value will be set during evaluation.
					linked_list_node* llnode =
						linked_list_node_init_values(node->sym->value, 0, variable_name_counter);
					
					llnode->count = 1;
						
					//printf("Added linked list node.\n");
					//printf("node->sym->value: '%s'\n", node->sym->value);
					
					linked_list_append(variable_names, llnode);
					
					s = (struct HASH_OBJECT*)malloc(sizeof(struct HASH_OBJECT));
					
					memset(s->name, 0, sizeof(char)*MAX_SYMBOL_SIZE);
					strcpy(s->name, sym_value);
					s->count = 1;
					s->node = llnode;
					
					HASH_ADD_STR(head, name, s );  /* name: name of key field */
				}
				else
				{
					s->count = s->count + 1;
					s->node->count = s->node->count + 1;
					
					// free existing string memory
					free(*(node->sym->value));
					
					// let expression.c know not to free this memory
					node->sym->allocated = 0;
					
					// set expression node pointer to string to point to the same string
					// as already previously found from the hash
					node->sym->value = s->node->var_name;
				}
				
				slot_counter++;
			}
		}
		
		return;
	}
	
	if (node->left != 0)
	{
		consolidate_recursive(node->left);
	}
	
	if (node->right != 0)
	{
		consolidate_recursive(node->right);
	}
}

static void consolidate_variable_pointers(expression_node* e)
{
	consolidate_recursive(e);
}

static void normalize_variables(linked_list* llist)
{
	linked_list_node* list_item = llist->head;
	
	char** ppval;
	char* pval;
	char* new_var_name;
	struct HASH_OBJECT *s;
	
	int count = 1;
	
	while(list_item)
	{
		ppval = list_item->var_name;
		pval = *ppval;
		
		if (pval != 0)
		{	
			//printf("found node '%s'\n", pval);
			
			struct HASH_OBJECT *s;
				
			HASH_FIND_STR(head, pval, s);
			
			if (s != 0)
			{
				// delete item from hash
				HASH_DEL(head, s);
				
				// create new short name	
				new_var_name = count_to_var_name(count);
			
				//printf("new name: '%s'\n", new_var_name);
			
				// free old memory
				free(pval);
			
				// update pointer to new memory
				*(list_item->var_name) = new_var_name;
			
				count++;
				
				// now need to update the hash with the new variable name
				memset(s->name, 0, sizeof(char)*MAX_SYMBOL_SIZE);
				strcpy(s->name, new_var_name);
				HASH_ADD_STR(head, name, s );  /* name: name of key field */
			}
		}
		
		list_item = list_item->next;
	}
}

int32_t eval(expression_node* e, int32_t* nan)
{
	int32_t final_value = 0;
	
	char* sym_value;
	int32_t v;
	
	if (e == 0)
	{
		felog_d(LOG_NORMAL, "Warning! evaluating empty node.\n");
		return 0;
	}
	
	if (e->sym == 0 && e->left == 0)
	{
		felog_d(LOG_NORMAL, "Warning! evaluating node with empty symbol.\n");
		return 0;
	}
	else if (e->sym == 0 && e->left != 0)
	{
		if (e->unary_minus)
		{
			return (0 - eval(e->left, nan)) & max_val_mask;
		}
		else if (e->unary_bitwise_negate)
		{
			return (~eval(e->left, nan)) & max_val_mask;
		}
		
		return eval(e->left, nan);
	}
	
	sym_value = *(e->sym->value);
	
	elog(LOG_VERBOSE, "eval '");
	if (e->unary_minus)
	{
		elog(LOG_VERBOSE, "`");
	}
	if (e->unary_bitwise_negate)
	{
		elog(LOG_VERBOSE, "~");
	}
	elog(LOG_VERBOSE, "%s'\n", sym_value);
	
	switch (e->sym->symbol_type)
	{
		case tk_term:
			if (strlen(sym_value) >= 2 && sym_value[0] == '0' && (sym_value[1] == 'x' || sym_value[1] == 'X'))
			{
				final_value =  strtoul(sym_value, 0, 16);
			}
			else
			{
				if (is_alpha(sym_value[0]))
				{
					struct HASH_OBJECT *s;
				
					HASH_FIND_STR(head, sym_value, s);
					
					if (s == 0)
					{
						elog(LOG_FATAL_ERROR, "Error, can't find symbol '%s' in hash\n", sym_value);
					}
						
					final_value =  s->node->var_value;
				}
				else
				{
					final_value =  strtoul(sym_value, 0, 10);
				}
			}
			break;
			
		case tk_operator:
			// TODO: warn on overflow
			if (is_binary_operator(sym_value[0]))
			{
				switch(sym_value[0])
				{
					//case '`': 
					//case '~': 
					case '*':
						final_value = (eval(e->left, nan) * eval(e->right, nan)) & max_val_mask;
						break;
					case '/':
						v = eval(e->right, nan);
						if (v == 0)
						{
							*nan = 1;
							final_value = 0;
						}
						else
						{
							final_value = (eval(e->left, nan) / v) & max_val_mask;
						}
						break;
					case '%':
						v = eval(e->right, nan);
						if (v == 0)
						{
							*nan = 1;
							final_value = 0;
						}
						else
						{
							final_value = (eval(e->left, nan) % v) & max_val_mask;
						}
						break;
					case '+':
						final_value = (eval(e->left, nan) + eval(e->right, nan)) & max_val_mask;
						break;
					case '-':
						final_value = (eval(e->left, nan) - eval(e->right, nan)) & max_val_mask;
						break;
					case '<':
						final_value = (eval(e->left, nan) << eval(e->right, nan)) & max_val_mask;
						break;
					case '>':
						final_value = (eval(e->left, nan) >> eval(e->right, nan)) & max_val_mask;
						break;
					case '&':
						final_value = (eval(e->left, nan) & eval(e->right, nan)) & max_val_mask;
						break;
					case '^':
						final_value = (eval(e->left, nan) ^ eval(e->right, nan)) & max_val_mask;
						break;
					case '|':
						final_value = (eval(e->left, nan) | eval(e->right, nan)) & max_val_mask;
						break;
					default:
						elog(LOG_FATAL_ERROR, "Attempting to evaluate unknown operator.\n");
						break;
				}
			}
			else if (is_unary_operator(sym_value[0]))
			{
				switch(sym_value[0])
				{
					case '`':
						final_value = (0 - final_value) & max_val_mask;
						break;
					case '~':
						final_value = ( ~ final_value) & max_val_mask;
						break;
					default:
						elog(LOG_FATAL_ERROR, "Attempting to evaluate unknown operator.\n");
						break;
				}
			}
		break;
		
		default:
			elog(LOG_FATAL_ERROR, "Attempting to evaluate unknown token.\n");
			break;
	}
	
	if (e->unary_minus)
	{
		final_value = (0 - final_value) & max_val_mask;
	}
	if (e->unary_bitwise_negate)
	{
		final_value = ( ~ final_value) & max_val_mask;
	}
	
	return final_value & max_val_mask;
}

void set_max_bits(size_t bits)
{
	max_bits = bits;
	max_val = 1 << bits;
	
	max_val_mask = max_val - 1;
	
	elog(LOG_VERBOSE, "max_val = %d\n", max_val);
	elog(LOG_VERBOSE, "max_val_mask = 0x%x\n", max_val_mask);
}

// remove whitespace, replace tokens with eval appropriate tokens
char* clean_expression(char* expr)
{
	size_t len = strlen(expr);
	
	char* cleaned_expr = (char*)malloc(sizeof(char) * len + 1);
	
	memset(cleaned_expr, 0, sizeof(char) * len + 1);
	
	char ch = 0;
	char last_ch = 0;
	char last_non_whitespace = 0;
	
	char found_something = 0;
	
	size_t expr_position = 0;
	size_t clean_position = 0;
	
	ch = expr[expr_position];
	
	while(ch)
	{
		if (!is_whitespace(ch))
		{
			if ((is_binary_operator(last_non_whitespace) || found_something == 0)
				&& ch == '-')
			{
				ch = '`';
			}
			
			if ((last_ch == '<' && ch == '<') ||
				(last_ch == '>' && ch == '>'))
			{
				last_ch = ch;
				expr_position++;
				ch = expr[expr_position];
				
				continue;
			}
			
			cleaned_expr[clean_position] = ch;
			clean_position++;
			
			found_something = 1;
			
			last_non_whitespace = ch;
		}
		
		last_ch = ch;
		
		expr_position++;
		ch = expr[expr_position];
	}
	
	return cleaned_expr;
}

void eval_main(char* expr)
{
	int32_t val;
	MD5_CTX md5_ctx;
	char md5_buffer[64];
	char md5_length;
	int i;
	int32_t nan;
	
	memset(&md5_ctx, 0, sizeof(MD5_CTX));
	
	variable_names = linked_list_init();
	
	char* cleaned_expr = clean_expression(expr);
	
	expression_node* e = parse(cleaned_expr);
	
	felog_d(LOG_NORMAL, "raw input: %s\n", expr);
	felog_d(LOG_NORMAL, "cleaned intput: %s\n", cleaned_expr);

	// don't need this anymore
	free(cleaned_expr);
	
	// consolidate and count the number of non-constant variables
	consolidate_recursive(e);
	
	// normalize variables names to: a,b,c,d,e...
	normalize_variables(variable_names);
	
	felog_d(LOG_NORMAL, "parsed intput: ");
	printf_expression_tree(0, e);
	
	felog_d(LOG_NORMAL, "variables: %d\n", variable_name_counter);
	felog_d(LOG_NORMAL, "slots: %d\n", slot_counter);
	felog_d(LOG_NORMAL, "max_bits: %d\n", max_bits);
	
	MD5_Init(&md5_ctx);
	
	memset(md5_buffer, 0, sizeof(char)*64);
	md5_length = 0;
	
	if (variable_name_counter > 0)
	{
		printf_linked_list(variable_names);
	
		uint32_t overflow = 0;
		uint32_t max_iterations = lazy_pow(max_val, variable_name_counter, &overflow);
		
		if (overflow)
		{
			elog(LOG_EXIT_ERROR, "Too many combinations for max_bits=%d (max_val=%d), variable_name_counter=%d\n",
				max_bits, max_val, variable_name_counter);
			goto free_quit;
		}
		
		elog(LOG_VERBOSE, "Evaluating all (%d) combinations\n", max_iterations);
		
		size_t iter_count = 0;
		for(; iter_count < max_iterations; iter_count++)
		{
			elog(LOG_VERBOSE, "\n");
			printf_linked_list(variable_names);
			
			nan = 0;
			val = eval(e, &nan);
			
			memset(md5_buffer, 0, sizeof(char)*64);
			md5_length = 0;
			
			if (nan == 0)
			{
				sprintf(md5_buffer, "%d,", val);
			}
			else
			{
				sprintf(md5_buffer, "n,");
			}
			
			md5_length = strlen(md5_buffer);
			
			MD5_Update(&md5_ctx, md5_buffer, md5_length);

			if (nan == 0)
			{
				felog_d(LOG_NORMAL, "%d,", val);
			}
			else
			{
				felog_d(LOG_NORMAL, "n,");
			}
			
			if (is_using_file())
			{
				if (nan == 0)
				{
					elog(LOG_VERBOSE, "%d\n", val);
				}
				else
				{
					elog(LOG_VERBOSE, "n\n");
				}
			}

			increment_variable_list_values(variable_names);
		}
		felog_d(LOG_NORMAL, "\n");
	}
	else
	{
		nan = 0;
		val = eval(e, &nan);
		
		if (nan == 0)
		{
			felog_d(LOG_NORMAL, "%d,\n", val);
		}
		else
		{
			felog_d(LOG_NORMAL, "n,\n");
		}
	}
	
	memset(md5_buffer, 0, sizeof(char)*64);
	MD5_Final(md5_buffer, &md5_ctx);
	

	felog_d(LOG_NORMAL, "eval md5: ");
	
	while(i<16)
	{
		felog_d(LOG_NORMAL, "%.02x", (unsigned char)(md5_buffer[i]));
		i++;
	}
	
	felog_d(LOG_NORMAL, "\n");

	
free_quit:

	free_expression_node(e);
	linked_list_free(variable_names);
}