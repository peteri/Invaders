stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	segment 'ram1'
explosion_timer	ds.b	1
	segment 'rom'
.player_shot_init.w
.player_shot_available.w
.player_shot_initiated.w
.player_shot_normal_move.w 
.player_shot_hit_something.w 
.player_shot_alien_exploded.w 
.player_shot_alien_exploding.w	

	ret
	END
	