stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	#include "alienshot.inc"
	#include "timerobject.inc"
	#include "player.inc"
	#include "playerbase.inc"
	#include "sprite.inc"
	#include "attractscreen.inc"
	segment 'rom'	
.init_alien_shots.w
	call	alien_shot_plunger_init
	call	alien_shot_rolling_init
	jp	alien_shot_squigly_init
reset_shot_data.w
	ld	a,#4
	ld	(shot_blow_count_offs,x),a
	ld	a,#0
	ld	(shot_flags_offs,x),a
	ld	(shot_step_count_offs,x),a
	ld	a,(shot_sprite_offs,x)
	clrw	y
	ld	yl,a
	ld	a,#0
	ld	(sprite_visible,y),a
	ld	(sprite_image_offs,y),a
	ret
;============================================
;
; On entry
; x = current shot
; sp+3 - othershot1
; sp+5 - othershot2
; sp+7 - address of col shot routine
;
;============================================
handle_alien_shot.w
	ld	a,(shot_flags_offs,x)
	and	a,#{1 shl shot_active}
	jreq	shot_not_active
	jp	move_shot
shot_not_active	
	ldw	y,attract_state
	cpw	y,#ani_coin_pointer
	jreq	activate_shot
	btjt	game_flags_2,#flag2_alien_shot_enable,alien_shots_enabled
	ret
alien_shots_enabled	
	ld	a,#0
	ld	(shot_step_count_offs,x),a
	ldw	y,(3,sp)
	ld	a,(shot_step_count_offs,y)
	jreq	test_other_shot2
	cp	a,alien_shot_reload_rate
	jrugt	test_other_shot2
	ret
test_other_shot2	
	ldw	y,(5,sp)
	ld	a,(shot_step_count_offs,y)
	jreq	get_shot_column
	cp	a,alien_shot_reload_rate
	jrugt	get_shot_column
	ret
get_shot_column
	ldw	y,(7,sp)
	call	(y)	;Get the shot column for this type of shot
	clrw	y
	dec	a
	ld	(5,sp),a	; Save away our result
	sll	a	; Figure out the x coord
	sll	a	; column*$10+7+refAlien_x
	sll	a
	sll	a
	add	a,ref_alien_x
	add	a,#7	
	exg	a,yl	;Stash it
	; Get which sprite we're working with
	ld	a,(shot_sprite_offs,x)
	exg	a,yl	;Put this into the index register
	; now x=shot,y=sprite for shot,a=shot x coord
	ld	(sprite_x_offs,y),a
	ld	a,ref_alien_y
	sub	a,#$0a
	ld	(sprite_y_offs,y),a
	ldw	(3,sp),x	;save x away for later
find_alien_that_fires
	clrw	x
	ld	a,(5,sp)	;Get index back
	addw	x,current_player
	ld	a,(aliens_offs,x)
	jrne	found_alien_that_fires
	ld	a,(sprite_y_offs,y)
	add	a,$10
	ld	(sprite_y_offs,y),a
	ld	a,(5,sp)	;Add 11 to our index
	add	a,#11
	ld	(5,sp),a	;save index back
	cp	a,#55		;Out of aliens?
	jrult	find_alien_that_fires
	ldw	x,(3,sp)	;restore x for exit
	ret
found_alien_that_fires	
	ldw	x,(3,sp)	;restore x 
activate_shot
	ld	a,(shot_flags_offs,x)
	or	a,#{1 shl shot_active}
	ld	(shot_flags_offs,x),a
	inc	(shot_step_count_offs,x)
	ret
;============================================
;
;	Move the shot
;	x=current shot
;
;============================================
move_shot.w
	ld	a,(shot_sprite_offs,x)
	clrw	y
	ld	yl,a
	ld	a,(sprite_x_offs,y)
	add	a,#$20
	and	a,#$80
	cp	a,vblank_status
	jreq	move_shot_blank_ok
	ret
move_shot_blank_ok
	ld	a,(shot_flags_offs,x)
	and	a,#{1 shl shot_blowing_up}
	jrne	animate_shot_explosion
	inc	(shot_step_count_offs,x)
	ld	a,(sprite_image_offs,y)
	inc	a
	cp	a,#4
	jrult	image_no_wrap
	ld	a,#0
image_no_wrap	
	ld	(sprite_image_offs,y),a
	ld	a,#1
	ld	(sprite_visible,y),a
	ld	a,(sprite_y_offs,y)
	add	a,alien_shot_delta_y
	ld	(sprite_y_offs,y),a
	cp	a,#$15
	jrugt	check_collision
	ld	a,(shot_flags_offs,x)
	or	a,#{1 shl shot_blowing_up}
	ld	(shot_flags_offs,x),a
	ldw	x,y
	call	sprite_set_image
	ret
