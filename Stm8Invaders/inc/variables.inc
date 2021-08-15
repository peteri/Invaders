; ram segment 0
	EXTERN	led_state.b
	EXTERN	syncdma.b
	EXTERN	linenumber.b
	EXTERN	renderbuff1.b
	EXTERN	renderbuff2.b
	EXTERN	current_player.b
	EXTERN	numaliens.b
	EXTERN	hi_score.b
	EXTERN	credits.b
	EXTERN	isr_delay.b
	EXTERN	game_mode.b
	EXTERN	demo_mode.b
	EXTERN	alien_shot_reload_rate.b
	EXTERN	suspend_play.b
	EXTERN	vblank_status.b
	EXTERN	tweak_flag.b
	EXTERN	skip_player.b
	EXTERN	shot_sync.b
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
	
; ram segment 1
	EXTERN	player_one.w
	EXTERN	player_two.w
	EXTERN	screen.w
	EXTERN	udg.w
; Subroutines	
	EXTERN	clear_memory.w
	