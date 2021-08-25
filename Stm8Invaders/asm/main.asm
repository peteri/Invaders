stm8/
	.tab	0,8,16,60
	#include "mapping.inc"
	#include "stm8l152c6.inc"
	#include "boardsetup.inc"
	#include "variables.inc"
	#include "videosync.inc"
	#include "linerender.inc"
	#include "constants.inc"
	#include "attractscreen.inc"
	#include "waittask.inc"
	#include "screenhelper.inc"
	#include "timerobject.inc"
	#include "sprite.inc"
	#include "aliens.inc"
	#include "player.inc"
	#include "alienshot.inc"
	#include "playerbase.inc"
	#include "playershot.inc"
stack_start.w	EQU $stack_segment_start
stack_end.w	EQU $stack_segment_end
	segment 'ram0'
tim3cntr.w ds.w 1
	segment 'ram1'
shothit_alien_index	ds.b	1
shothit_col_x	ds.b	1
frame_counter	ds.w	1
pause_button_timer	ds.b	1
	segment 'rom'
main.l
	; initialize SP
	ldw X,#stack_end
	ldw SP,X
	; clear stack
	ldw X,#stack_start
clear_stack.l
	clr (X)
	incw X
	cpw X,#stack_end	
	jrule clear_stack
	; we have clear stack
	; time for more setup
	call init_cpu	  ; speed up the cpu and turn on stuff
	call clear_memory ; Clear rest of ram
	call power_on_reset ; setup the game
	call init_gpio	  ; setup the gpio pins
	call init_dma	  ; setup dma channels
	call init_timers  ; setup the timers.
	call init_spi1	  ; setup SPI1 for video out
	rim		  ; interrupts on
infinite_loop.l
	jra infinite_loop
;==============================================
;
;	PowerOnReset routine
;
;==============================================
power_on_reset
	call	draw_status
	call	reset_attract_state
	call	reset_wait_state
	call	sprite_init
	bres	game_flags_1,#flag1_game_mode
	bres	game_flags_1,#flag1_demo_mode
	ret
draw_status
	call	clear_screen
	call	draw_screen_head
	call	draw_player_one_score
	call	draw_player_two_score
	call	draw_high_score
	call	draw_credit_label
	call	draw_num_credits
	ret
;==============================================
;	Interrupt handler for DMA channel
;	transaction complete.
;==============================================
	interrupt DMAChannel23Int
DMAChannel23Int.l
	btjt DMA1_GCSR,#2,dmachan2 ;Channel2?
; Channel 3 DMA	
	bres DMA1_C3CR,#0	;turn off channel
 	bres DMA1_C3SPR,#1	;clear transcation completed
	ldw x,syncdma		;Current value to DMA src
	ldw DMA1_C3M0ARH,x
	ld a,#$80		;How many bytes to transfer
	addw x,#$0100		;Next buffer
	cpw x,#synccompend	;gone off end?
	jrule syncnowrap
	ld a,#$62		;Only do $62 bytes
	ldw x,#synccomp		;Set us up wrapped for next 
syncnowrap	
	ldw syncdma,x
	ld DMA1_C3NDTR,a	;128 or (625*2) mod 128
	bset DMA1_C3CR,#0	;turn back on channel
	iret
; Channel 2 DMA	
dmachan2
 	bres DMA1_C2SPR,#1	;clear transaction completed
	iret
;==============================================
;	Interrupt handler for timer 3 comparator
;	Kicks off rendering the frame and outputting
;	data for screen via SPI.
;==============================================
	interrupt Timer3CompareInt
Timer3CompareInt.l
	bset TIM1_DER,#3	; Turn on CC3 DMA
	bres TIM3_SR1,#1
	bres TIM3_SR1,#2
	ldw y,#$0
	ldw linenumber,y
	mov SPI1_CR2,#%00000010
	mov SPI1_ICR,#%00000010	
	mov SPI1_CR1,#%01000000
renderloop
	ld a,TIM3_CNTRH	;Save current line counter
	ld xh,a
	ld a,TIM3_CNTRL
	ld xl,a
	ldw tim3cntr,x
	; Even line? Render in odd line buffer
	ldw x,#{renderbuff2+1}
	btjf {linenumber+1},#0,dorenderline
	; odd line so render into even buffers
	ldw x,#{renderbuff1+1}
