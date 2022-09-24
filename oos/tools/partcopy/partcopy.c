#include <stdio.h>
#include <stdlib.h>

void print_usage()
{
	printf("\"Partial Copy Ex\" version x.0 by Yuanji Wang\n");
	printf("USAGE:\n");
	printf("partcopy source source_offset length destination {dest_offset}\n");
}

#define showmsg_if_error(condition, error_message, error_value) \
	if ( (condition) )\
	{\
		printf((error_message), (error_value));\
		printf("\n"); \
		print_usage();\
		goto RELEASE_RESOURCE; \
	}

int ret;
int main(int argc, char* argv[])
{
	char* source;
	int source_offset;
	int length;
	char* destination;
	int dest_offset;
	
	unsigned char buffer[4096];

	FILE* fd_source = NULL;
	FILE* fd_destination = NULL;

	if ( argc < 5 )
	{
		print_usage();
		return -1;
	}	

	source = argv[1];
	sscanf(argv[2], "%x", &source_offset);
	sscanf(argv[3], "%x", &length);
	destination = argv[4];
	if ( argv > 5 )
	{
		sscanf(argv[5], "%x", &dest_offset);
	}
	else
	{
		dest_offset = 0;
	}

	fd_source = fopen(source, "rb");
	showmsg_if_error( fd_source == NULL, "open %s error!", source);

	fd_destination = fopen(destination, "r+b");
	showmsg_if_error( fd_destination == NULL, "open %s error!", destination);
	
	ret = fseek(fd_source, source_offset, SEEK_CUR);
	showmsg_if_error( ret != 0, "source_offset %x error!", source_offset);

	ret = fseek(fd_destination, dest_offset, SEEK_CUR);
	showmsg_if_error( ret != 0, "dest_offset %x error!", dest_offset);

	showmsg_if_error( length < 0, "length %d error!", length);

	while ( length > 0 )
	{
		int byte_to_read = length > sizeof(buffer) ? sizeof(buffer) : length;
		fread( buffer, sizeof(char), byte_to_read, fd_source);		
		fwrite( buffer, sizeof(char), byte_to_read, fd_destination);
		//printf("source: %x dest: %x\n", ftell(fd_source), ftell(fd_destination));
		length -= byte_to_read;
	}

RELEASE_RESOURCE:
	if ( fd_source != NULL )
	{
		fclose(fd_source);
	}
	if ( fd_destination != NULL )
	{
		fclose(fd_destination);
	}
	return 0;
}