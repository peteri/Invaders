stm8/
	.tab	0,8,16,60

	#include "mapping.inc"
	#include "variables.inc"
	#include "constants.inc"
	#include "screenhelper.inc"
	segment 'ram0'
state.b	ds.w	1
minor_state.b	ds.w	1
delayed_msg.b	ds.w	1
delayed_msg_pos.b	ds.w	1
animate_splash.b	ds.b	1
	segment 'rom'
;jump table for major states	
states.w	
	dc.w	one_second_delay 
	dc.w	print_play 
	dc.w	print_space_invaders 
	dc.w	print_advance_table 
	dc.w	print_mystery 
	dc.w	print_30_points 
	dc.w	print_20_points 
	dc.w	print_10_points 
	dc.w	score_table_two_sec_delay 
	dc.w	ani_alien_in_to_get_y 
	dc.w	ani_alien_pulling_y_off 
	dc.w	ani_alien_off_stage_delay 
	dc.w	ani_alien_pushing_y_back_on 
	dc.w	ani_alien_job_done_delay 
skip_animate_1.w	
	dc.w	ani_alien_removal 
	dc.w	play_demo 
	dc.w	after_play_delay 
	dc.w	insert_coin 
	dc.w	one_or_two_players 
	dc.w	one_player_one_coin 
	dc.w	two_players_two_coins 
	dc.w	ani_coin_exp_alien_in_delay 
	dc.w	ani_coin_exp_alien_in 
	dc.w	ani_coin_exp_fire_bullet 
	dc.w	ani_coin_exp_remove_extra_c 
skip_animate_2.w
	dc.w	after_coin_delay
	dc.w	toggle_animate_state
last_state.w
;
; text messages
;
play_at.w		dc.b	"PLA@",0
play.w			dc.b	"PLAY",0
play_space.w		dc.b	"PLA ",0
space_inv.w 		dc.b	"SPACE  INVADERS",0
score_advance.w		dc.b	"*SCORE ADVANCE TABLE*",0
saucer.w		dc.b	$24,$25,$26,0
invader_type_c.w	dc.b	$A0,$A1,0
invader_type_b.w	dc.b	$BC,$BD,0
invader_type_a.w	dc.b	$80,$81,0
mystery.w		dc.b	"=? MYSTERY",0
thirty_points.w		dc.b	"=30 POINTS",0
twenty_points.w		dc.b	"=20 POINTS",0
ten_points.w		dc.b	"=10 POINTS",0
insert_ccoin_msg.w		dc.b	"INSERT CCOIN",0
insert_coin_msg.w		dc.b	"INSERT  COIN",0
one_or_two_players_msg.w	dc.b	"<1 OR 2 PLAYERS>",0
one_player_one_coin_msg.w	dc.b	"1 PLAYER  1 COIN",0
two_players_two_coins_msg.w dc.b	"2 PLAYERS 2 COINS",0
;
;	reset the attract state.
;
.reset_attract_state.w
	ldw	y,#{states-2}
	ldw	state,y
	ldw	y,#minor_idle
	ldw	minor_state,y
	ret
;
; Attract task called once per frame.
;
.attract_task.w
	jp	[minor_state]
;Idle advance to next major state	
minor_idle.w
	ldw	y,state
	incw	y
	incw	y
	cpw	y,#last_state
	jrne	still_in_table
	ldw	y,#states
still_in_table
	ldw	state,y
	ldw	y,(y)
	jp	(y)
; Wait for the delay...	
minor_wait.w
	ld	a,isr_delay
	jreq	minor_idle
	ret
; print single character then either delay or idle	
minor_print_msg_char
	ldw	y,delayed_msg
	ld	a,(y)
	jrne	more_message
; no more message back to idle	
back_to_minor_idle	
	ldw	y,#minor_idle
	ldw	minor_state,y
	ret
more_message	
	incw	y
	ldw	delayed_msg,y
	ldw	x,delayed_msg_pos
	ld	(x),a
	addw	x,#scr_width
	ldw	delayed_msg_pos,x
	mov	isr_delay,#5
	ldw	y,#minor_print_msg_delay
	ldw	minor_state,y
	ret
