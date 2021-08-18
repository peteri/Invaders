stm8/
	.tab	0,8,16,60

	#include "mapping.inc"
	#include "variables.inc"
	#include "constants.inc"
	#include "screenhelper.inc"
	#include "characterrom.inc"
	#include "sprite.inc"
	#include "alienshot.inc"
	#include "timerobject.inc"
	segment 'ram0'
state.b	ds.w	1
minor_state.b	ds.w	1
delayed_msg.b	ds.w	1
delayed_msg_pos.b	ds.w	1
animate_splash.b	ds.b	1
ani_image	ds.b	1
ani_target_x	ds.b	1
ani_delta_x	ds.b	1
ani_count	ds.b	1
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
	jreq	back_to_minor_idle
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
	clrw	x
	ld	xl,a
	ld	a,(charactermap,x)
	ldw	x,delayed_msg_pos
	ld	(screen,x),a
	addw	x,#scr_width
	ldw	delayed_msg_pos,x
	mov	isr_delay,#5
	ldw	y,#minor_print_msg_delay
	ldw	minor_state,y
	ret
; Delay while printing
minor_print_msg_delay.w
	ld	a,isr_delay
	jrne	print_message_wait_for_delay
	ldw	y,#minor_print_msg_char
	ldw	minor_state,y
print_message_wait_for_delay	
	ret
minor_play_demo_wait_death.w
	jra	back_to_minor_idle
minor_play_demo_wait_end_exp.w
	jra	back_to_minor_idle
minor_animate_splash_alien
	dec	ani_count
	jrne	move_splash_alien
	mov	ani_count,#4
	ld	a,ani_image
	cp	a,{sp_splash_alien+sprite_image_offs}
	jrne	image_different
	inc	a
image_different
	ld	{sp_splash_alien+sprite_image_offs},a
move_splash_alien
	ld	a,{sp_splash_alien+sprite_x_offs}
	add	a,ani_delta_x
	ld	{sp_splash_alien+sprite_x_offs},a
	cp	a,ani_target_x
	jrne	set_alien_image
	ldw	y,#minor_idle
	ldw	minor_state,y
set_alien_image	
	ldw	x,#sp_splash_alien
	call	sprite_set_image
	ret
await_coin_shot_explosion.w
	mov	vblank_status,#$80
	call	[{alien_squigly_timer+timer_action_offs}]
	btjf	{alien_squigly_shot+shot_flags},#shot_active,back_to_minor_idle
	ret
;
;	When ever the minor state is idle
;	the major state is advanced along
;	and one of these labels is called.
;
one_second_delay.w
	mov	alien_shot_reload_rate,#8
	call	clear_play_field
	ld	a,#splash_delay_one_second
	jp	splash_delay
print_play.w
	ldw	y,#play_at
	ld	a,animate_splash
	jrne	play_animate
	ldw	y,#play
play_animate	
	ldw	x,#$170c
	jp	print_delayed_message
print_space_invaders.w
	ldw	y,#space_inv
	ldw	x,#$1407
	jp	print_delayed_message
print_advance_table.w
	ldw	y,#score_advance
	ldw	x,#$1004
	call	write_text
	ldw	y,#saucer
	ldw	x,#$0e07
	call	write_text_unmapped
	ldw	y,#invader_type_c
	ldw	x,#$0c08
	call	write_text_unmapped
	ldw	y,#invader_type_b
	ldw	x,#$0a08
	call	write_text_unmapped
	ldw	y,#invader_type_a
	ldw	x,#$0808
	call	write_text_unmapped
	ld	a,#splash_delay_one_second
	jp	splash_delay
print_mystery.w
	ldw	y,#mystery
	ldw	x,#$0e0a
	jp	print_delayed_message
print_30_points.w
	ldw	y,#thirty_points
	ldw	x,#$0c0a
	jp	print_delayed_message
print_20_points.w
	ldw	y,#twenty_points
	ldw	x,#$0a0a
	jp	print_delayed_message
print_10_points.w
	ldw	y,#ten_points
	ldw	x,#$080a
	jp	print_delayed_message
score_table_two_sec_delay.w
	ld	a,#splash_delay_two_second
	jp	splash_delay
