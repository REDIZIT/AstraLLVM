section .multiboot
align 8

mb2_start:
    dd 0xe85250d6             ; Multiboot2 magic number
    dd 0                      ; Архитектура (0 для i386, 1 для MIPS, 2 для x86_64)
    dd mb2_end - mb2_start    ; Длина заголовка
    dd -(0xe85250d6 + 0 + (mb2_end - mb2_start)) ; Контрольная сумма

    ; Укажите настройки фреймабуфера (опционально)
    align 8
    dw 6                      ; Тип тега (Framebuffer)
    dw 0                      ; Зарезервировано
    dd 8                      ; Размер (только заголовок, без данных)
    
mb2_end:



section .data
    test_str dd "Hello world"
    const2:     dd 2
    const10:    dd 10
    const16:    dd 16
    hex_digits dd '0123456789ABCDEF'

section .bss
    buffer resb 32


section .text

global _start
extern main

section .text
_start:

    call main

    push rax
    push buffer
    call int_to_string
    sub rsp, 8

    push buffer
    push 0
    push 0
    call print_string_stack_coord_locals
    sub rsp, 12

.halt:
    cli
    hlt
    jmp .halt



; =====================================================
;
;                     UTILITIES
;
; =====================================================


; 1: (+12) int ptr_string - pointer to string
; 2: (+8) int x - x coord
; 3: (+4) int y - y coord
; used: rcx, al, rdi, rax
print_string_stack_coord_locals:

    ; int ptr_vga_cell = 0xB8000 + (x + y * width) * 2
    ; =>

    ; temp - rbx
    ; [rsp-4] - ptr_vga_cell

    ; temp = y * width
    mov rcx, [rsp+4]
    mov rdx, 80 ; 80 = width
    imul rcx, rdx
    mov rbx, rcx

    ; temp = x + temp
    mov rcx, [rsp+8]
    mov rdx, rbx
    add rcx, rdx
    mov rbx, rcx

    ; temp = temp * 2
    mov rcx, rbx
    mov rdx, 2
    imul rcx, rdx
    mov rbx, rcx

    ; temp = 0xB8000 + temp
    mov rcx, 0xB8000
    mov rdx, rbx
    add rcx, rdx
    mov rbx, rcx

    ; ptr_vga_cell = temp
    mov [rsp-4], rbx

    mov rdi, [rsp-4]  ; rdi = ptr_vga_cell
    mov rsi, [rsp+12] ; rsi = arg[0] (ptr_string)

.print_loop:
    lodsb
    test al, al
    jz .done
    mov [rdi], al
    mov byte [rdi+1], 0x0f
    add rdi, 2
    loop .print_loop
.done:
    ret





; Input
; (+8) eax - number
; (+4) rcx - ptr to buffer
int_to_string:

    mov edx, 0
    mov rbx, 0

    mov eax, [rsp+8]
    add eax, 1  ; MAGIC NUMBER. Somewhy if you use "mov [rcx+rbx], dl" instead of "mov [buffer+rbx], dl" you will get number exactly on 1 less. This "add" "fix" that.

    mov rcx, [rsp+4]

int_to_string_loop:
    ; Divide EAX by 10
    ; div - Unsigned divide EDX:EAX by r/m32, with result stored in EAX := Quotient, EDX := Remainder

    div dword [const10]

    push rax

    ; Convert int (digit) to ASCII char
    add edx, '0'

    ; Move char to buffer
    mov [rcx+rbx], dl
    add rbx, 1

    pop rax
    mov rdx, 0

    ; if remainder - EDX == 0: quit, else: recursive
    test eax, eax
    je .int_to_string_exit
    ; else
    jmp int_to_string_loop
.int_to_string_exit:
    ; then
    mov byte [rcx+rbx], 0

    push rcx
    call reverse_string
    add rsp, 4

    ret


; input
; (+4) rsi - pointer to source string
reverse_string:
    mov rbx, 0  ; len
    mov rsi, [rsp+4]

.get_len:
    mov rcx, [rsi+rbx]  ; string[rbx]
    test rcx, rcx
    je .l1  ; if char is 0?
    ; else
    add rbx, 1
    jmp .get_len
.l1:
    ; then
    jmp .swap

.swap:
    ; rax - 
    ; rbx - len => j
    ; rcx - i
    ; rdx - temp
    ; rsi, rdi - pointers

    sub rbx, 1  ; j
    mov rcx, 0  ; i

    ; while (i < j)
.while:
    cmp rbx, rcx
    jng .end  ; if (i < j)
    ; body

    ; temp = str[i]
    mov byte al, [rsi+rcx]

    ; str[i] = str[j]
    mov byte dl, [rsi+rbx]
    mov byte [rsi+rcx], dl

    ; str[j] = temp
    mov byte [rsi+rbx], al


    add rcx, 1
    sub rbx, 1

    jmp .while
.end:
    ; break
    ret