dorenderline	
	call renderline
waitforcounterchange
	ld a,TIM3_CNTRH	;Read current line counter
	ld xh,a
	ld a,TIM3_CNTRL
	ld xl,a
	cpw x,tim3cntr ; Wait for line counter to change
	jreq waitforcounterchange
newline	
	inc	{linenumber+1}
	ldw	y,linenumber
	cpw	y,#{scr_height mult 8 +1}	;28*8+2 lines
	jrule	renderloop	;Not done yet
	bres	TIM1_DER,#3	; Turn off CC3 DMA
	; check for pause
	call	handle_pause
	btjt	game_flags_2,#flag2_pause_game,game_paused
	call	game_tick
	; Do the in game frame tick
	ldw	y,frame_counter
	incw	y
	ldw	frame_counter,y
	ldw	x,#$1c10
	call	write_hex_word	
game_paused
	; Did the compare registers fire?
	ld	a,TIM3_SR1
	and	a,#%00000110
	; Took too long in the game tick
	; time to light up the error and die
	jrne	took_too_long
	bcpl PC_ODR,#7	;toggle led
	iret
took_too_long
	; turn on the green led die and loop
	bset PE_ODR,#7	
	sim
	jra	took_too_long
	interrupt NonHandledInterrupt
NonHandledInterrupt.l
	iret
; Handles pausing the game
; Can either trigger off a frame counter.
; Can be single stepped by tapping button for less than 200ms
; long press removes pause entirely
handle_pause
	btjt	game_flags_2,#flag2_pause_game,already_paused
	ldw	y,frame_counter
;	cpw	y,#$462		; First rack bump
;	jreq	set_pause_flag
	cpw	y,#$4d0		; Bug
	jreq	set_pause_flag
already_paused
	; button up?
	btjf	PC_IDR,#1,button_down
	ld	a,pause_button_timer
	jreq	handle_pause_exit
	ld	a,pause_button_timer
	cp	a,#$ff	;single step?
	jrne	check_pause_length
	mov	pause_button_timer,#0
	jra	set_pause_flag
;Ok button has gone up... If the timer is in the first
;200ms (10 frames) then assume we want to single step
;If it's longer the assume we want to stop pausing
check_pause_length
	bres	game_flags_2,#flag2_pause_game
	cp	a,#90
	jrugt	single_shot
	mov	pause_button_timer,#0
	ret
single_shot	
	mov	pause_button_timer,#$ff
handle_pause_exit
	ret
button_down
	ld	a,pause_button_timer
	jrne	dec_pause_timer
	mov	pause_button_timer,#100
dec_pause_timer	
	dec	pause_button_timer
set_pause_flag	
	bset	game_flags_2,#flag2_pause_game
	ret
;=============================================
;
; 	Main tick routine
;	Called from frame interrupt.
;
;=============================================
game_tick
	dec	isr_delay
	call	handle_coin_switch
	btjf	game_flags_1,#flag1_suspend_play,not_wait_task
	jp	run_wait_task
not_wait_task	
	btjt	game_flags_1,#flag1_game_mode,run_game
	btjt	game_flags_1,#flag1_demo_mode,run_game
	ld	a,credits
	jreq	do_attract_screen
	jp	enter_wait_start_loop
do_attract_screen
	jp	attract_task
run_game
	call	game_loop_step
	mov	vblank_status,#0
	btjf	game_flags_1,#flag1_tweak,skip_run_game_objects
	bset	game_flags_1,#flag1_skip_player
	call	run_game_objects
skip_run_game_objects
	bres	game_flags_1,#flag1_tweak
	call	game_loop_step
	call	cursor_next_alien
	mov	vblank_status,#$80
	ld	a,{alien_rolling_timer+timer_extra_count_offs}
	ld	shot_sync,a
	call	draw_alien
	bres	game_flags_1,#flag1_skip_player
	call	run_game_objects
	call	start_saucer
	jp	game_loop_step
