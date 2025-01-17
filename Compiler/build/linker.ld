ENTRY(_start)

SECTIONS
{
	. = 1M; /* Начало памяти для ядра */
	.multiboot : { *(.multiboot) }
	.text : { *(.text) }
	.data : { *(.data) }
	.bss : { *(.bss COMMON) }
}