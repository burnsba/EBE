#ifndef __A071159_H__
#define __A071159_H__

// A path down the A071159 tree can be described by a seuqnce of values, one for each
// item visited. Since every node has unique children, only the value of the current node
// needs to be stored, not the entire path.
// Examples:
// A071159(1): node->values = {1}
// A071159(2): node->values = {1, 1}
// A071159(3): node->values = {1, 2}
// A071159(4): node->values = {1, 1, 1}
typedef struct node_description
{
        // array containing the path to the current node
        uint64_t* values;
        
        // current/last item in the values array
        uint32_t curr;
        
        // maximum number of values that can currently be stored
        uint32_t max_len;
} node_description;

// returns the nth item in the sequence sum
uint64_t get_catalan_sum(uint64_t i);

// returns the nth item in the sequence
uint64_t get_catalan(uint64_t i);

// Clears a node_description. Memory is freed for the node and values
// array. Members are reset to default values.
void node_free(node_description* node);

// Returns the path to the nth item in the sequence A071159. Memory is allocated
// which should be freed with node_free.
node_description* A071159(uint64_t n);

#endif