; ram segment 0
	EXTERN	syncdma.b
	EXTERN	linenumber.b
	EXTERN	renderbuff1.b
	EXTERN	renderbuff2.b
	EXTERN	current_player.b
	EXTERN	numaliens.b
	EXTERN	hi_score.b
	EXTERN	credits.b
	EXTERN	isr_delay.b
	EXTERN	game_flags_1.b
	EXTERN	game_flags_2.b
	EXTERN	alien_shot_reload_rate.b
	EXTERN	vblank_status.b
	EXTERN	shot_sync.b
	EXTERN	alien_shot_delta_y.b
	EXTERN	ref_alien_x.b
	EXTERN	ref_alien_y.b
	EXTERN	ref_alien_delta_x.b
	EXTERN	ref_alien_delta_y.b
	EXTERN	saucer_score_index.b
	EXTERN	score_delta.b
	EXTERN	alien_explode_timer.b
	EXTERN	time_to_saucer.b
	EXTERN	alien_cur_index.b
	EXTERN	alien_fire_delay.b	
	EXTERN	player_base_x.b
	EXTERN	player_alive.b
	
	EXTERN	sprites_start.b
	EXTERN	sp_alien_plunger_shot.b
	EXTERN	sp_alien_plunger_exp.b
	EXTERN	sp_alien_rolling_shot.b
	EXTERN	sp_alien_rolling_exp.b
	EXTERN	sp_alien_squigly_shot.b
	EXTERN	sp_alien_squigly_exp.b
	EXTERN	sp_player_shot.b
	EXTERN	sp_player_shot_exp.b
	EXTERN	sp_splash_alien.b
	EXTERN	sprites_end.b
	EXTERN	alien_plunger_timer.b
	EXTERN	alien_rolling_timer.b
	EXTERN	alien_squigly_timer.b
	EXTERN	player_shot_timer.b
	EXTERN	player_base_timer.b
	EXTERN	timer_objects_end.b
	EXTERN	alien_plunger_shot.b
	EXTERN	alien_rolling_shot.b
	EXTERN	alien_squigly_shot.b	
; ram segment 1
	EXTERN	game_flags_3.w
	EXTERN	player_one.w
	EXTERN	player_two.w
	EXTERN	screen.w
	EXTERN	udg.w
	EXTERN	player_shot_status.w
	EXTERN	alien_explode_x.w
	EXTERN	alien_explode_x_offset.w
	EXTERN	alien_explode_y.w
; Subroutines	
	EXTERN	clear_memory.w
;
;	Game flags 1 bit positions
;
flag1_game_mode		EQU	0
flag1_demo_mode		EQU	1
flag1_suspend_play	EQU	2
flag1_tweak		EQU	3
flag1_skip_player	EQU	4
flag1_animate_splash	EQU	5
flag1_rack_dir_rtol	EQU	6
flag1_player_ok		EQU	7
;
;	Game flags 2 bit positions
;
flag2_alien_shot_enable	EQU	0
flag2_adjust_score	EQU	1
flag2_alien_exploding	EQU	2
flag2_invaded		EQU	3
flag2_shot_count	EQU	4
flag2_allow_quick_move	EQU	5
flag2_player_hit_alien	EQU	6
flag2_pause_game	EQU	7
;
;	Game flags 3 bit positions
;
flag3_wait_on_draw	EQU	0
flag3_no_aliens_found	EQU	1
flag3_single_alien_is_type1	EQU	2
