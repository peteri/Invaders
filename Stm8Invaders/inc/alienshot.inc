shot_flags_offs		EQU	0
shot_blow_count_offs	EQU	{shot_flags_offs+1}
shot_step_count_offs	EQU	{shot_blow_count_offs+1}
shot_sprite_offs	EQU	{shot_step_count_offs+1}
shot_sprite_exp_offs	EQU	{shot_sprite_offs+1}
shot_current_shot_col_offs	EQU {shot_sprite_exp_offs+1}
shot_saucer_timer_offs	EQU {shot_current_shot_col_offs}

shot_active	EQU	0
shot_blowing_up	EQU	1
plunger_shot_active	EQU	2
fire_shot	EQU	3
saucer_active	EQU	4
saucer_hit	EQU	5
saucer_start	EQU	6


plunger_shot_size EQU	{shot_current_shot_col_offs+1}
rolling_shot_size EQU	{shot_sprite_exp_offs+1}
squigly_shot_size EQU	{shot_saucer_timer_offs+1}

	EXTERN	init_alien_shots.w
	