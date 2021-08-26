stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	#include "sprite.inc"
	#include "alienshot.inc"
	#include "timerobject.inc"
	segment 'ram1'
explosion_timer	ds.b	1
	segment 'rom'
.player_shot_init.w
	mov	explosion_timer,#$10
	ldw	x,#player_shot_action
	ldw	{player_shot_timer+timer_action_offs},x
	ldw	x,#player_shot_available
	ldw	player_shot_status,x
	ret
player_shot_action
	ld	a,{sp_player_shot+sprite_x_offs}
	and	a,#$80
	cp	a,vblank_status
	jreq	do_player_shot_action
	ret
do_player_shot_action
	ldw	y,player_shot_status
	jp	(y)
.player_shot_available.w
	mov	{sp_player_shot+sprite_visible},#0
	ret
.player_shot_initiated.w
	mov	{sp_player_shot+sprite_visible},#1
	mov	{sp_player_shot+sprite_y_offs},#$28
	ld	a,player_base_x
	add	a,#8
	ld	{sp_player_shot+sprite_x_offs},a
	ldw	x,#player_shot_normal_move
	ldw	player_shot_status,x
	ldw	x,#sp_player_shot
	jp	sprite_set_image
.player_shot_normal_move.w
	ld	a,{sp_player_shot+sprite_y_offs}
	add	a,#4
	ld	{sp_player_shot+sprite_y_offs},a
	ldw	x,#sp_player_shot
	call	sprite_set_image
	ldw	x,#sp_player_shot
	call	sprite_collided
	cp	a,#0
	jreq	shot_normal_move_exit
	bset	game_flags_2,#flag2_alien_exploding
shot_normal_move_exit	
	ret
.player_shot_hit_something.w
	dec	explosion_timer
	jrne	check_first_time
	mov	{sp_player_shot_exp+sprite_visible},#0
	jp	end_blow_up
check_first_time	
	ld	a,explosion_timer
	cp	a,#$0f
	jrne	shot_normal_move_exit
	mov	{sp_player_shot+sprite_visible},#0
	ldw	x,#sp_player_shot
	call	sprite_battle_damage
	; Copy player shot sprite to explosion
	ld	a,{sp_player_shot+sprite_x_offs}
	sub	a,#3
	ld	{sp_player_shot_exp+sprite_x_offs},a
	ld	a,{sp_player_shot+sprite_y_offs}
	sub	a,#2
	ld	{sp_player_shot_exp+sprite_y_offs},a
	mov	{sp_player_shot_exp+sprite_visible},#1
	ldw	x,#sp_player_shot_exp
	jp	sprite_set_image
.player_shot_alien_exploding.w	
	ret
.player_shot_alien_exploded.w
	jp	end_blow_up
	ret
end_blow_up
	ldw	y,player_shot_status
	cpw	y,#player_shot_hit_something
	jrne	no_battle_damage
	ldw	x,#sp_player_shot_exp
	call	sprite_battle_damage
no_battle_damage
	ldw	x,#player_shot_available
	ldw	player_shot_status,x
	mov	{sp_player_shot+sprite_y_offs},#$28
	mov	{sp_player_shot+sprite_x_offs},#$00
	ldw	x,#sp_player_shot
	call	sprite_set_image
	mov	explosion_timer,#$10
	jp	inc_saucer_score_shot_count
	END
	