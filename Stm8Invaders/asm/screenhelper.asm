stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "characterrom.inc"
	#include "constants.inc"
	segment 'ram0'
screen_pos.b	ds.w	1	
message_ptr.b	ds.w	1
	segment 'ram1'
hex_temp	ds.b	5
	segment	'rom'
screen_head_msg	dc.b	" SCORE<1> HI-SCORE SCORE<2> ",0
credit_msg	dc.b	"CREDIT ",0
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
	ldw	x,#2
clear_play_field_loop
	ld	a,#$23
	ld	(screen,x),a
	incw	x
	ld	a,xl
	and	a,#$1f
	cp	a,#$1c
	jrult	clear_play_field_loop
	addw	x,#6
	cpw	x,#{scr_height mult scr_width}
	jrult	clear_play_field_loop
	ret
.clear_screen.w
	clrw	x
	ld	a,#$23
clear_screen_loop
	ld	(screen,x),a
	incw	x
	cpw	x,#{scr_height mult scr_width}
	jrult	clear_screen_loop
	ret
.draw_screen_head.w
	ldw	x,#$1e00
	ldw	y,#screen_head_msg
	jp	write_text
.draw_credit_label.w
	ldw	x,#$0111
	ldw	y,#credit_msg
	jp	write_text
.draw_player_one_score.w
	ldw	x,#$1c03
	ldw	y,player_one
	jp	write_hex_word
.draw_player_two_score.w
	ldw	x,#$1c15
	ldw	y,player_two
	jp	write_hex_word
.draw_high_score.w
	ldw	x,#$1c0b
	ldw	y,hi_score
	jp	write_hex_word
.draw_num_credits.w
	ldw	x,#$0118
	ld	a,credits
	clrw	y
	ld	yl,a
write_hex_byte
	ld	a,#$00
	ld	{hex_temp+2},a
	ld	a,yl
	and	a,#$0f
	add	a,#$30
	ld	{hex_temp+1},a
	sllw	y
	sllw	y
	sllw	y
	sllw	y
	ld	a,yl
	and	a,#$0f
	add	a,#$30
	ld	{hex_temp+0},a
	ldw	y,#hex_temp
	jp	write_text_unmapped
write_hex_word
	ld	a,#$00
	ld	{hex_temp+4},a
	ld	a,yl
	and	a,#$0f
	add	a,#$30
	ld	{hex_temp+3},a
	sllw	y
	sllw	y
	sllw	y
	sllw	y
	ld	a,yl
	and	a,#$0f
	add	a,#$30
	ld	{hex_temp+2},a
	sllw	y
	sllw	y
	sllw	y
	sllw	y
	ld	a,yl
	and	a,#$0f
	add	a,#$30
	ld	{hex_temp+1},a
	sllw	y
	sllw	y
	sllw	y
	sllw	y
	ld	a,yl
	and	a,#$0f
	add	a,#$30
	ld	{hex_temp+0},a
	ldw	y,#hex_temp
	jp	write_text_unmapped
.display_ship_count.w
.draw_ships.w
.draw_bottom_line.w
	ret
	END
	