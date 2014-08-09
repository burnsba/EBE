#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>

#include "A071159.h"

// Will calculate nth item in A071159 sequence.
// https://oeis.org/A071159
// "Integers whose decimal expansion start with 1, do not contain zeros and 
// each successive digit to the right is at most one greater than the previous."
// 1, 11, 12, 111, 112, 121, 122, 123, 1111, ...
// 
// A little bit easier to understand with a graph:
//                               1
//           11-------------------------------------12
//       111----112                     121---------122----------123
// 1111-1112    1121-1122-1123    1211-1212    1221-1222-1223    1231-1232-1233-1234
//
// The graph also explains my notion of a row which I use in the code below.

// Path down the tree is described in an array, initially with this many elements.
// When the array is resized, this many additional positions are added for use.
#define node_chunk_size 100

// Helper macro to retrieve the last item in the node values array
#define node_last(X) (X->values[X->curr])

// Catalan numbers: C(n) = binomial(2n,n)/(n+1) = (2n)!/(n!(n+1)!). Also called Segner numbers.
// https://oeis.org/A000108
// ignore zero'th element
static uint64_t catalan_sequence[] = {1, 1, 2, 5, 14, 42, 132, 429, 1430, 4862, 16796, 58786, 208012, 742900, 2674440, 9694845, 35357670, 129644790, 477638700, 1767263190, 6564120420, 24466267020, 91482563640, 343059613650, 1289904147324, 4861946401452, 18367353072152, 69533550916004, 263747951750360, 1002242216651368, 3814986502092304};

#define CATALAN_SEQUENCE_LENGTH 31

// Partial sums of (Catalan numbers starting 1,2,5,...), cf. A000108.
// https://oeis.org/A014138
// ignore zero'th element
static uint64_t catalan_sequence_sum[] = {1, 1, 3, 8, 22, 64, 196, 625, 2055, 6917, 23713, 82499, 290511, 1033411, 3707851, 13402696, 48760366, 178405156, 656043856, 2423307046, 8987427466, 33453694486, 124936258126, 467995871776, 1757900019100, 6619846420552, 24987199492704, 94520750408708, 358268702159068, 1360510918810436};

#define CATALAN_SUM_SEQUENCE_LENGTH 30

// safety check
#define MAX_RECURSION 30

// returns the nth item in the sequence sum
uint64_t get_catalan_sum(uint64_t i)
{
        if (i >= CATALAN_SUM_SEQUENCE_LENGTH)
        {
                return 0;
        }
        
        return catalan_sequence_sum[i];
}

// returns the nth item in the sequence
uint64_t get_catalan(uint64_t i)
{
        if (i >= CATALAN_SEQUENCE_LENGTH)
        {
                return 0;
        }
        
        return catalan_sequence[i];
}

// Initializes a new node_description. Memory is allocated for the node
// and values and set to zero. Members are initialized to default values.
static node_description* node_init()
{
        node_description* node = (node_description*)malloc(sizeof(node_description));
        
        if (node == 0)
        {
                printf("node_init: out of memory 1\n");
                exit(1);
        }

        memset(node, 0, sizeof(node_description));
        
        node->curr = (uint32_t)(-1);
        node->max_len = node_chunk_size;
        
        node->values = (uint64_t*)malloc(sizeof(uint64_t)*node->max_len);
        
        if (node->values == 0)
        {
                printf("node_init: out of memory 2\n");
                exit(1);
        }

        return node;
}

// Clears a node_description. Memory is freed for the node and values
// array. Members are reset to default values.
void node_free(node_description* node)
{
        if (node == 0)
        {
                return;
        }
        
        node->curr = -1;
        node->max_len = 0;

        if (node->values != 0)
        {
                free(node->values);
                node->values = 0;
        }

        free(node);
        node = 0;
}

// Appends an item to the end of the array and increments the curr position, 
// resizing the array if necessary by calling realloc. If the array is
// resized, the max_len is increased.
static void append(node_description* node, uint64_t item)
{
        node->curr = node->curr + 1;
        
        if (node->curr == node->max_len)
        {
                node->max_len = node->max_len + node_chunk_size;
                
                node->values = (uint64_t*)realloc(node->values, sizeof(uint64_t)*node->max_len);
                
                if (node->values == 0)
                {
                        printf("append: out of memory\n");
                        exit(1);
                }
        }
        
        node->values[node->curr] = item;
}

