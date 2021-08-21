stm8/
	.tab	0,8,16,60
	#include "player.inc"
	#include "variables.inc"
	#include "characterrom.inc"
	#include "screenhelper.inc"
	#include "constants.inc"
	segment 'ram1'
saved_shields	ds.b	shield_size
col_x	ds.b	1
	segment 'rom'
shields_line_length	EQU 20
shields_line_one 
	dc.b	$00,$01,$02,$23,$23,$06,$07,$08,$09,$23
	dc.b	$23,$0e,$0f,$10,$23,$23,$14,$15,$16,$17
shields_line_two 
	dc.b	$03,$04,$05,$23,$23,$0a,$0b,$0c,$0d,$23
	dc.b	$23,$11,$12,$13,$23,$23,$18,$19,$1a,$1b	
;=============================================
;
;	Reset the player
;
;=============================================
.reset_player.w
	ldw	x,current_player
	ld	a,#0
	ld	(score_offs,x),a
	ld	({score_offs+1},x),a
	ld	(rack_count_offs,x),a
	ld	a,#1
	ld	(extra_ship_available_offs,x),a
	ld	a,#2
	ld	(ref_alien_delta_x_offs,x),a
	ld	a,#3
	ld	(ships_rem_offs,x),a
	ld	a,#$18
	ld	(ref_alien_x_offs,x),a
	ld	a,#$78
	ld	(ref_alien_x_offs,x),a
	call	init_aliens
; fall through into reset shields
;=============================================
;
;	Reset the shields udg
;
;=============================================
.reset_shields.w
	ldw	x,#$1b
reset_shields_loop
	ldw	y,#8
	ld	a,xl
	mul	y,a
	ld	a,({characterrom+$000},x)
	ld	({udg+0},y),a
	ld	a,({characterrom+$100},x)
	ld	({udg+1},y),a
	ld	a,({characterrom+$200},x)
	ld	({udg+2},y),a
	ld	a,({characterrom+$300},x)
	ld	({udg+3},y),a
	ld	a,({characterrom+$400},x)
	ld	({udg+4},y),a
	ld	a,({characterrom+$500},x)
	ld	({udg+5},y),a
	ld	a,({characterrom+$600},x)
	ld	({udg+6},y),a
	ld	a,({characterrom+$700},x)
	ld	({udg+7},y),a
	decw	x
	jrpl	reset_shields_loop
	ret
;=============================================
;
;	Swap the shield udg with the saved 
;	shields also blanks udg characters
;	that have been wiped out by the invaders
;
;=============================================
.swap_shields.w
	ldw	x,#{4 mult scr_width+$07}
	ldw	y,$0
alien_wipe_loop
	ld	a,(screen,x)
	cp	a,#$1c
	jruge	alien_wipe_check_line_two
	ld	a,(shields_line_one,y)
	cp	a,#$20
	jrult	alien_wipe_check_line_two
	call	clear_udg
alien_wipe_check_line_two	
	ld	a,({screen-1},x)
	cp	a,#$1c
	jruge	alien_wipe_check_loop_done
	cp	a,#$20
	jrult	alien_wipe_check_loop_done
	call	clear_udg
alien_wipe_check_loop_done
	addw	x,#scr_width
	incw	y
	cpw	y,#shields_line_length
	jrule	alien_wipe_loop
; Swap the udg with saved_shields
	ldw	x,#0
swap_udg_sheild_loop
	ld	a,(udg,x)
	ld	yl,a
	ld	a,(saved_shields,x)
	ld	yh,a
	ld	a,yl
	ld	(udg,x),a
	ld	a,yh
	ld	(saved_shields,x),a
	incw	x
	cpw	x,#shield_size
	ret
clear_udg	
	pushw	y
	sll	a
	sll	a
	sll	a
	clrw	y
	ld	yl,a
	ld	a,#0
	ld	({udg+0},y),a
	ld	({udg+1},y),a
	ld	({udg+2},y),a
	ld	({udg+3},y),a
	ld	({udg+4},y),a
	ld	({udg+5},y),a
	ld	({udg+6},y),a
	ld	({udg+7},y),a
	popw	y
	ret
;=============================================
;
;	Draw the shields
;
;=============================================
.draw_shields.w
	ldw	y,#shields_line_one
	ldw	x,#$0704
	call	write_text_unmapped
	ldw	y,#shields_line_two
	ldw	x,#$0604
	jp	write_text_unmapped
;=============================================
;
;	Remove a ship and redraw the bottom
;	line
;
;=============================================
.remove_ship.w
	ldw	x,current_player
	ld	a,(ships_rem_offs,x)
	jreq	remove_ship_exit
	and	a,#$0f	;Display ship count
	add	a,#$20
	ldw	y,#$0021
	ld	(screen,y),a
; draw players ships	
	ldw	x,current_player
	dec	(ships_rem_offs,x)
	ldw	y,#$0023
	ld	a,(ships_rem_offs,x)
	ld	xl,a
draw_ship_loop	
	ld	a,#$56
	ld	(screen,y),a
	addw	y,#scr_width
	ld	a,#$57
	ld	(screen,y),a
	addw	y,#scr_width
	ld	a,xl
	dec	a
	ld	xl,a
	jrne	draw_ship_loop
	ld	a,#$23
blank_ship_loop
	ld	(screen,y),a
	addw	y,#scr_width
	cpw	y,#{$11 mult scr_width}	
	jrult	blank_ship_loop
remove_ship_exit	
	ret
;=============================================
;
;	initialise the aliens.
;
;=============================================
.init_aliens.w
	ldw	y,#{number_of_aliens-1}
	ldw	x,current_player
	ld	a,#1
init_aliens_loop
	ld	(aliens_offs,x),a
	incw	x
	decw	y
	jrpl	init_aliens_loop
	;fall through and count aliens
;=============================================
;
;	Count number of aliens
;
;=============================================
.count_aliens.w
	clr	numaliens
	ldw	y,#{number_of_aliens-1}
	ldw	x,current_player
count_aliens_loop	
	ld	a,(aliens_offs,x)
	jreq	no_alien
	inc	numaliens
no_alien
	incw	x
	decw	y
	jrpl	count_aliens_loop
	ret
;=============================================
;	accumulator is x position
;	returns index of column (zero based)
;	in the accumulator
;	destroys x and y 
;	needs optimisation
;=============================================
.find_column.w
	push	a
	ldw	x,#$ffff
	cp	a,ref_alien_x
	jrult	find_column_ret
	incw	x
	ld	a,ref_alien_x
	add	a,#$10
	ld	col_x,a
	pop	a
	ld	yl,a
find_column_loop
	cp	a,col_x
	jrugt	find_column_ret
	ld	a,col_x
	add	a,#$10
	ld	col_x,a
	ld	a,yl
	incw	x
	jra	find_column_loop
find_column_ret
	ld	a,xl
	ret
	END
	