;=============================================
;
;
;=============================================
game_loop_step
	call	player_fire_or_demo
	call	player_shot_hit
	call	count_aliens
	btjf	game_flags_1,#flag1_game_mode,game_loop_game_mode
	call	attract_task
game_loop_game_mode
	;TODO Add non-demo mode code
	ret
;=============================================
;
; Check for player fire or always fire
; in demo mode.
;
;=============================================
player_fire_or_demo
	ld	a,player_alive
	cp	a,player_alive_alive
	jreq	player_fire_or_demo_ret
	ldw	y,{player_base_timer+timer_tick_offs}
	jrne	player_fire_or_demo_ret
	ldw	y,player_shot_status
	cpw	y,#player_shot_available
	jrne	player_fire_or_demo_ret
	btjf	game_flags_1,#flag1_game_mode,player_fire_game
	ldw	y,#player_shot_initiated
	ldw	player_shot_status,y
	jp	increment_demo_command
player_fire_game
	; TODO check switches
player_fire_or_demo_ret
	ret
;=============================================
;
; Check if the player shot has hit something
;
;=============================================
player_shot_hit
	ldw	y,player_shot_status
	cpw	y,#player_shot_normal_move
	jrne	player_shot_hit_ret
	ld	a,{sp_player_shot+sprite_y_offs}
	cp	a,#$d8	;Off top of screen?
	jrult	check_alien_exploding
	ldw	y,#player_shot_hit_something
	ldw	player_shot_status,y
	bres	game_flags_2,#flag2_alien_exploding
check_alien_exploding
	btjf	game_flags_2,#flag2_alien_exploding,player_shot_hit_ret
	cp	a,#$ce	;Hit saucer?
	jrult	check_alien_hit
	bset	{alien_squigly_shot+shot_flags_offs},#saucer_hit
	bres	game_flags_2,#flag2_alien_exploding
	ldw	y,#player_shot_alien_exploded
	ldw	player_shot_status,y
	ret
check_alien_hit
	bres	game_flags_2,#flag2_player_hit_alien
	cp	a,ref_alien_y
	jrult	check_player_hit_alien_flag
	ld	a,ref_alien_y
	add	a,#8
	clrw	y
	ld	yl,a
find_alien_row_loop	
	ld	a,yl
	cp	a,{sp_player_shot+sprite_y_offs}
	jruge	found_alien_row
	add	a,#$10
	ld	yl,a
	ld	a,yh
	add	a,#11
	ld	yh,a
	jra	find_alien_row_loop
check_player_hit_alien_flag
	btjt game_flags_2,#flag2_player_hit_alien,player_shot_hit_ret
	bres	game_flags_2,#flag2_alien_exploding
	ldw	y,#player_shot_hit_something
	ldw	player_shot_status,y
player_shot_hit_ret	
	ret
found_alien_row	
	ld	a,yh
	ld	shothit_alien_index,a
	ld	a,{sp_player_shot+sprite_x_offs}
	call	find_column
	ld	yl,a	;Save column in yl for later
	cp	a,#0
	jrult	check_player_hit_alien_flag
	cp	a,#10
	jrugt	check_player_hit_alien_flag
	add	a,shothit_alien_index
	ld	shothit_alien_index,a
	clrw	x
	ld	xl,a
	addw	x,current_player
	ld	a,(aliens_offs,x)
	jreq	check_player_hit_alien_flag
	ld	a,#0	;get rid of the alien
	ld	(aliens_offs,x),a
	ld	a,yl
	sll	a
	sll	a
	sll	a
	sll	a
	add	a,ref_alien_x
	ld	shothit_col_x,a
; If we haven't draw this alien yet in the new ref_alien_x 
; then the adjust the ColX back to the correct position. 
; Y is correct as we use the sprite position rounded.	
	ld	a,shothit_alien_index
	cp	a,alien_cur_index
	jrule	no_delta_x_adjust
	ld	a,numaliens
	cp	a,1
	jreq	no_delta_x_adjust
	ld	a,shothit_col_x
	add	a,ref_alien_delta_x
	ld	shothit_col_x,a
