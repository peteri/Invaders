stm8/
	.tab	0,8,16,60
	#include "variables.inc"
	#include "player.inc"
	#include "screenhelper.inc"
	#include "playerbase.inc"
	#include "playershot.inc"
	#include "constants.inc"
	#include "linerender.inc"
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
	ld	a,ref_alien_x
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
	jrne	draw_alien_two_3
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
	btjf	game_flags_1,#flag1_rack_dir_rtol,draw_alien_four_ltor
	ld	a,alien_character_start
	add	a,#$0d
	cp	a,({screen+$00},x)
	jreq	draw_alien_four_rtl_0a
	ld	a,alien_character_start
	add	a,#$06
	cp	a,({screen+$00},x)
	jreq	draw_alien_four_rtl_0a
	ld	a,alien_character_start
	add	a,#$04
	ld	({screen+$00},x),a
	jra	draw_alien_four_rtl_middle
draw_alien_four_rtl_0a	
	ld	a,alien_character_start
	add	a,#$0a
	ld	({screen+$00},x),a
draw_alien_four_rtl_middle	
	ld	a,alien_character_start
	add	a,#$05
	ld	({screen+$20},x),a
	
	ld	a,alien_character_start
	add	a,#$0e
	cp	a,({screen+$40},x)
	jreq	draw_alien_four_rtl_right
	ld	a,alien_character_start
	add	a,#$06
	ld	({screen+$40},x),a
	ret
draw_alien_four_rtl_right	
	ld	a,alien_character_start
	add	a,#$0d
	ld	({screen+$40},x),a
	ret
draw_alien_four_ltor
	ld	a,alien_character_start
	add	a,#$05
	cp	a,({screen-$20},x)
	jreq	draw_alien_four_ltr_0a
	ld	a,alien_character_start
	add	a,#$04
	ld	({screen+$00},x),a
	jra	draw_alien_four_ltr_middle
draw_alien_four_ltr_0a	
	ld	a,alien_character_start
	add	a,#$0a
	ld	({screen+$00},x),a
draw_alien_four_ltr_middle	
	ld	a,alien_character_start
	add	a,#$05
	ld	({screen+$20},x),a
	
	ld	a,alien_character_start
	add	a,#$02
	cp	a,({screen+$40},x)
	jreq	draw_alien_four_ltr_right
	ld	a,alien_character_start
	add	a,#$06
	ld	({screen+$40},x),a
	ret
draw_alien_four_ltr_right	
	ld	a,alien_character_start
	add	a,#$0b
	ld	({screen+$40},x),a
	ret
;==============================================
;
; draw the alien with a shifted offset of six
;
;==============================================
draw_alien_six
	btjf	game_flags_1,#flag1_rack_dir_rtol,draw_alien_six_ltor
	ld	a,alien_character_start
	add	a,#$09
	cp	a,({screen+$00},x)
	jreq	draw_alien_six_rtl_0e
	ld	a,alien_character_start
	add	a,#$0e
	cp	a,({screen+$00},x)
	jreq	draw_alien_six_rtl_0e
	ld	a,alien_character_start
	add	a,#$07
	ld	({screen+$00},x),a
	jra	draw_alien_six_rtl_middle
draw_alien_six_rtl_0e	
	ld	a,alien_character_start
	add	a,#$0e
	ld	({screen+$00},x),a
draw_alien_six_rtl_middle
	ld	a,alien_character_start
	add	a,#$08
	ld	({screen+$20},x),a
	ld	a,alien_character_start
	add	a,#$09
	ld	({screen+$40},x),a
	ret
draw_alien_six_ltor	
	ld	a,alien_character_cur_x
	jreq	draw_alien_six_ltr_07
	ld	a,alien_character_start
	add	a,#$08
	cp	a,({screen-$20},x)
	jrne	draw_alien_six_ltr_07
	ld	a,alien_character_start
	add	a,#$0e
	ld	({screen+$00},x),a
	jra	draw_alien_six_ltr_middle
draw_alien_six_ltr_07	
	ld	a,alien_character_start
	add	a,#$07
	ld	({screen+$00},x),a
draw_alien_six_ltr_middle
	ld	a,alien_character_start
	add	a,#$08
	ld	({screen+$20},x),a

	ld	a,alien_character_start
	add	a,#$0a
	cp	a,({screen+$40},x)
	jreq	draw_alien_six_rtl_0c
	ld	a,alien_character_start
	add	a,#$09
	ld	({screen+$40},x),a
	ret
draw_alien_six_rtl_0c	
	ld	a,alien_character_start
	add	a,#$0c
	ld	({screen+$40},x),a
	ret
;==============================================
;
;	Bump the rack
;
;==============================================
rack_bump
	btjf	game_flags_1,#flag1_rack_dir_rtol,bump_ltor
	ld	a,#9
	call	play_field_line_is_blank
	cp	a,#0
	jreq	rack_bump_exit
	mov	ref_alien_delta_x,#$02
	ld	a,numaliens
	cp	a,#1
	jrne	rack_bump_more_than_one_alien
	mov	ref_alien_delta_x,#$03
