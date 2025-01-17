nasm -f elf64 source/multiboot.asm -o obj/multiboot.o
llc -filetype=obj -mtriple=x86_64-unknown-elf source/program.ll -o obj/program.o
ld -m elf_x86_64 -T linker.ld -o iso/boot/kernel.bin obj/multiboot.o obj/program.o
grub-mkrescue -o myos_llvm.iso iso/