check_collision
	exgw	x,y
	call 	sprite_collided
	exgw	x,y
	cp	a,#0
	jreq	alien_shot_set_image
	ld	a,(shot_flags_offs,x)
	or	a,#{1 shl shot_blowing_up}
	ld	(shot_flags_offs,x),a
	ld	a,(sprite_y_offs,y)
	cp	a,#$1e
	jrult	alien_shot_set_image
	cp	a,#$27
	jrugt	alien_shot_set_image
	mov	player_alive,#player_alive_blowup_one
alien_shot_set_image
	ldw	x,y
	call	sprite_set_image
	ret
	; x= shot y=sprite
animate_shot_explosion
	ld	a,(shot_blow_count_offs,x)
	dec	a
	ld	(shot_blow_count_offs,x),a
	cp	a,#3
	jreq	explode_shot
	cp	a,#0
	jrne	move_shot_exit
	; Get the explosion sprite
	ld	a,(shot_sprite_exp_offs,x)
	clrw	y
	ld	yl,a
	ldw	x,y
	call	sprite_battle_damage
	ld	a,#0
	ld	(sprite_visible,x),a
move_shot_exit
	ret
explode_shot
;	x=current_shot
;	y=current sprite.
	ld	a,#0
	ld	(sprite_visible,y),a
	exgw	x,y
	call	sprite_battle_damage
	pushw	x
	ld	a,(shot_sprite_exp_offs,y)
	clrw	x
	ld	xl,a
	popw	y
	;Now x=shot_sprite_exploded
	;    y=shot_sprite
	ld	a,(sprite_x_offs,y)
	sub	a,#2
	ld	(sprite_x_offs,x),a
	ld	a,(sprite_y_offs,y)
	sub	a,#2
	ld	(sprite_y_offs,x),a
	ld	a,#1
	ld	(sprite_visible,x),a
	call	sprite_set_image
	ret
;============================================
;
;	Plunger specific code
;
;============================================
plunger_shot_columns.w	dc.b $01,$07,$01,$01,$01,$04,$0B,$01
			dc.b $06,$03,$01,$01,$0B,$09,$02,$08
.alien_shot_plunger_init.w
	mov	{alien_plunger_shot+shot_sprite_offs},#sp_alien_plunger_shot
	mov	{alien_plunger_shot+shot_sprite_exp_offs},#sp_alien_plunger_exp
	mov	{alien_plunger_shot+shot_current_shot_col_offs},#0
	ldw	x,#alien_plunger_shot
	call	alien_shot_plunger_reset_data
	ldw	x,#alien_shot_plunger_action
	ldw	{alien_plunger_timer+timer_action_offs},x
	ret
;Reset data
;destroys x register
.alien_shot_plunger_reset_data.w
	ldw	x,#alien_plunger_shot
	call	reset_shot_data
	ld	a,numaliens
	cp	a,#1
	jrne	more_than_one_alien
	bres	{alien_plunger_shot+shot_flags_offs},#plunger_shot_active
more_than_one_alien	
	ret
;call action	
.alien_shot_plunger_action.w
	btjf	{alien_plunger_shot+shot_flags_offs},#plunger_shot_active,alien_shot_plunger_action_exit
	ld	a,shot_sync
	cp	a,#1
	jrne	alien_shot_plunger_action_exit
	ldw	y,#alien_plunger_shot_column
	pushw	y
	ldw	y,#alien_rolling_shot
	pushw 	y
	ldw	y,#alien_squigly_shot
	pushw	y
	ldw	x,#alien_plunger_shot
	call	handle_alien_shot
	popw	y
	popw	y
	popw	y
	ldw	x,#alien_plunger_shot
	ld	a,(shot_blow_count_offs,x)
	jreq	alien_shot_plunger_reset_data
alien_shot_plunger_action_exit
	ret
; compute shot column
; x is saved
; returns column (1 based) in acc
; y is destroyed
alien_plunger_shot_column	
	clrw	y
	ld	a,{alien_plunger_shot+shot_current_shot_col_offs}
	ld	yl,a
	inc	a
	cp	a,#$10
	jrne	alien_plunger_shot_column_exit
	ld	a,#$00
alien_plunger_shot_column_exit
	ld	{alien_plunger_shot+shot_current_shot_col_offs},a
	ld	a,(plunger_shot_columns,y)
	ret
