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
stack_start.w	EQU $stack_segment_start
stack_end.w	EQU $stack_segment_end
	segment 'ram0'
tim3cntr.w ds.w 1
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
	call init_gpio	  ; setup the gpio pins
	call init_dma	  ; setup dma channels
	call init_timers  ; setup the timers.
	call init_spi1	  ; setup SPI1 for video out
	call power_on_reset
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
	mov	game_mode,#0
	mov	demo_mode,#0
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
	cpw	y,#{scr_height mult 8 +1}	;28*8 lines
	jrule	renderloop	;Not done yet
	bres	TIM1_DER,#3	; Turn off CC3 DMA
	; Do the in game frame tick
	call	game_tick
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
;=============================================
;
; 	Main tick routine
;	Called from frame interrupt.
;
;=============================================
game_tick
	dec	isr_delay
	call	handle_coin_switch
	btjf	suspend_play,#0,not_wait_task
	jp	run_wait_task
not_wait_task	
	btjt	game_mode,#0,run_game
	btjt	demo_mode,#0,run_game
	ld	a,credits
	jreq	do_attract_screen
	jp	enter_wait_start_loop
do_attract_screen
	jp	attract_task
run_game
	call	game_loop_step
	mov	vblank_status,#0
	btjf	tweak_flag,#0,skip_run_game_objects
	mov	skip_player,#1
	call	run_game_objects
skip_run_game_objects
	mov	tweak_flag,#0
	call	game_loop_step
	call	cursor_next_alien
	mov	vblank_status,#$80
;	ld	a,{alien_shot_rolling+extra_count_offs}
;	ld	shot_sync,a
	call	draw_alien
	mov	skip_player,#0
	call	run_game_objects
	call	time_to_saucer
	jp	game_loop_step
;=============================================
;
;	stub routines start here
;
;=============================================
handle_coin_switch
	ret
game_loop_step
	ret
cursor_next_alien
	ret
draw_alien
	ret
time_to_saucer
	ret
run_game_objects
	ret
enter_wait_start_loop
	ret
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
