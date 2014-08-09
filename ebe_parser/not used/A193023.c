#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>

#include "A193023.h"

// Will calculate nth item in A193023 sequence.
// http://oeis.org/A193023
// "contains all set partitions of an n-set in canonical order."
// 1, 11, 12, 111, 112, 121, 122, 123, 1111, 1112, 1121, 
// 1122, 1123, 1211, 1212, 1213, 1221, 1222, 1223, 1231, 1232, 1233, 1234



static int32_t c_set_partitionappend(int32_t* src, int32_t* dst, int32_t k, int32_t a)
// Copy partition in src[0,...,k-2] to dst[0,...,k-1]
// append element k at subset a (a>=0)
// Return number of sets in created partition.
{
        int32_t counter = 0;
        int32_t j;

        for (j=0; j < k - 1; ++j)
        {
                int32_t e = src[j];

        if (e > 0)
        {
                dst[j] = e;
        }
        else
        {
                if (a == counter)
                {
                        dst[j] = -e;
                        ++dst;
                        dst[j] = -k;
                }
                else
                {
                        dst[j] = e;
                }

                ++counter;
                }
        }

        if (a >= counter)
        {
                dst[k - 1] = -k;
                ++counter;
        }

        return counter;
}

static void first(setpart* sp)
{
        sp->set_partition[0] = -1;

        int32_t k;

        for (k=0; k <= sp->set_length; ++k)
        {
                sp->attached_at_set[k] = 0;
                sp->number_of_sets[k] = 1;
        }

        for (k=2; k <= sp->set_length; ++k)
        {
                c_set_partitionappend(
                        sp->start_of_set_partition[k-1],
                        sp->start_of_set_partition[k],
                        k,
                        sp->attached_at_set[k]);
        }
}

static int32_t next_rec(setpart* sp, int32_t k)
// Update partition in level k from partition in level k-1  (k<=n)
// Return number of sets in created partition
{
        // current is last
        if (k <= 1)
        {
                return 0;
        }

        int32_t as = sp->attached_at_set[k] + 1;

        // have to recurse
        if (as > sp->number_of_sets[k-1])
        {
                int32_t ns1 = next_rec(sp, k - 1);

                if (0 == ns1)
                {
                        return 0;
                }

                as = 0;
        }
        
        sp->attached_at_set[k] = as;

        int32_t ns = c_set_partitionappend(
                sp->start_of_set_partition[k - 1],
                sp->start_of_set_partition[k],
                k,
                as);

        sp->number_of_sets[k] = ns;

        return  ns;
}

// Allocates memory for a setpart and initializes to default
// values.
// len is the set length
static setpart* setpart_init(int32_t len)
{
        int32_t k;
        setpart* sp = (setpart*)malloc(sizeof(setpart));

        sp->set_length = len;

        int32_t np = (len * (len + 1)) / 2;

        sp->set_partition = (int32_t*)malloc(sizeof(int32_t) * np);

        int32_t n1 = sp->set_length + 1;
        sp->start_of_set_partition = (int32_t**)malloc(sizeof(int32_t*) * n1);
        sp->start_of_set_partition[0] = 0;  // unused
        sp->start_of_set_partition[1] = sp->set_partition;

        for (k=2; k <= len; k++)
        {
                sp->start_of_set_partition[k] = sp->start_of_set_partition[k-1] + k - 1;
        }

        sp->number_of_sets = (int32_t*)malloc(sizeof(int32_t) * n1);

        sp->attached_at_set = (int32_t*)malloc(sizeof(int32_t) * n1);

        sp->current_set_partition = sp->start_of_set_partition[sp->set_length];

        first(sp);

        return sp;
}

static void setpart_free(setpart* sp)
{
        int32_t k;

        if (sp == 0)
        {
                return;
        }

        if (sp->attached_at_set != 0)
        {
                free(sp->attached_at_set);
                sp->attached_at_set = 0;
        }

        if (sp->number_of_sets != 0)
        {
                free(sp->number_of_sets);
                sp->number_of_sets = 0;
        }

        if (sp->start_of_set_partition != 0)
        {
                free(sp->start_of_set_partition);
                sp->start_of_set_partition = 0;
        }

        free(sp);
        sp = 0;
}

// Bell or exponential numbers: number of ways to partition a set of n labeled elements. 
// http://oeis.org/A000110
// ignore zero'th element
static uint64_t bell_sequence[] = {1, 1, 2, 5, 15, 52, 203, 877, 4140, 21147, 115975, 678570, 4213597, 27644437, 190899322, 1382958545, 10480142147, 82864869804, 682076806159, 5832742205057, 51724158235372, 474869816156751, 4506715738447323, 44152005855084346, 445958869294805289, 4638590332229999353};

// a(n) = sum of S(i,j), 1<=j<=i<=n, where S(i,j) are Stirling numbers of the second kind.
// http://oeis.org/A024716
// ignore zero'th element
static uint64_t bell_sequence_sum[] = {1, 1, 3, 8, 23, 75, 278, 1155, 5295, 26442, 142417, 820987, 5034584, 32679021, 223578343, 1606536888, 12086679035, 94951548839, 777028354998, 6609770560055, 58333928795427, 533203744952178, 5039919483399501, 49191925338483847, 495150794633289136};

// returns the nth item in the sequence sum
uint64_t get_bell_sum(uint64_t i)
{
        if (i >= BELL_SUM_SEQUENCE_LENGTH)
        {
                return 0;
        }
        
        return bell_sequence_sum[i];
}

// returns the nth item in the sequence
uint64_t get_bell(uint64_t i)
{
        if (i >= BELL_SEQUENCE_LENGTH)
        {
                return 0;
        }
        
        return bell_sequence[i];
}

// Given the nth number of the sequence, returns the row the object resides in.
// Rows begin at 1.
static uint64_t bell_row(uint64_t n)
{
        uint64_t i;
        for(i=1; i<BELL_SUM_SEQUENCE_LENGTH; i++)
        {
                if (bell_sequence_sum[i] >= n)
                {
                        return i;
                }
        }
        
        return 0;
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
        
        node->len = 0;
        node->sp = 0;

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
        
        node->len = 0;

        memset(node, 0, sizeof(node_description));
        
        setpart_free(node->sp);
        node->sp = 0;

        free(node);
        node = 0;
}

void node_next(node_description* node)
{
        setpart* sp = node->sp;
        
        next_rec(sp, sp->set_length);
        
        uint32_t j;
        for (j=1; j <= sp->set_length; j++)
        {
                node->values[j - 1] = sp->attached_at_set[j];
	}
}

// Returns the path to the nth item in the sequence A193023. Memory is allocated
// which should be freed with node_free.
node_description* A193023(uint64_t n)
{
        uint64_t row = bell_row(n);
        
        node_description* node = node_init();
        
        uint64_t starting_n = n - bell_sequence_sum[row];
        
        node->sp = setpart_init(row);
        node->len = row;
        
        return node;
}