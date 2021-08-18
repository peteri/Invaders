stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	#include "alienshot.inc"
	#include "timerobject.inc"
	segment 'rom'	
reset_shot_data.w
	ld	a,#4
	ld	(shot_blow_count,x),a
	ld	a,#0
	ld	(shot_flags,x),a
	ld	(shot_step_count,x),a
	ldw	y,(shot_sprite,x)
	ldw	(y),a
	ret
;============================================
;
;	Plunger specific code
;
;============================================
plunger_shot_columns	dc.b $01,$07,$01,$01,$01,$04,$0B,$01,$06,$03,$01,$01,$0B,$09,$02,$08
.alien_shot_plunger_init.w
	mov	{alien_plunger_shot+shot_sprite},#sp_alien_plunger_shot
	mov	{alien_plunger_shot+shot_sprite_exp},#sp_alien_plunger_exp
	mov	{alien_plunger_shot+current_shot_col},#0
	ldw	x,#alien_plunger_shot
	call	reset_shot_data
	ldw	x,#alien_shot_plunger_action
	ldw	{alien_plunger_timer+timer_action_offs},x
	ret
;Reset data
.alien_shot_plunger_reset_data.w
	ldw	x,#alien_plunger_shot
	ld	a,numaliens
	cp	a,#1
	jrne	more_than_one_alien
	bres	{alien_plunger_shot+shot_flags},#plunger_shot_active
more_than_one_alien	
	jp	reset_shot_data
;call action	
.alien_shot_plunger_action.w
	btjf	{alien_plunger_shot+shot_flags},#plunger_shot_active,alien_shot_action_go_home
	ld	a,shot_sync
	cp	a,#1
	jreq	alien_shot_action_go_home
	ldw	x,#alien_plunger_shot
	ldw	y,alien_shot_squigly
	pushw	y
	ldw	y,alien_shot_rolling
	pushw 	y
	ldw	y,#alien_plunger_shot_column
	call	handle_alien_shot
	popw	y
	popw	y
	cp	a,0
	jrne	alien_shot_plunger_reset_data
alien_shot_action_go_home	
	ret
	END
	