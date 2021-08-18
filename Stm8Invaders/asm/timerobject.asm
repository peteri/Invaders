stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	#include "timerobject.inc"
	segment 'ram0'
cur_timer_obj	ds.w	1
	segment 'rom'
;==============================================
;
;	Run various objects that live on timers
;
;==============================================
.run_game_objects.w
	ldw	x,#player_base_timer
	btjf	skip_player,#0,no_skip_player
	ldw	x,#player_shot_timer
no_skip_player
	ldw	cur_timer_obj,x
game_object_loop
	ldw	y,cur_timer_obj
	ldw	x,y
	;Timer zero? Run action
	ldw	x,(timer_tick_offs,x)
	jreq	game_object_tick_action
	;Not zero so decrement
	decw	x
	ldw	(timer_tick_offs,y),x
game_object_next_timer
	;Move along to next timer
	addw	y,#timer_size
	ldw	cur_timer_obj,y
	;loop if not off end
	cpw	y,#timer_objects_end
	jrult	game_object_loop
	ret
game_object_tick_action
	;Deal with extra count
	ld	a,(timer_extra_count_offs,y)
	jreq	game_object_call_action
	dec	a
	ld	(timer_extra_count_offs,y),a
	jra	game_object_next_timer
game_object_call_action
	;Time to run the action
	ldw	x,y
	ldw	x,(timer_action_offs,x)
	;Action is null? don't bother
	jreq	game_object_next_timer
	call	(x)
	ldw	y,cur_timer_obj
	jra	game_object_next_timer
	END
	