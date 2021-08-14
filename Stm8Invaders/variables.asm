stm8/
	.tab	0,8,16,60

;=============================================
; Contain all the varible definitions
;=============================================
	#include "mapping.inc"
	#include "player.inc"
	#include "constants.inc"
ram0_start.b	EQU $ram0_segment_start
ram0_end.b	EQU $ram0_segment_end
ram1_start.w	EQU $ram1_segment_start
ram1_end.w	EQU $ram1_segment_end	

	segment 'rom'
;=============================
; Helper routine to clear ram
;=============================
.clear_memory.w
	ldw X,#ram0_start
clear_ram0.l
	clr (X)
	incw X
	cpw X,#ram0_end	
	jrule clear_ram0
	; clear RAM1
	ldw X,#ram1_start
clear_ram1.l
	clr (X)
	incw X
	cpw X,#ram1_end	
	jrule clear_ram1
	ret
;==================================
;
; Variables in zero page start here
;
;==================================
	segment 'ram0'
.led_state.b	ds.b	1
.linenumber.b	ds.w	1
.syncdma.b	ds.w	1	
.renderbuff1.b	ds.b 	{scr_width+2}
.renderbuff2.b	ds.b	{scr_width+2}
.current_player.b ds.w	1
.numaliens.b	ds.b	1
.hi_score.b	ds.w	1
.credits.b	ds.b	1
.isr_delay.b	ds.b	1
.game_mode.b	ds.b	1
.demo_mode.b	ds.b	1
.alien_shot_reload_rate.b ds.b	1
;==================================
;
; Variables in rest of ram start here
;
;==================================
	segment 'ram1'
.screen.w 	ds.b 	{scr_height mult scr_width}
.udg.w		DS.B	$100
.player_one.w	ds.b	player_end_offs	
.player_two.w	ds.b	player_end_offs
	end
	