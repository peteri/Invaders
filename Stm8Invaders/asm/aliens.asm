stm8/
	.tab	0,8,16,60
	#include "variables.inc"
	#include "player.inc"
	#include "screenhelper.inc"
	#include "playerbase.inc"
	segment 'ram1'
alien_character_cur_y	ds.b	1
alien_character_cur_x	ds.b	1	
alien_character_cur_offs	ds.b	1	
alien_character_start	ds.b	1	
	segment 'rom'
.cursor_next_alien.w
	btjf	game_flags_1,#flag1_player_ok,cursor_next_alien_exit
	btjt	game_flags_3,#flag3_wait_on_draw,cursor_next_alien_exit
	bres	game_flags_3,#flag3_no_aliens_found
cursor_next_alien_loop	
	inc	alien_cur_index
	ld	a,alien_cur_index
	cp	a,#55
	jrne	check_alien_exists
	call	rack_bump
	mov	alien_cur_index,#0
	ld	a,ref_alien_delta_x
	add	a,ref_alien_x
	ld	ref_alien_x,a
	ld	a,ref_alien_delta_y
	add	a,ref_alien_y
	ld	ref_alien_y,a
	mov	ref_alien_delta_y,#0
	btjt	game_flags_3,#flag3_no_aliens_found,cursor_next_alien_exit
	bset	game_flags_3,#flag3_no_aliens_found
check_alien_exists
	ld	a,alien_cur_index
	clrw	x
	ld	xl,a
	addw	x,current_player
	ld	a,(aliens_offs,x)
	jreq	cursor_next_alien_loop
;	Found an alien to draw	
	call	calculate_alien_position
	ld	a,alien_character_cur_y
	cp	a,#4
	jreq	aliens_invaded
	bset	game_flags_3,#flag3_wait_on_draw
cursor_next_alien_exit
	ret
aliens_invaded	
; Aliens have invaded game over...	
	bset	game_flags_2,#flag2_invaded
	mov	player_alive,#player_alive_blowup_one
	ld	a,#1
	ldw	x,current_player
	ld	(ships_rem_offs,x),a
	call	remove_ship
	call	display_ship_count
	ret
calculate_alien_position
	ret
rack_bump
	ret
.draw_alien.w
	ret
.explode_alien.w
	ret
	END
	