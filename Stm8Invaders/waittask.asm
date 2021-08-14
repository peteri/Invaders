stm8/
	.tab	0,8,16,60

	#include "mapping.inc"
	#include "variables.inc"
	#include "constants.inc"
	#include "screenhelper.inc"
	segment 'ram0'
wait_state.b	ds.w	1
	segment 'rom'
.reset_wait_state.w
	ret
.run_wait_task.w
	ret
	END