;============================================
;
;	Rolling specific code
;
;============================================
.alien_shot_rolling_init.w
	mov	{alien_rolling_shot+shot_sprite_offs},#sp_alien_rolling_shot
	mov	{alien_rolling_shot+shot_sprite_exp_offs},#sp_alien_rolling_exp
	ldw	x,#alien_rolling_shot
	call	alien_shot_rolling_reset_data
	ldw	x,#alien_shot_rolling_action
	ldw	{alien_rolling_timer+timer_action_offs},x
	ret
;Reset data
.alien_shot_rolling_reset_data.w
	ldw	x,#alien_rolling_shot
	call	reset_shot_data
	mov	{alien_rolling_timer+timer_extra_count_offs},#2
	bres	{alien_rolling_shot+shot_flags_offs},#fire_shot
	ret
;call action	
.alien_shot_rolling_action.w
	mov	{alien_rolling_timer+timer_extra_count_offs},#2
	btjf	{alien_rolling_shot+shot_flags_offs},#fire_shot,alien_shot_action_fire
	bset	{alien_rolling_shot+shot_flags_offs},#fire_shot
	jra	alien_shot_rolling_action_exit
alien_shot_action_fire
	ldw	y,#alien_rolling_shot_column
	pushw 	y
	ldw	y,#alien_squigly_shot
	pushw 	y
	ldw	y,#alien_plunger_shot
	pushw	y
	ldw	x,#alien_rolling_shot
	call	handle_alien_shot
	popw	y
	popw	y
	popw	y
	ldw	x,#alien_rolling_shot
	ld	a,(shot_blow_count_offs,x)
	jreq	alien_shot_rolling_reset_data
alien_shot_rolling_action_exit
	ret
; compute shot column
; x is saved
; returns column (1 based) in acc
; y is destroyed
alien_rolling_shot_column
	ld	a,player_base_x	;Aim at middle of player
	add	a,#8
	pushw	x
	call	find_column	;can we find an invader?
	popw	x
	cp	a,#$ff
	jrne	rolling_found	;Player is to right of stack....
	ld	a,#1
	ret
rolling_found
	cp	a,#$0b		;Player is to left
	jrult	found_column
	ld	a,#$0b
	ret
found_column
	inc	a		;Add one (for reasons)
	ret
;============================================
;
;	squigly specific code
;	TODO Add saucer code.
;
;============================================
squigly_shot_columns.w	dc.b $0B,$01,$06,$03,$01,$01,$0B
			dc.b $09,$02,$08,$02,$0B,$04,$07,$0A 
.alien_shot_squigly_init.w
	mov	{alien_squigly_shot+shot_sprite_offs},#sp_alien_squigly_shot
	mov	{alien_squigly_shot+shot_sprite_exp_offs},#sp_alien_squigly_exp
	mov	{alien_squigly_shot+shot_current_shot_col_offs},#0
	ldw	x,#alien_squigly_shot
	call	alien_shot_squigly_reset_data
	ldw	x,#alien_shot_squigly_action
	ldw	{alien_squigly_timer+timer_action_offs},x
	ret
;Reset data
;destroys x register
.alien_shot_squigly_reset_data.w
	ldw	x,#alien_squigly_shot
	call	reset_shot_data
	ret
;call action	
.alien_shot_squigly_action.w
	ld	a,shot_sync
	cp	a,#2
	jrne	alien_shot_squigly_action_exit
	ldw	y,#alien_squigly_shot_column
	pushw 	y
	ldw	y,#alien_rolling_shot
	pushw 	y
	ldw	y,#alien_plunger_shot
	pushw	y
	ldw	x,#alien_squigly_shot
	call	handle_alien_shot
	popw	y
	popw	y
	popw	y
	ldw	x,#alien_squigly_shot
	ld	a,(shot_blow_count_offs,x)
	jreq	alien_shot_squigly_reset_data
alien_shot_squigly_action_exit
	ret
; compute shot column
; x is saved
; returns column (1 based) in acc
; y is destroyed
alien_squigly_shot_column	
	clrw	y
	ld	a,{alien_squigly_shot+shot_current_shot_col_offs}
	ld	yl,a
	inc	a
	cp	a,#$0f
	jrne	alien_squigly_shot_column_exit
	ld	a,#$00
alien_squigly_shot_column_exit
	ld	{alien_squigly_shot+shot_current_shot_col_offs},a
	ld	a,(squigly_shot_columns,y)
	ret
;==============================================
;
;	Stub routine
;
;==============================================
.inc_saucer_score_shot_count.w
	ret
	END
	