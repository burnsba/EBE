#ifndef __LINKED_LIST_H__
#define __LINKED_LIST_H__

#include <stdint.h>

// Doubly linked list node.
// Tailored for use in expression parsing and evaluation
typedef struct linked_list_node
{
	struct linked_list_node* next;
	struct linked_list_node* prev;
	
	// Pointer to string containing variable name
	char** var_name;
	
	// Value of the variable
	size_t var_value;
	
	// number of times this variable has appeared
	size_t count;
	
	// Unique id of the variable
	size_t id;
	
} linked_list_node;

// Doubly linked list structure
typedef struct linked_list
{
	// very first item; there is no head->prev
	struct linked_list_node* head;
	
	// very last item; there is no tail->next
	struct linked_list_node* tail;
	
	size_t length;
	
} linked_list;

// Mallocs a new node initializing the contents to 0
linked_list_node* linked_list_node_init();

// Mallocs a new node initializing the contents to 0.
// Name, value, and id of the node are set according to parameters
linked_list_node* linked_list_node_init_values(char** name, size_t value, size_t id);

// Mallocs a new linked list
linked_list* linked_list_init();

// Frees the linked list and all of its nodes
void linked_list_free(linked_list* list);

// Appends an item to the end of the linked list
void linked_list_append(linked_list* list, linked_list_node* node);

// Prints the contents of each node in the linked list.
void printf_linked_list(linked_list* list);

#endif