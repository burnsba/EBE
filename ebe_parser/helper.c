#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>

uint32_t lazy_pow(uint32_t base, uint32_t exp, uint32_t* overflow)
{
	if (exp == 0)
	{
		return 1;
	}
	
	if (exp == 1)
	{
		return base;
	}
	
	if (base == 0)
	{
		return 0;
	}
	
	if (base == 1)
	{
		return 1;
	}
	
	uint64_t result = base;
	
	*overflow = 0;
	
	int i;
	for(i=1; i<exp; i++)
	{
		result *= base;
		
		if (result > UINT32_MAX)
		{
			*overflow = 1;
		}
	}
	
	return (uint32_t)(result & 0xffffffff);
}

int is_alpha(char c)
{
	return (c >= 'a' && c <= 'z' || c >= 'A' && c<= 'Z' || c == '_');
}

int is_numeric(char c)
{
	return (c >= '0' && c <= '9');
}

int is_alphanumeric(char c)
{
	return is_alpha(c) || is_numeric(c);
}

int is_whitespace(char c)
{
	return (c == ' ' || c == '\t' || c == '\n' || c == '\r');
}

int is_binary_operator(char c)
{
	return (c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || 
		c == '<' || c == '>' || c == '&' || c == '^' || c == '|');
}

int is_unary_operator(char c)
{
	return (c == '`' || c == '~');
}

int is_operator(char c)
{
	return is_unary_operator(c) || is_binary_operator(c);
}

int is_open_paren(char c)
{
	return (c == '(' || c == '{' || c == '[');
}

int is_close_paren(char c)
{
	return (c == ')' || c == '}' || c == ']');
}

int is_paren(char c)
{
	return is_open_paren(c) || is_close_paren(c);
}

// determines if the left operator is lower, same, or higher precendence than the right
// -1 lower
// 0 same
// 1 higher
int operator_precendence(char left, char right)
{
	switch(left)
	{
		case '`':
			switch(right)
			{
				case '`': return 0; break;
				case '~': return 1; break;
				case '*': return 1; break;
				case '/': return 1; break;
				case '%': return 1; break;
				case '+': return 1; break;
				case '-': return 1; break;
				case '<': return 1; break;
				case '>': return 1; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '~':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return 0; break;
				case '*': return 1; break;
				case '/': return 1; break;
				case '%': return 1; break;
				case '+': return 1; break;
				case '-': return 1; break;
				case '<': return 1; break;
				case '>': return 1; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '*':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return 0; break;
				case '/': return 1; break;
				case '%': return 1; break;
				case '+': return 1; break;
				case '-': return 1; break;
				case '<': return 1; break;
				case '>': return 1; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '/':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return 0; break;
				case '%': return 1; break;
				case '+': return 1; break;
				case '-': return 1; break;
				case '<': return 1; break;
				case '>': return 1; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '%':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return -1; break;
				case '%': return 0; break;
				case '+': return 1; break;
				case '-': return 1; break;
				case '<': return 1; break;
				case '>': return 1; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '+':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return -1; break;
				case '%': return -1; break;
				case '+': return 0; break;
				case '-': return 1; break;
				case '<': return 1; break;
				case '>': return 1; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '-':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return -1; break;
				case '%': return -1; break;
				case '+': return -1; break;
				case '-': return 0; break;
				case '<': return 1; break;
				case '>': return 1; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '<':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return -1; break;
				case '%': return -1; break;
				case '+': return -1; break;
				case '-': return -1; break;
				case '<': return 0; break;
				case '>': return 1; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '>':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return -1; break;
				case '%': return -1; break;
				case '+': return -1; break;
				case '-': return -1; break;
				case '<': return -1; break;
				case '>': return 0; break;
				case '&': return 1; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '&':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return -1; break;
				case '%': return -1; break;
				case '+': return -1; break;
				case '-': return -1; break;
				case '<': return -1; break;
				case '>': return -1; break;
				case '&': return 0; break;
				case '^': return 1; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '^':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return -1; break;
				case '%': return -1; break;
				case '+': return -1; break;
				case '-': return -1; break;
				case '<': return -1; break;
				case '>': return -1; break;
				case '&': return -1; break;
				case '^': return 0; break;
				case '|': return 1; break;
				default: return -2; break;
			}
		break;
		case '|':
			switch(right)
			{
				case '`': return -1; break;
				case '~': return -1; break;
				case '*': return -1; break;
				case '/': return -1; break;
				case '%': return -1; break;
				case '+': return -1; break;
				case '-': return -1; break;
				case '<': return -1; break;
				case '>': return -1; break;
				case '&': return -1; break;
				case '^': return -1; break;
				case '|': return 0; break;
				default: return -2; break;
			}
		break;
		default:
			return -2;
		break;
	}
}

char* count_to_var_name(uint32_t count)
{
	char* name;

	if (count < 27)
	{
		// 1 returns 'a'
		// 2 returns 'b' etc
		name = (char*)malloc(sizeof(char) + 1);
		
		name[0] = 'a' + count - 1;
		name[1] = '\0';
		
		return name;
	}
	
	if (count > 26 && count < 677)
	{
		// 27 returns 'aa'
		// 28 returns 'ab'
		name = (char*)malloc(sizeof(char)*2 + 1);
		
		name[0] = 'a' + (count / 26) - 1;
		name[1] = 'a' + (count % 26) - 1;
		name[2] = '\0';
		
		return name;
	}
	
	printf("Error, can't resolve %d to a variable\n", count);
        exit(1);
}