; Delay while printing
minor_print_msg_delay
	ld	a,isr_delay
	jreq	print_message_wait_for_delay
	ldw	y,#minor_print_msg_char
	ldw	minor_state,y
print_message_wait_for_delay	
	ret
minor_play_demo_wait_death
	jra	back_to_minor_idle
minor_play_demo_wait_end_exp
	jra	back_to_minor_idle
minor_animate_splash_alien
	ret
;
;	When ever the minor state is idle
;	the major state is advanced along
;	and one of these labels is called.
;
one_second_delay.w
	mov	alien_shot_reload_rate,#8
	call	clear_play_field
	ld	a,splash_delay_one_second
	jp	splash_delay
print_play.w
	ldw	y,#play_at
	ld	a,animate_splash
	jrne	play_animate
	ldw	y,#play
play_animate	
	ldw	x,#$0c17
	jp	print_delayed_message
print_space_invaders.w
	ldw	y,#space_inv
	ldw	x,#$0714
	jp	print_delayed_message
print_advance_table.w
	ldw	y,#score_advance
	ldw	x,#$0410
	call	write_text
	ldw	y,#saucer
	ldw	x,#$070e
	call	write_text_unmapped
	ldw	y,#invader_type_c
	ldw	x,#$080c
	call	write_text_unmapped
	ldw	y,#invader_type_b
	ldw	x,#$080a
	call	write_text_unmapped
	ldw	y,#invader_type_a
	ldw	x,#$0808
	call	write_text_unmapped
	ld	a,splash_delay_one_second
	jp	splash_delay
print_mystery.w
	ldw	y,#mystery
	ldw	x,#$0a0e
	jp	print_delayed_message
print_30_points.w
	ldw	y,#thirty_points
	ldw	x,#$0a0c
	jp	print_delayed_message
print_20_points.w
	ldw	y,#twenty_points
	ldw	x,#$0a0a
	jp	print_delayed_message
print_10_points.w
	ldw	y,#ten_points
	ldw	x,#$0a08
	jp	print_delayed_message
score_table_two_sec_delay.w
	ld	a,splash_delay_two_second
	jp	splash_delay
ani_alien_in_to_get_y.w
ani_alien_pulling_y_off.w
ani_alien_off_stage_delay.w
ani_alien_pushing_y_back_on.w
ani_alien_job_done_delay.w
ani_alien_removal.w
play_demo.w
	ret
after_play_delay.w
	ld	a,splash_delay_one_second
	jp	splash_delay
insert_coin.w
	call	clear_play_field
	ldw	y,#insert_ccoin_msg
	ld	a,animate_splash
	jrne	coin_animate
	ldw	y,#insert_coin_msg
coin_animate
	ldw	x,#$0811
	call	write_text
	ret
one_or_two_players.w
	ldw	y,#one_or_two_players_msg
	ldw	x,#$060d
	jp	print_delayed_message
one_player_one_coin.w
	ldw	y,#one_player_one_coin_msg
	ldw	x,#$060a
	jp	print_delayed_message
two_players_two_coins.w
	ldw	y,#two_players_two_coins_msg
	ldw	x,#$0607
	jp	print_delayed_message
ani_coin_exp_alien_in_delay.w
ani_coin_exp_alien_in.w
ani_coin_exp_fire_bullet.w
ani_coin_exp_remove_extra_c.w 
	ldw	y,#insert_coin
	ldw	x,#$0811
	call	write_text
	ret
after_coin_delay.w
	ld	a,splash_delay_two_second
	jp	splash_delay
toggle_animate_state.w
	ret
; Setup for a delay and change minor state to wait
; a = number of ticks to delay
splash_delay.w
	ld	isr_delay,a
	ldw	y,#minor_wait
	ldw	minor_state,y
	ret
; Print a message with inter character delays
; y=address of message, nul terminated
; xl=horizontal position (0..28)
; xh=vertical position (0..31)
print_delayed_message.w
	ldw	delayed_msg,y
	call	convert_x_to_screen_pos
	ldw	delayed_msg_pos,x
	ldw	y,#minor_print_msg_char
	ldw	minor_state,y
	ret
	end
	