rack_bump_more_than_one_alien	
	mov	ref_alien_delta_y,#$F8
	bres	game_flags_1,#flag1_rack_dir_rtol
	jra	rack_bump_exit
bump_ltor
	ld	a,#213
	call	play_field_line_is_blank
	cp	a,#0
	jreq	rack_bump_exit
	mov	ref_alien_delta_x,#$FE
	mov	ref_alien_delta_y,#$F8
	bset	game_flags_1,#flag1_rack_dir_rtol
rack_bump_exit
	ret
;===================================================
;
;	Erase alien explosion
;
;===================================================
erase_explosion
	clrw	x
	ld	a,alien_explode_x
	ld	xl,a
	ld	a,#scr_width
	mul	x,a
	ld	a,xl
	add	a,alien_explode_y
	ld	xl,a
	ld	a,(screen,x)
	cp	a,#$20
	jruge	not_udg_explosion
	; Fast alien drawn using udg so fill with spaces.
	ld	a,#$23
	ld	({screen+$00},x),a
	ld	({screen+scr_width},x),a
	ld	({screen+scr_width+scr_width},x),a
	ret
not_udg_explosion	
	; Left hand side
	ld	a,({screen-scr_width},x)
	and	a,#$f0
	ld	alien_character_start,a
	ld	a,(screen,x)
	and	a,#$0f
	cp	a,#$00
	jreq	middle_char
	cp	a,#$0e
	jreq	lhs_xf
	cp	a,#$0f
	jrne	lhs_test_x8
lhs_xf	
	ld	a,({screen-scr_width},x)
	sub	a,#$40
	ld	({screen-scr_width},x),a
	jra	lhs_store_blank
lhs_test_x8	
	cp	a,#$08
	jrne	lhs_test_x5
	ld	a,alien_character_start
	or	a,#$09
	jra	store_lhs
lhs_test_x5	
	cp	a,#$05
	jrne	lhs_store_blank
	ld	a,alien_character_start
	or	a,#$06
	jra	store_lhs
lhs_store_blank
	ld	a,#$23
store_lhs
	ld	(screen,x),a
middle_char	
	; Middle
	addw	x,#scr_width
	ld	a,#$23
	ld	(screen,x),a
	addw	x,#scr_width
	; Right hand side
	ld	a,(screen,x)
	;Type 1 Alien
rhs_test_ca	
	cp	a,#$ca
	jrne	rhs_test_cb
	ld	a,#$84
	jra	store_rhs
rhs_test_cb
	cp	a,#$cb
	jrne	rhs_test_cc
	ld	a,#$82
	jra	store_rhs
rhs_test_cc
	cp	a,#$cc
	jrne	rhs_test_da
	ld	a,#$84
	jra	store_rhs
	;Type 2 Alien
rhs_test_da	
	cp	a,#$da
	jrne	rhs_test_db
	ld	a,#$94
	jra	store_rhs
rhs_test_db
	cp	a,#$db
	jrne	rhs_test_dc
	ld	a,#$92
	jra	store_rhs
rhs_test_dc
	cp	a,#$dc
	jrne	rhs_test_ea
	ld	a,#$94
	jra	store_rhs
	;Type 3 Alien
rhs_test_ea	
	cp	a,#$ea
	jrne	rhs_test_eb
	ld	a,#$a4
	jra	store_rhs
rhs_test_eb
	cp	a,#$eb
	jrne	rhs_test_ec
	ld	a,#$a2
	jra	store_rhs
rhs_test_ec
	cp	a,#$ec
	jrne	rhs_test_b1
	ld	a,#$a4
	jra	store_rhs
rhs_test_b1	
	cp	a,$b1
	jreq	rhs_store_blank
	cp	a,$b6
	jreq	rhs_store_blank
	cp	a,$b9
	jreq	rhs_store_blank
	ret
rhs_store_blank
	ld	a,#$23
store_rhs
	ld	(screen,x),a
	ret
;===================================================
;
;	Explode alien
;
;===================================================	
.explode_alien.w
	clrw	x
	ld	a,alien_explode_x
	ld	xl,a
	ld	a,#scr_width
	mul	x,a
	ld	a,xl
	add	a,alien_explode_y
	ld	xl,a
;	x=currOffset
	ld	a,(screen,x)
	cp	a,#$20
	jruge	explode_non_udg_alien
	jp	explode_udg_alien
explode_non_udg_alien
	cp	a,#$80
	jrult	explode_alien_exit
	cp	a,#$af
	jrugt	explode_alien_exit
	call	explode_lhs
	addw	x,#scr_width
	call	explode_middle
	ld	a,(screen,x)
	and	a,#$0f
	cp	a,#$03
	jreq	explode_alien_exit
	addw	x,#scr_width
	call	explode_rhs
