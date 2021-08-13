stm8/
	.tab	0,8,16,60
	#include "player.inc"
	segment 'rom'
.reset.w
.reset_shields.w
.save_shields.w
.restore_shields.w
.draw_sheilds.w
.count_aliens.w
.init_aliens.w
	ret
	END
	