ani_alien_in_to_get_y.w
	ld	a,animate_splash
	jrne	animate_alien_in
	ldw	x,#skip_animate_1
	ldw	state,x
	ret
animate_alien_in
	ldw	x,#{223 mult 256 + 123}
	ldw	y,#{$000+184}
	jp	animate_alien_init
ani_alien_pulling_y_off.w
	ldw	y,#play_space
	ldw	x,#$170c
	call	write_text
	ldw	x,#{120 mult 256 + 221}
	ldw	y,#{$200+184}
	jp	animate_alien_init
ani_alien_off_stage_delay.w
	ld	a,#splash_delay_one_second
	jp	splash_delay
ani_alien_pushing_y_back_on.w
	ldw	x,#{221 mult 256 + 120}
	ldw	y,#{$400+184}
	jp	animate_alien_init
ani_alien_job_done_delay.w
	ld	a,#splash_delay_one_second
	jp	splash_delay
ani_alien_removal.w
	mov	{sp_splash_alien+sprite_visible},#0
	ldw	y,#play
	ldw	x,#$170c
	call	write_text
	ld	a,#splash_delay_one_second
	jp	splash_delay
play_demo.w
	ret
after_play_delay.w
	ld	a,#splash_delay_one_second
	jp	splash_delay
insert_coin.w
	call	clear_play_field
	ldw	y,#insert_coin_msg
	ld	a,animate_splash
	jreq	coin_animate
	ldw	y,#insert_ccoin_msg
coin_animate
	ldw	x,#$1108
	jp	write_text
one_or_two_players.w
	ldw	y,#one_or_two_players_msg
	ldw	x,#$0d06
	jp	print_delayed_message
one_player_one_coin.w
	ldw	y,#one_player_one_coin_msg
	ldw	x,#$0a06
	jp	print_delayed_message
two_players_two_coins.w
	ldw	y,#two_players_two_coins_msg
	ldw	x,#$0706
	jp	print_delayed_message
ani_coin_exp_alien_in_delay.w
	ld	a,animate_splash
	jrne	do_coin_animate
	ldw	x,#skip_animate_2
	ldw	state,x
do_coin_animate	
	ld	a,#splash_delay_two_second
	jp	splash_delay
ani_coin_exp_alien_in.w
	ldw	x,#{0 mult 256 + 115}
	ldw	y,#{$000+208}
	jp	animate_alien_init
ani_coin_exp_fire_bullet.w
	mov	{sp_alien_squigly_shot+sprite_x_offs},#{115+8}
	mov	{sp_alien_squigly_shot+sprite_y_offs},#$c5
	mov	alien_shot_delta_y,#$FF
	mov	shot_sync,#2
	ldw	y,#await_coin_shot_explosion
	ldw	minor_state,y
	ret
ani_coin_exp_remove_extra_c.w 
	ldw	y,#insert_coin_msg
	ldw	x,#$1108
	jp	write_text
after_coin_delay.w
	ld	a,#splash_delay_two_second
	jp	splash_delay
toggle_animate_state.w
	mov	{sp_splash_alien+sprite_visible},#0
	ld	a,animate_splash
	jreq	set_true
	mov	animate_splash,#0
	ret
set_true	
	mov	animate_splash,#1
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
; xh=start alien position
; xl=target alien postion
; yh=image to use
; yl=y position
animate_alien_init.w
	ld	a,xl
	ld	ani_target_x,a
	ld	a,xh
	ld	{sp_splash_alien+sprite_x_offs},a
	mov	ani_delta_x,#$01
	cp	a,ani_target_x
	jrult	moving_left_to_right
	mov	ani_delta_x,#$FF
moving_left_to_right
	ld	a,yl
	ld	{sp_splash_alien+sprite_y_offs},a
	mov	{sp_splash_alien+sprite_visible},#1
	mov	ani_count,#4
	ld	a,yh
	ld	ani_image,a
	ld	{sp_splash_alien+sprite_image_offs},a
	ld	a,xh
	jrne	not_starting_zero
	inc	{sp_splash_alien+sprite_image_offs}
not_starting_zero
	ldw	x,#minor_animate_splash_alien
	ldw	minor_state,x
	ldw	x,#sp_splash_alien
	call	sprite_set_image
	ret
	end
	