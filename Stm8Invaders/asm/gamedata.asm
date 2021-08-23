stm8/
	.tab	0,8,16,60
	#include "variables.inc"
	#include "constants.inc"
	#include "player.inc"
	#include "sprite.inc"
	#include "alienshot.inc"
	#include "timerobject.inc"
	#include "playerbase.inc"
	#include "playershot.inc"
	segment 'rom'
.reset_variables
	ldw	x,current_player
	ld	a,(ref_alien_y_offs,x)
	ld	ref_alien_y,a
	ld	a,(ref_alien_x_offs,x)
	ld	ref_alien_x,a
	ld	a,(ref_alien_delta_x_offs,x)
	ld	ref_alien_delta_x,a
	bres	game_flags_1,#flag1_rack_dir_rtol
	cp	a,#0
	jrpl	rack_left_to_right
	bset	game_flags_1,#flag1_rack_dir_rtol
rack_left_to_right
	ld	a,#0
	ld	saucer_score_index,a
	ld	score_delta,a
	ld	{score_delta+1},a
	ld	alien_explode_timer,a
	ld	shot_count,a

	bset	game_flags_1,#flag1_player_ok
	bres	game_flags_2,#flag2_alien_shot_enable
	bres	game_flags_2,#flag2_adjust_score
	bres	game_flags_2,#flag2_alien_exploding
	bres	game_flags_2,#flag2_invaded
	bres	game_flags_2,#flag2_shot_count
	ldw	y,#$0600
	ldw	time_to_saucer,y
	mov	alien_cur_index,#$ff
	mov	alien_fire_delay,#$30
	mov	alien_shot_delta_y,#$fc
	mov	shot_sync,#1
	call	player_base_init
	btjf	game_flags_2,#flag2_allow_quick_move,slow_move
	mov	{player_base_timer+timer_tick_offs},#0
	mov	{player_base_timer+timer_tick_offs+1},#0
slow_move
	call	sprite_init
	call	init_alien_shots
	call	player_shot_init
	bres	{alien_squigly_shot+shot_flags_offs},#saucer_active
	bres	{alien_squigly_shot+shot_flags_offs},#saucer_hit
	bres	{alien_squigly_shot+shot_flags_offs},#saucer_start
	bset	{alien_plunger_shot+shot_flags_offs},#plunger_shot_active
	ret
	END
	