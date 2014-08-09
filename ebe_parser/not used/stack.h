#include <stdlib.h>

#ifndef __STACK_H__
#define __STACK_H__

#define MAX_STACK_SIZE 100

typedef char* stack_item;

typedef struct stack {

	stack_item *contents;
	int top;

} stack;

void stack_init(stack* s);

void stack_free(stack* s);

int stack_is_empty(stack* s);

int stack_is_full(stack* s);

void stack_push(stack* s, stack_item item);

stack_item stack_pop(stack* s);

#endif