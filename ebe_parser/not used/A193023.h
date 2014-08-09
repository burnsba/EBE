#ifndef __A193023_H__
#define __A193023_H__

#define BELL_SUM_SEQUENCE_LENGTH 23

#define BELL_SEQUENCE_LENGTH 26

// Set partitions of the set {1,2,3,...,n}
typedef struct setpart
{
        // Number of elements of set (set = {1,2,3,...,n})
        int32_t set_length;

        // p[] contains set partitions of length 1,2,3,...,n
        int32_t* set_partition;

        // pp[k] points to start of set partition k
        int32_t** start_of_set_partition;

        // ns[k] Number of Sets in set partition k
        int32_t* number_of_sets;

        // element k attached At Set (0<=as[k]<=k) of set(k-1)
        int32_t* attached_at_set;

        // current set partition (==pp[n])
        int32_t* current_set_partition;
    
} setpart;

// A path down the A193023 tree can be described by a seuqnce of values, one for each
// item visited. Since every node has unique children, only the value of the current node
// needs to be stored, not the entire path.
// Examples:
// A193023(1): node->values = {1}
// A193023(2): node->values = {1, 1}
// A193023(3): node->values = {1, 2}
// A193023(4): node->values = {1, 1, 1}
typedef struct node_description
{
        // array containing the path to the current node
        uint64_t values[BELL_SEQUENCE_LENGTH];
        
        // number of values in array
        uint32_t len;
        
        // index of current iteration
        uint64_t curr;
        
        setpart* sp;
} node_description;

// returns the nth item in the sequence sum
uint64_t get_bell_sum(uint64_t i);

// returns the nth item in the sequence
uint64_t get_bell(uint64_t i);

// iterates to the next combination
void node_next(node_description* node);

// Clears a node_description. Memory is freed for the node and values
// array. Members are reset to default values.
void node_free(node_description* node);

// Returns the path to the nth item in the sequence A193023. Memory is allocated
// which should be freed with node_free.
node_description* A193023(uint64_t n);

#endif