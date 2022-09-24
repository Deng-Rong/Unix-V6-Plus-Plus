
gcc -Wall -O0 -g -nostdlib -nostdinc -nostartfiles -fno-builtin -fno-exceptions -I"%oos_path%\src\lib\include" -e _main1 peProgram.c "%oos_path%\src\lib\Lib_V6++.a" -o peProgram.exe
objdump -d peProgram.exe > peProgram.asm