#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include "log.h"
#include "linked_list.h"

linked_list_node* linked_list_node_init()
{
	linked_list_node* node = (linked_list_node*)malloc(sizeof(linked_list_node));
	
	if (node == 0)
	{
		elog(LOG_FATAL_ERROR, "linked_list_node_init: Out of memory\n");
	}
	
	node->next = 0;
	node->prev = 0;
	
	node->var_name = 0;
	node->var_value = 0;
	node->count = 0;
	node->id = 0;
	
	return node;
}

linked_list_node* linked_list_node_init_values(char** name, size_t value, size_t id)
{
	linked_list_node* node = linked_list_node_init();
	
	//size_t len = strlen(*name);
	
	node->var_name = name;
	
	//printf("Setting var_name to '%s'\n", *name);
	
	node->var_value = value;
	node->id = id;
	
	return node;
}

linked_list* linked_list_init()
{
	linked_list* list = (linked_list*)malloc(sizeof(linked_list));
	
	if (list == 0)
	{
		elog(LOG_FATAL_ERROR, "linked_list_init: Out of memory\n");
	}
	
	list->head = 0;
	list->tail = 0;
	
	list->length = 0;
	
	return list;
}

void linked_list_free(linked_list* list)
{
	if (list == 0)
	{
		return;
	}
	
	linked_list_node* node;
	linked_list_node* next_node;
	
	node = list->head;
	
	while (node)
	{
		next_node = node->next;
		free(node);
		node = next_node;
	}
	
	list->head = 0;
	list->tail = 0;
	list->length = 0;
	
	free(list);
}

void linked_list_append(linked_list* list, linked_list_node* node)
{
	if (list == 0 || node == 0)
	{
		return;
	}
	
	if (list->length == 0)
	{
		list->head = node;
		list->tail = node;
		
		list->head->next = 0;
		list->head->prev = 0;
		list->tail->next = 0;
		list->tail->prev = 0;
	}
	else
	{
		// if there's only one item in the list
		// then list->head = list->tail
		
		node->prev = list->tail;
		node->next = 0;
		
		list->tail->next = node;
		
		list->tail = node;
	}
	
	list->length++;
}

void printf_linked_list(linked_list* list)
{
	if (list == 0)
	{
		elog(LOG_VERBOSE, "List is null\n");
		return;
	}
	
	if (list->length == 0)
	{
		elog(LOG_VERBOSE, "List is empty\n");
		return;
	}
	
	linked_list_node* node = list->head;
	size_t count = 0;
	
	do
	{
		count++;
		elog(LOG_VERBOSE, "Node %d: '%s'=%d, count=%d, id=%d\n", count, *(node->var_name), node->var_value, node->count, node->id);
	} while (node = node->next);
	
}