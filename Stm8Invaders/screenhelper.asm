stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "characterrom.inc"
	segment 'ram0'
screen_pos.b	ds.w	1	
message_ptr.b	ds.w	1
	segment	'rom'
; xl=horizontal position (0..28)
; xh=vertical position (0..31)
; returns value back in x
; destroys a and y
.convert_x_to_screen_pos.w
	ld	a,xl
	ldw	y,#$20
	mul	y,a
	ldw	screen_pos,y
	ld	a,xh
	or	a,{screen_pos+1}
	ld	{screen_pos+1},a
	ldw	x,screen_pos
	ret
; xl=horizontal position (0..28)
; xh=vertical position (0..31)
; y =pointer to message
.write_text.w
	ldw	message_ptr,y
	call	convert_x_to_screen_pos
write_text_loop
	ldw	y,message_ptr
	ld	a,(y)
	jreq	write_text_loop_exit
	incw	y
	ldw	message_ptr,y
	clrw	y
	ld	yl,a
	ld	a,(charactermap,y)
	ld	(screen,x),a
	addw	x,#$20
	jra	write_text_loop
write_text_loop_exit
	ret
;Writes text raw	
.write_text_unmapped.w
	ldw	message_ptr,y
	call	convert_x_to_screen_pos
	ldw	y,message_ptr
write_text_loop_unmapped
	ld	a,(y)
	jreq	write_text_loop_exit_unmapped
	incw	y
	ld	(screen,x),a
	addw	x,#$20
	jra	write_text_loop_unmapped
write_text_loop_exit_unmapped
	ret
.clear_play_field.w
	END
	