explode_alien_exit	
	ret
;
;	Explode left hand bit of alien
;
explode_lhs
	and	a,#$0f
	cp	a,#1
	jreq	explode_lhs_exit1
	cp	a,#9
	jrne	check_special_case
explode_lhs_exit1	
	ret
check_special_case	
	cp	a,#$0a
	jruge	explode_lhs_special_case
	cp	a,#$08
	jrne	explode_lhs_or_b0
	ld	a,({screen-scr_width},x)
	and	a,#$0f
	cp	a,#$0e
	jreq	explode_lhs_add_left_40
	cp	a,#$0f
	jrne	explode_lhs_or_b0
explode_lhs_add_left_40
	ld	a,({screen-scr_width},x)
	add	a,#$40
	ld	({screen-scr_width},x),a
explode_lhs_or_b0
	ld	a,(screen,x)
	or	a,#$b0
	jra	explode_lhs_store
explode_lhs_special_case
	ld	a,({screen+scr_width},x)
	and	a,#$0f
	cp	a,#$05
	jrne	explode_lhs_a_or_c_next_5
	ld	a,(screen,x)
	cp	a,#$8b
	jrne	exp_lhs_test_9b
	ld	a,#$c9
	jra	explode_lhs_store
exp_lhs_test_9b	
	cp	a,#$9b
	jrne	exp_lhs_test_ab
	ld	a,#$d9
	jra	explode_lhs_store
exp_lhs_test_ab	
	cp	a,#$ab
	jrne	exp_lhs_test_9b
	ld	a,#$e9
	jra	explode_lhs_store
exp_lhs_add_40
	add	a,#$40
	jra	explode_lhs_store
explode_lhs_a_or_c_next_5
	ld	a,(screen,x)
exp_lhs_test_8a
	cp	a,#$8a
	jrne	exp_lhs_test_9a
	ld	a,#$27
	jra	explode_lhs_store
exp_lhs_test_9a
	cp	a,#$8a
	jrne	exp_lhs_test_aa
	ld	a,#$28
	jra	explode_lhs_store
exp_lhs_test_aa
	cp	a,#$8a
	jrne	exp_lhs_test_8c
	ld	a,#$2b
	jra	explode_lhs_store
exp_lhs_test_8c
	cp	a,#$8a
	jrne	exp_lhs_test_9c
	ld	a,#$2c
	jra	explode_lhs_store
exp_lhs_test_9c
	cp	a,#$8a
	jrne	exp_lhs_test_ac
	ld	a,#$54
	jra	explode_lhs_store
exp_lhs_test_ac
	cp	a,#$8a
	jrne	explode_lhs_exit
	ld	a,#$55
	jra	explode_lhs_store
explode_lhs_store
	ld	(screen,x),a
explode_lhs_exit
	ret
;
;	Explode middle bit of alien
;
explode_middle
	ld	a,(screen,x)
	and	a,#$0f
	cp	a,#$0a
	jruge	exp_mid_odd
	ld	a,(screen,x)
	or	a,#$b0
	jra	explode_middle_store
exp_mid_odd
	cp	a,#$0e
	jrne	exp_mid_test_xf
	ld	a,#$b9
	jra	explode_middle_store
exp_mid_test_xf
	cp	a,#$0f
	jrne	exp_mid_test_def
	ld	a,#$b1
	jra	explode_middle_store
exp_mid_test_def
	ld	a,(screen,x)
	add	a,#$40
explode_middle_store
	ld	(screen,x),a
	ret
;
;	Explode right hand side of alien
;
explode_rhs
	ld	a,alien_explode_x_offset
	cp	a,#$03
	jruge	explode_rhs_rest
	ld	a,(screen,x)
	and	a,#$0f
	cp	a,#$0a
	jrne	exp_rhs_test_x6
	ld	a,(screen,x)
	add	a,#$40
	jra	explode_rhs_store
exp_rhs_test_x6	
	cp	a,#$06
	jrne	explode_rhs_exit
	ld	a,#$b6
	jra	explode_rhs_store
explode_rhs_rest
	ld	a,(screen,x)
	and	a,#$0f
	cp	a,#$0a
	jruge	exp_rhs_odd
	ld	a,(screen,x)
	or	a,#$b0
	jra	explode_rhs_store
exp_rhs_odd	
	cp	a,#$0e
	jrne	exp_rhs_test_xd
	ld	a,#$b9
	jra	explode_rhs_store
exp_rhs_test_xd
	cp	a,#$0f
	jrne	exp_rhs_test_def
	ld	a,#$b6
	jra	explode_rhs_store
exp_rhs_test_def
	ld	a,(screen,x)
	add	a,#$40
explode_rhs_store
	ld	(screen,x),a
explode_rhs_exit	
	ret
;================================
; Stub routines start here
;================================
explode_udg_alien
	ret
draw_fast_single_alien	
	ret
	END
	