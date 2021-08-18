stm8/
	.tab	0,8,16,60
	#include "player.inc"
	#include "variables.inc"
	#include "characterrom.inc"
	segment 'ram1'
saved_shields	ds.b	shield_size
	segment 'rom'
.reset.w
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
.update_shields_udg.w
.swap_shields.w
.draw_shields.w
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
.find_column.w
	ret
	END
	