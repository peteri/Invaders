stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	#include "alienshot.inc"
	#include "timerobject.inc"
	#include "player.inc"
	#include "sprite.inc"
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
	ld	(y),a
	ret
handle_alien_shot.w
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
	ld	a,numaliens
	cp	a,#1
	jrne	more_than_one_alien
	bres	{alien_plunger_shot+shot_flags_offs},#plunger_shot_active
more_than_one_alien	
	jp	reset_shot_data
;call action	
.alien_shot_plunger_action.w
	btjf	{alien_plunger_shot+shot_flags_offs},#plunger_shot_active,alien_shot_plunger_action_exit
	ld	a,shot_sync
	cp	a,#1
	jreq	alien_shot_plunger_action_exit
	ldw	x,#alien_plunger_shot
	ldw	y,alien_squigly_shot
	pushw	y
	ldw	y,alien_rolling_shot
	pushw 	y
	ldw	y,#alien_plunger_shot_column
	call	handle_alien_shot
	popw	y
	popw	y
	ldw	x,#alien_rolling_shot
	cp	a,0
	jrne	alien_shot_plunger_reset_data
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
	mov	{alien_rolling_timer+timer_extra_count_offs},#2
	bres	{alien_rolling_shot+shot_flags_offs},#fire_shot
	jp	reset_shot_data
;call action	
.alien_shot_rolling_action.w
	mov	{alien_rolling_timer+timer_extra_count_offs},#2
	btjf	{alien_rolling_shot+shot_flags_offs},#fire_shot,alien_shot_action_fire
	bset	{alien_rolling_shot+shot_flags_offs},#fire_shot
	jra	alien_shot_rolling_action_exit
alien_shot_action_fire
	ldw	x,#alien_rolling_shot
	ldw	y,alien_plunger_shot
	pushw	y
	ldw	y,alien_squigly_shot
	pushw 	y
	ldw	y,#alien_rolling_shot_column
	call	handle_alien_shot
	popw	y
	popw	y
	ldw	x,#alien_rolling_shot
	cp	a,0
	jrne	alien_shot_rolling_reset_data
alien_shot_rolling_action_exit
	ret
; compute shot column
; x is saved
; returns column (1 based) in acc
; y is destroyed
alien_rolling_shot_column
	ld	a,player_base_x	;Aim at middle of player
	add	a,#8
	call	find_column	;can we find an invader?
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
	ld	a,#$20
	ld	(shot_current_shot_col_offs,x),a
	jp	reset_shot_data
;call action	
.alien_shot_squigly_action.w
	cp	a,#2
	jreq	alien_shot_squigly_action_exit
	ldw	x,#alien_squigly_shot
	ldw	y,alien_plunger_shot
	pushw	y
	ldw	y,alien_rolling_shot
	pushw 	y
	ldw	y,#alien_squigly_shot_column
	call	handle_alien_shot
	popw	y
	popw	y
	ldw	x,#alien_rolling_shot
	cp	a,0
	jrne	alien_shot_squigly_reset_data
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
	END
	