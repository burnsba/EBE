#ifndef __HELPER_H__
#define __HELPER_H__

#include <stdint.h>

// notes on operators:
// hard to distinguish between binary and unary operators;
// Also, not going to figure out how to parse multi-chacter operators (<<, >>)
// Therefore, the following operators are defined (listed in order of precendence):
//
// ` (grave) unary minus
// ~ unary bitwise not
// 
// (the rest are binary operators)
// 
// * times
// / integer divide
// % modulus
// + addition
// - subtraction
// < shift left
// > shift right
// & bitwise AND
// ^ bitwise XOR
// | bitwise OR

// lazy exponentiation for integers
// Multiples base times itself exp times.
// If the result is larger than can fit in uint32, overflow is set to 1
uint32_t lazy_pow(uint32_t base, uint32_t exp, uint32_t* overflow);

// Checks if a character is an alphabetic character or underscore
// Returns 1 if: a-z or A-Z or _
// Otherwise 0
int is_alpha(char c);

// Checks if a character is numeric
// Returns 1 if '0'-'9'
// Otherwise 0
int is_numeric(char c);

// Checks if a character is alphanumeric.
// Combination of is_alpha and is_numeric
int is_alphanumeric(char c);

// Checks if a character is a space, tab or newline
// Returns 1 if: ' ' or \t or \n or \r
// Otherwise 0
int is_whitespace(char c);

// Checks if a character is a binary operator as interpreted by the parser
// Returns 1 if: */%+-<>&^|
// Otherwise 0
int is_binary_operator(char c);

// Checks if a character is a unary operator as interpreted by the parser
// Returns 1 if: `-
// Otherwise 0
int is_unary_operator(char c);

// Checks if a character is an operator.
// Combination of is_binary_operator and is_unary_operation
int is_operator(char c);

// Determines if the character is a parenthesis character.
// Returns 1 for: ({[]})
// Otherwise returns 0
int is_paren(char c);

// determines if the left operator is lower, same, or higher precendence than the right
// -1 lower
// 0 same
// 1 higher
int operator_precendence(char left, char right);

// Determines if the character is an open parenthesis character.
// Returns 1 for: ({[
// Otherwise returns 0
int is_open_paren(char c);

// Determines if the character is a close parenthesis character.
// Returns 1 for: ]})
// Otherwise returns 0
int is_close_paren(char c);

char* count_to_var_name(uint32_t count);

#endif