no_delta_x_adjust
	mov	alien_explode_timer,#$10
	ld	a,shothit_col_x
	srl	a
	srl	a
	srl	a
	ld	alien_explode_x,a
	ld	a,shothit_col_x
	and	a,#7
	ld	alien_explode_x_offset,a
	ld	a,{sp_player_shot+sprite_y_offs}
	ld	alien_explode_y,a
	call	explode_alien
	mov 	{sp_player_shot+sprite_visible},#0
	ldw	y,#player_shot_alien_exploding
	ldw	player_shot_status,y
	bset	game_flags_2,#flag2_player_hit_alien
	;phew now figure out what the player scored....
	ldw	y,#$0010
	ld	a,shothit_alien_index
	cp	a,{11 mult 2}
	jrult 	store_score
	ldw	y,#$0020
	cp	a,{11 mult 4}
	jrult 	store_score
	ldw	y,#$0030
store_score	
	ldw	score_delta,y
	bset	game_flags_2,#flag2_adjust_score
	jp	check_player_hit_alien_flag
;=============================================
;
;	Start the saucer if the timer
; 	has expired.
;
;=============================================
start_saucer.w
	ld	a,ref_alien_x
	cp	a,#$78
	jrult	start_saucer_ret
	ldw	y,time_to_saucer
	jrne	start_saucer_decy
	ldw	y,#$0600
	bset	{alien_squigly_shot+shot_flags_offs},#saucer_start
start_saucer_decy
	decw	y
	ldw	time_to_saucer,y
start_saucer_ret	
	ret
;=============================================
;
;	stub routines start here
;
;=============================================
handle_coin_switch
	ret
enter_wait_start_loop
	ret
.player_ship_blown_up.w
	jra	player_ship_blown_up
;=============================================
;
;	interrupt vector loop
;
;=============================================
	segment 'vectit'
	dc.l {$82000000+main}			; reset
	dc.l {$82000000+NonHandledInterrupt}	; trap
	dc.l {$82000000+NonHandledInterrupt}	; irq0
	dc.l {$82000000+NonHandledInterrupt}	; irq1
	dc.l {$82000000+NonHandledInterrupt}	; irq2
	dc.l {$82000000+DMAChannel23Int}	; irq3
	dc.l {$82000000+NonHandledInterrupt}	; irq4
	dc.l {$82000000+NonHandledInterrupt}	; irq5
	dc.l {$82000000+NonHandledInterrupt}	; irq6
	dc.l {$82000000+NonHandledInterrupt}	; irq7
	dc.l {$82000000+NonHandledInterrupt}	; irq8
	dc.l {$82000000+NonHandledInterrupt}	; irq9
	dc.l {$82000000+NonHandledInterrupt}	; irq10
	dc.l {$82000000+NonHandledInterrupt}	; irq11
	dc.l {$82000000+NonHandledInterrupt}	; irq12
	dc.l {$82000000+NonHandledInterrupt}	; irq13
	dc.l {$82000000+NonHandledInterrupt}	; irq14
	dc.l {$82000000+NonHandledInterrupt}	; irq15
	dc.l {$82000000+NonHandledInterrupt}	; irq16
	dc.l {$82000000+NonHandledInterrupt}	; irq17
	dc.l {$82000000+NonHandledInterrupt}	; irq18
	dc.l {$82000000+NonHandledInterrupt}	; Timer 2 Update/overflow
	dc.l {$82000000+NonHandledInterrupt}	; Timer 2 capture/compare
	dc.l {$82000000+NonHandledInterrupt}	; Timer 3 Update/overflow
	dc.l {$82000000+Timer3CompareInt}	; Timer 3 capture/compare
	dc.l {$82000000+NonHandledInterrupt}	; irq23
	dc.l {$82000000+NonHandledInterrupt}	; irq24
	dc.l {$82000000+NonHandledInterrupt}	; irq25
	dc.l {$82000000+NonHandledInterrupt}	; irq26
	dc.l {$82000000+NonHandledInterrupt}	; irq27
	dc.l {$82000000+NonHandledInterrupt}	; irq28
	dc.l {$82000000+NonHandledInterrupt}	; irq29

	end
