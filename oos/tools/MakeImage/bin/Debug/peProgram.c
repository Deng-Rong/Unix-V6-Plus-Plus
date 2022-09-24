#include "stdio.h"
#include "stdlib.h"
#include "sys.h"

void foo()
{
	return;
}

void Delay()
{
	int i, j, a, b, c;
	for ( i = 0; i < 50; i++ )
		for ( j = 0; j < 10000; j++ )
		{
			c=a+b;
			c++;
		}
}

int main1()
{
	int x = 3;
	x = 5;
	int ans;
	char* argv[2];
	foo();
	printf("this is Test.exe\n");
	int pid = fork();
	printf("Fork() Finished in Test.exe\n");
	
	while(1)
	{
		if( pid == 0)
		{
			printf("Test.exe: CCCCCCCCCCCCCCCCC [2]\n");
			Delay();
			
			if(--x == 0)
			{
				printf("Exit() child\n");
				exit(0x66);
			}
		}
		else
		{
			printf("Test.exe: PPPPPPPPPPPPPPPPPPPP [1]\n");
			Delay();
			printf("Start Wait------------------------------>\n");
			Delay();
			ans = wait(&x);
			printf("End Wait=================[ %d ], [ %d ]\n", x, ans);
			//while(1);
			Delay();Delay();
			if( ans == -1)
				break;
		}
	}
	argv[0] = 0;
	execv("/peProgram.exe", argv);
	//while(1);
	
	return 2;
}
