#include <sys.h>
#include <malloc.h>
#include <stdio.h>

#define PAGE_SIZE 12288

char *malloc_begin = NULL;
char *malloc_end = NULL;

typedef struct flist {
   unsigned int size;
   struct flist *nlink;
};

struct flist *malloc_head = NULL;

void* malloc(unsigned int size)
{
    if (malloc_begin == NULL)
    {
        /* code */
        malloc_begin = sbrk(0);
        malloc_end = sbrk(PAGE_SIZE);
        malloc_head = malloc_begin;
        malloc_head->size = sizeof(struct flist);
        
        malloc_head->nlink = NULL;

    }
    if (size == 0)
    {
        return NULL;
    }
    size += sizeof(struct flist);
    size = ((size + 7) >> 3) << 3;
    struct flist* iter = malloc_head;
    // find a place to insert
    while(iter->nlink)
    {
        if ((int)(iter->nlink) - iter->size - (int)iter >= size)
        {
            struct flist *temp = (char *)iter + (iter->size);
            temp->nlink = iter->nlink;
            iter->nlink = temp;
            temp->size = size;
            return (char *)temp + sizeof(struct flist);
        }
        iter = iter->nlink;
    }
    // not found
    int expand = size - (malloc_end - (char *)iter - (iter->size));
    expand = ((expand + PAGE_SIZE - 1) / PAGE_SIZE) * PAGE_SIZE;
    malloc_end = sbrk(expand);
    iter->nlink = (char *)iter + (iter->size);
    iter = iter->nlink;
    iter->size = size;
    iter->nlink = NULL;
    printf("%u\n", iter);
    return (char*)iter + sizeof(struct flist);
}

int free(void* addr)
{
    char * real_addr = addr - 8;
    struct flist* iter = malloc_head;
    struct flist* last = malloc_head;
    if (addr == 0)
    {
        return -1;
    }
    // find a place to insert
    while(iter)
    {
        if (iter == real_addr)
        {
            last->nlink = iter->nlink;
            if (last->nlink == NULL)
            {
                char *pos = (char *)last + last->size;
                if (malloc_end - pos > PAGE_SIZE * 2)
                {
                    malloc_end = sbrk(-((malloc_end - pos) / PAGE_SIZE * PAGE_SIZE));
                }
            }
            return 0;
        }
        last = iter;
        iter = iter->nlink;
    }
    return -1;
}

