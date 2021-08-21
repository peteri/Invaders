stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	segment 'ram1'
demo_command ds.b 1
blow_up_counter ds.b 1
blow_up_Changes ds.b 1
	segment 'rom'
.player_base_init.w
	ret
.increment_demo_command.w
	ret
	END
	