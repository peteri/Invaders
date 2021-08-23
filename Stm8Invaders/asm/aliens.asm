stm8/
	.tab	0,8,16,60
	#include "variables.inc"
	#include "player.inc"
	#include "screenhelper.inc"
	#include "playerbase.inc"
	#include "playershot.inc"
	#include "constants.inc"
	segment 'ram1'
alien_character_cur_y	ds.b	1
alien_character_cur_x	ds.b	1	
alien_character_cur_x_offs	ds.b	1	
alien_character_start	ds.b	1	
	segment 'rom'
;=========================================
;
;	Move along to the next alien to draw
;
;=========================================
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
	ret
;=========================================
;
;	Calculates where our alien is.
;
;=========================================
calculate_alien_position
	ld	a,ref_alien_y
	srl	a
	srl	a
	srl	a
	ld	alien_character_cur_y,a
	ld	a,alien_cur_index
	clrw	x
	ld	xh,a
;	xh=curAlien
;	xl=alienRow
;	a=curAlien
alien_row_loop
	cp	a,#11
	jrult	found_alien
	sub	a,#11
	ld	xh,a
	ld	a,alien_character_cur_y
	add	a,#2
	ld	alien_character_cur_y,a
	incw	x
	ld	a,xh
	jra	alien_row_loop
found_alien
	ld	a,ref_alien_x
	srl	a
	srl	a
	srl	a
	ld	alien_character_cur_x,a
	ld	a,xh
	sll	a
	add	a,alien_character_cur_x
	ld	alien_character_cur_x,a
	ld	a,alien_character_cur_x
	and	a,#7
	ld	alien_character_cur_x_offs,a
	ld	a,xl
	and	a,#$FE
	sll	a
	sll	a
	sll	a
	or	a,#$80
	ld	alien_character_start,a
	ret
;==========================================
;
;	Time for an exploding alien
;
;==========================================
explode_alien_timer
	dec	alien_explode_timer
	jrne	still_exploding
	call	erase_explosion
	ldw	x,#player_shot_alien_exploded
	ldw	player_shot_status,x
	bres	game_flags_2,#flag2_alien_exploding
still_exploding
	ret
;==========================================
;
;	Draws our alien.
;
;==========================================
draw_alien_exit_early
	bres	game_flags_3,#flag3_wait_on_draw
	ret
.draw_alien.w
	btjt	game_flags_2,#flag2_alien_exploding,explode_alien_timer
	ld	a,alien_cur_index
	cp	a,#$ff
	jreq	draw_alien_exit_early
	; alien still there?
	clrw	x
	ld	xl,a
	addw	x,current_player
	ld	a,(aliens_offs,x)
	jreq	draw_alien_exit_early
;figure out where on screen.	
	clrw	x
	ld	a,alien_character_cur_x
	ld	xl,a
	ld	a,#scr_width
	mul	x,a
	ld	a,xl
	add	a,alien_character_cur_y
	ld	xl,a
;	x=currOffset
; Side effect of the original shift logic is that the row 
; above the current invader is cleared when an alien is drawn.
	ld	a,#$23
	ld	({screen+$01},x),a
	ld	({screen+$21},x),a
; If we're advancing Right to left and we only have type C 
; aliens left we go one more step so we don't wipe out the
; row above us in the NE direction.
	btjf	game_flags_1,#flag1_rack_dir_rtol,going_left_to_right
	ld	a,alien_character_cur_x_offs
	jreq	draw_new_alien
	ld	a,#$23
	ld	({screen+$41},x),a
	jra	draw_new_alien
going_left_to_right	
; If we're advancing left to right and we only have type C 
; aliens left at the edges with a partial row of B aliens
; below i.e. the rack looks like this  
;     45a5a5a5a5a56   
;       45a56 456 
;
; then as the row of type B aliens moves down it looks like 
;     45a5a5a5a5a56   
;         a56 456 
;       78	
	ld	a,alien_character_start
	add	a,#$0a
	cp	a,({screen+$41},x)
	jrne	six_test
	ld	a,alien_character_start
	add	a,#$04
	ld	({screen+$41},x),a
