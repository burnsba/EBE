#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <stdint.h>

typedef struct ipe {
    int32_t* state;
    uint32_t len;
} ipe;

ipe* ipe_init(uint32_t n)
{
    ipe* p = (ipe*)malloc(sizeof(ipe));
    
    if (p == 0)
    {
        printf("ipe_init: out of memory\n");
        exit(1);
    }
    
    p->len = n;
    
    p->state = (int32_t*)malloc(sizeof(int32_t)*n);
    
    if (p->state == 0)
    {
        printf("ipe_init: out of memory 2\n");
        exit(1);
    }
    
    memset(p->state, 0, sizeof(int32_t)*n);
    
    return p;
}

void ipe_free(ipe* p)
{
    if (p == 0)
    {
        return;
    }
    
    if (p->state != 0)
    {
        free(p->state);
        p->state = 0;
    }
    
    free(p);
    p = 0;
}

void ipe_printf(ipe* p)
{
    int32_t len = p->len;
        
    int32_t i,j;
    
    for(i=0; i<len - 1; i++)
    {
        if (p->state[i] < 0)
        {                       
            for (j = p->state[i]; j<0; j++)
            {
                printf("(");
            }
            
            printf("s:");
        }
        else if (p->state[i] > 0)
        {
            printf("s");
            
            for (j = p->state[i]; j>0; j--)
            {
                printf(")");
            }
            
            printf(":");
        }
        else
        {
            printf("s:");
        }
    }
    
    if (p->state[i] < 0)
    {                       
        for (j = p->state[i]; j<0; j++)
        {
            printf("(");
        }
        
        printf("s");
    }
    else if (p->state[i] > 0)
    {
        printf("s");
        
        for (j = p->state[i]; j>0; j--)
        {
            printf(")");
        }
    }
    else
    {
        printf("s");
    }
    
    printf("\n");
}

void iter_eps(ipe* e, uint32_t sub_start, uint32_t sub_len)
{
    int32_t choose;
    int32_t times;
    
    int32_t choose_start = 0;
    int32_t choose_position[sub_len];
    
    int32_t choose_count;
    
    int32_t i,j;
    
    for (choose=sub_len; choose>=2; choose--)
    {
        uint32_t f = (uint32_t)((uint32_t)sub_len/(uint32_t)choose);
        f++;
        for (times=1; times<=f; times++)
        {
            if (choose*times > sub_len)
            {
                    continue;
            }
            
            choose_start = sub_start;
            
            for (i=0; i<sub_len; i++)
            {
                    choose_position[i] = -1;
            }

            for (i=0; i<times; i++)
            {
                    choose_position[i] = choose_start;
                    choose_start += choose;                     
            }
            
            int32_t do_the_choose = 1;
            
            if (choose*times == sub_len && times == 1)
            {
                    do_the_choose = 0;
            }
            
            while (do_the_choose)
            {
                for (choose_count = 0; choose_count < times; choose_count++)
                {
                    e->state[choose_position[choose_count]]--;
                    e->state[choose_position[choose_count] + choose - 1]++;        
                }
                
                ipe_printf(e);
                
                if (choose > 2 && choose < sub_len)
                {
                    for (choose_count = 0; choose_count < times; choose_count++)
                    {                                        
                        iter_eps(e, choose_position[choose_count], choose);        
                    }
                }

                // undo state increment
                for (choose_count = 0; choose_count < times; choose_count++)
                {
                    e->state[choose_position[choose_count]]++;
                    e->state[choose_position[choose_count] + choose - 1]--;
                }
                
                int32_t inc_position = 1;
                int32_t curr = times;

                while (inc_position)
                {
                    curr--;
                    
                    choose_position[curr]++;
                    
                    for (i=curr + 1; i<times && i<sub_len; i++)
                    {
                        choose_position[i] = choose_position[i - 1] + choose;
                    }
                    
                    if (choose_position[times - 1] + choose <= sub_len + sub_start)
                    {
                        inc_position = 0;
                        break;
                    }
                    
                    if (curr == 0)
                    {
                        break;
                    }
                }
                
                if (choose_position[times - 1] + choose > sub_len + sub_start)
                {
                    do_the_choose = 0;
                }
            }
        }
    }
}

main()
{
   ipe* p = ipe_init(4);
   
   ipe_printf(p);
   
   iter_eps(p, 0, 4);
   
   ipe_free(p);
}