// Given the nth number of the sequence, returns the row the object resides in.
// Rows begin at 1.
static uint64_t catalan_row(uint64_t n)
{
        uint64_t i;
        for(i=1; i<CATALAN_SUM_SEQUENCE_LENGTH; i++)
        {
                if (catalan_sequence_sum[i] >= n)
                {
                        return i;
                }
        }
        
        return 0;
}

// Given the nth item in the sequence, it returns the position of the object on the row
// it resides in. The first item starts at 1.
static uint64_t position_on_row(uint64_t n)
{
        uint64_t position_on_row = 0;
        uint64_t i;
        
        // max value needs to be the lesser of the two array lengths
        for(i=1; i<CATALAN_SUM_SEQUENCE_LENGTH; i++)
        {
                if (catalan_sequence_sum[i] > n)
                {
                        position_on_row = n - catalan_sequence_sum[i - 1];
                        
                        // if it's the last item on the row, position_on_row
                        // will be zero but it actually needs to be set to
                        // catalan_sequence
                        if (position_on_row == 0)
                        {
                                position_on_row = catalan_sequence[i - 1];
                        }

                        break;
                }
        }
        
        return position_on_row;
}

// Counts the number of children at some number of rows below the given node, 
// ignoring in-between children.
// Depth is the number of levels below the given node to count children; minimum
// acceptable value is 1, and max is arbitrarily chosen to be 30.
static uint64_t count_children_on_row(uint64_t node_val, uint32_t depth)
{
                if (depth < 1 || depth > MAX_RECURSION)
                {
                        printf("Depth (%d) out of range.", depth);
                        exit(1);
                }
                
                uint64_t i;
                                
                // number of children found
                uint64_t children = 0;
                
                // If only looking at the children immediately below the current node,
                // that's simply one more than the current value.
                if (depth == 1)
                {
                        children += node_val + 1;
                }
                else
                {
                        // otherwise, all of the grand-child nodes need to be counted;
                        // and their children, and their children, etc
                        for(i=1; i<node_val+2; i++)
                        {
                                // BTW, to count the intermediary nodes, and not just
                                // the nodes on the final row, change this to
                                // children += 1 + ...
                                children += count_children_on_row(i, depth - 1);
                        }
                }
                
                return children;
}

// Recursive helper for A071159().
//
// The function starts at the top of the tree and for each child counts the
// number of children on the target row. When the children count exceeds
// the target position it is then known that the path to the target must
// fall under the current child. The target is re-adjusted per the child
// count then the function continues on the next row.
//
// node: current working node
// current_depth: current row depth
// target_depth: terminal or max row depth
// target: target position on row
static node_description* A071159_r(node_description* node, uint64_t current_depth, uint64_t target_depth, uint64_t target)
{
        if (current_depth == target_depth)
        {
                return node;
        }
        
        if (current_depth + 1 == target_depth)
        {
                append(node, target);
                return node;
        }
        
        if (current_depth > MAX_RECURSION)
        {
                printf("Too much recursion.");
                exit(1);
        }
        
        uint64_t i;
                
        // last element in array
        uint64_t num = node_last(node);
        
        uint64_t children = 0;
        uint64_t count = 0;
        
        // need to look at every child
        for(i=1; i<num+2; i++)
        {
                uint64_t children_count = count_children_on_row(i, target_depth - current_depth - 1);
                
                // special case for exactly lining up with a catalan number
                if (target == count + children_count)
                {
                        append(node, i);
                        return A071159_r(node, current_depth + 1, target_depth, children_count);
                }
                // otherwise it was found in the current check
                else if (target < count + children_count)
                {
                        append(node, i);
                        return A071159_r(node, current_depth + 1, target_depth, target - count);
                }
                
                count += children_count;
        }
        
        printf("Shouldn't get here.");
        exit(1);
}

// Returns the path to the nth item in the sequence A071159. Memory is allocated
// which should be freed with node_free.
node_description* A071159(uint64_t n)
{
        uint64_t row = catalan_row(n);
        uint64_t position = position_on_row(n);
        
        node_description* node = node_init();
        
        // seed with root value
        append(node, 1);
        
        return A071159_r(node, 1, row, position);
}

/*
int main()
{
        node_description* node = A071159(22);
        
        uint64_t i;
        
        printf("node: ");
        
        for(i=0; i<node->curr; i++)
        {
                printf("%d, ", node->values[i]);
        }
        
        printf("%d", node->values[node->curr]);
        printf("\n");
        
        node_free(node);
        
        return 0;
}
*/