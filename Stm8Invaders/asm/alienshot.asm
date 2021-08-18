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
plunger_shot_columns.w	dc.b $01,$07,$01,$01,$01,$04,$0B,$01
			dc.b $06,$03,$01,$01,$0B,$09,$02,$08
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
	btjf	{alien_plunger_shot+shot_flags},#plunger_shot_active,alien_shot_plunger_action_go_home
	ld	a,shot_sync
	cp	a,#1
	jreq	alien_shot_plunger_action_go_home
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
alien_shot_plunger_action_go_home
	ret
; compute shot column
; x is saved
; returns column (1 based) in acc
; y is destroyed
alien_plunger_shot_column	
	clrw	y
	ld	a,{alien_plunger_shot+current_shot_col}
	ld	yl,a
	inc	a
	cp	a,#$10
	jrne	alien_plunger_shot_column_exit
	ld	a,#$00
alien_plunger_shot_column_exit
	ld	{alien_plunger_shot+current_shot_col},a
	ld	a,(plunger_shot_columns,y)
	ret
;============================================
;
;	Rolling specific code
;
;============================================
.alien_shot_rolling_init.w
	mov	{alien_rolling_shot+shot_sprite},#sp_alien_rolling_shot
	mov	{alien_rolling_shot+shot_sprite_exp},#sp_alien_rolling_exp
	ldw	x,#alien_rolling_shot
	call	reset_shot_data
	ldw	x,#alien_shot_rolling_action
	ldw	{alien_rolling_timer+timer_action_offs},x
	ret
;Reset data
.alien_shot_rolling_reset_data.w
	ldw	x,#alien_rolling_shot
	mov	{alien_rolling_timer+timer_extra_count_offs},#2
	bres	{alien_rolling_shot+shot_flags},#fire_shot
	jp	reset_shot_data
;call action	
.alien_shot_rolling_action.w
	mov	{alien_rolling_timer+timer_extra_count_offs},#2
	btjf	{alien_rolling_shot+shot_flags},#fire_shot,alien_shot_action_fire
	bset	{alien_rolling_shot+shot_flags},#fire_shot
	jra	alien_shot_rolling_action_go_home
alien_shot_action_fire
	ldw	x,#alien_rolling_shot
	ldw	y,alien_shot_plunger
	pushw	y
	ldw	y,alien_shot_squigly
	pushw 	y
	ldw	y,#alien_rolling_shot_column
	call	handle_alien_shot
	popw	y
	popw	y
	cp	a,0
	jrne	alien_shot_rolling_reset_data
alien_rolling_shot_action_go_home
	ret
; compute shot column
; x is saved
; returns column (1 based) in acc
; y is destroyed
alien_rolling_shot_column
	ld	a,player_base_x
	add	a,#8
	call	find_column
	cp	a,#$ff
	jr
	inc	a
	ret
	END
	