; As the row of type B aliens moves down it looks like this
;     45a5a5a5a5a56      
;           6 456 
;       7878	
six_test
	ld	a,alien_character_start
	add	a,#$06
	cp	a,({screen+$41},x)
	jrne	draw_new_alien
	ld	a,#$23
	ld	({screen+$41},x),a
draw_new_alien
	ld	a,numaliens
	cp	a,#$01
	jrne	regular_alien
	call	draw_fast_single_alien
	jra	draw_alien_exit
regular_alien
	ld	a,alien_character_cur_x_offs
	cp	a,#0
	jrne	alien_test2
	call	draw_alien_zero
	jra	set_single_alien_type	
alien_test2	
	cp	a,#2
	jrne	alien_test4
	call	draw_alien_two
	jra	set_single_alien_type	
alien_test4	
	cp	a,#4
	jrne	alien_test6
	call	draw_alien_four
	jra	set_single_alien_type	
alien_test6	
	cp	a,#6
	jrne	set_single_alien_type
	call	draw_alien_six
set_single_alien_type
	bres	game_flags_3,#flag3_single_alien_is_type1
	btjt	alien_character_cur_x_offs,#2,draw_alien_exit
	bset	game_flags_3,#flag3_single_alien_is_type1
draw_alien_exit
	bres	game_flags_3,#flag3_wait_on_draw
	ret
;==============================================
;
; draw the alien with a shifted offset of zero
;
;==============================================
draw_alien_zero	
	ld	a,alien_character_start
	add	a,#$07
	cp	a,({screen-$20},x)
	jrne	draw_alien_zero_1
	ld	a,#$23
	ld	({screen-$20},x),a
draw_alien_zero_1
	ld	a,alien_character_start
	add	a,#$0f
	cp	a,({screen-$20},x)
	jrne	draw_alien_zero_2
	ld	a,alien_character_start
	add	a,#$01
	ld	({screen-$20},x),a
draw_alien_zero_2	
	ld	a,alien_character_start
	ld	({screen},x),a
	ld	a,alien_character_start
	add	a,#$08
	cp	a,({screen-$40},x)
	jrne	draw_alien_zero_3
	ld	a,alien_character_start
	add	a,#$0f
	ld	({screen+$20},x),a
	ret
draw_alien_zero_3	
	ld	a,alien_character_start
	add	a,#$01
	ld	({screen+$20},x),a
	ret
;==============================================
;
; draw the alien with a shifted offset of two
;
;==============================================
draw_alien_two	
	ld	a,alien_character_start
	add	a,#$06
	cp	a,({screen-$20},x)
	jrne	draw_alien_two_1
	ld	a,alien_character_start
	add	a,#$0b
	ld	({screen+$00},x),a
	jra	draw_alien_two_2
draw_alien_two_1
	ld	a,alien_character_start
	add	a,#$02
	ld	({screen+$00},x),a
draw_alien_two_2
	ld	a,alien_character_start
	add	a,#$03
	ld	({screen+$20},x),a

	btjf	game_flags_1,#flag1_rack_dir_rtol,draw_alien_two_exit
	ld	a,alien_character_start
	add	a,#$0a
	cp	a,({screen-$40},x)
	jrne	draw_alien_two_1
	ld	a,alien_character_start
	add	a,#$04
	ld	({screen+$40},x),a
	jra	draw_alien_two_exit
draw_alien_two_3
	ld	a,#$23
	ld	({screen+$40},x),a
draw_alien_two_exit
	ret
;==============================================
;
; draw the alien with a shifted offset of four
;
;==============================================
draw_alien_four	
;==============================================
;
; draw the alien with a shifted offset of six
;
;==============================================
draw_alien_six
	ret
;================================
; Stub routines start here
;================================
draw_fast_single_alien	
	ret
rack_bump
	ret
erase_explosion
	ret
.explode_alien.w
	ret
	END
	