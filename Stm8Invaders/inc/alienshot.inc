shot_flags	EQU	0
shot_blow_count	EQU	{shot_flags+1}
shot_step_count	EQU	{shot_blow_count+1}
shot_sprite	EQU	{shot_step_count+1}
shot_sprite_exp	EQU	{shot_sprite+1}
shot_current_shot_col	EQU {shot_sprite_exp+1}
shot_saucer_timer	EQU {shot_current_shot_col}

shot_active	EQU	0
shot_blowing_up	EQU	1
plunger_shot_active	EQU	2
fire_shot	EQU	4

plunger_shot_size EQU	{shot_current_shot_col+1}
rolling_shot_size EQU	{shot_sprite_exp+1}
squigly_shot_size EQU	{shot_saucer_timer+1}
