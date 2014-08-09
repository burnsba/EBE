#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "stack.h"

void stack_init(stack* s)
{
	stack_item *new_contents;

	new_contents = (stack_item *)malloc(sizeof(stack_item) * MAX_STACK_SIZE);
	
	s->contents = new_contents;
	s->top = -1;
}

void stack_free(stack* s)
{
	free(s->contents);
	
	s->top = -1;
}

int stack_is_empty(stack* s)
{
	return s->top < 0;
}

int stack_is_full(stack* s)
{
	return s->top >= MAX_STACK_SIZE - 1;
}

void stack_push(stack* s, stack_item item)
{
	if (stack_is_full(s))
	{
		fprintf(stderr, "Can't push element on stack: stack is full.\n");
		exit(1);
	}
	
	s->contents[++s->top] = item;
}

stack_item stack_pop(stack* s)
{
	if (stack_is_full(s))
	{
		fprintf(stderr, "Can't pop element from stack: stack is empty.\n");
		exit(1);
	}

	return s->contents[s->top--];
}