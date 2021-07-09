stm8/
	.tab	0,8,16,60

	#include "mapping.inc"
	#include "variables.inc"
	#include "stm8l152c6.inc"
	segment 'rom'

;=====================================================
;
; This is a table of video sync values for PAL timings
; Assuming a fsclk of 16MHz
; Timer 2 runs with a 32us cycle which is 512 clocks.
; pulse widths are (Using values from ITU-R BT.470.5)
;   | Value          | Clocks | Actual  | Error
; d |  4.70uS +- 0.2 |  75    |  4.6875 | 0.0125
; p |  2.35uS +- 0.1 |  38    |  2.3750 | 0.0250
; q | 27.30uS +- 0.1 | 437    | 27.3125 | 0.0125
;
;=====================================================
no_sync		EQU 0
line_sync	EQU 75	;512-74 clocks is divisable 3 
short_sync	EQU 38
long_sync	EQU 437
.synccomp.w

	DC.W long_sync,long_sync 	;  1 sync pulses
	DC.W long_sync,long_sync        ;  2 
	DC.W long_sync,short_sync       ;  3
	DC.W short_sync,short_sync      ;  4
	DC.W short_sync,short_sync      ;  5
	DC.W line_sync,no_sync		;  6 Field blanking
blankcount	CEQU	7
	REPEAT
	DC.W line_sync,no_sync          ;  ....  
blankcount CEQU {blankcount+1}
	UNTIL {blankcount eq 23}
	DC.W line_sync,no_sync          ; 23 Video starts in second half  
	DC.W line_sync,no_sync          ; 24 Video 
videocount	CEQU	25
	REPEAT
	DC.W line_sync,no_sync          ;  .... 
videocount CEQU {videocount+1}
	UNTIL {videocount eq 310}
	DC.W line_sync,no_sync          ;310 Video 
	DC.W short_sync,short_sync	;311 eq pulses  
	DC.W short_sync,short_sync      ;312
	DC.W short_sync,long_sync       ;313
	DC.W long_sync,long_sync 	;314 sync pulses
	DC.W long_sync,long_sync        ;315 
	DC.W short_sync,short_sync	;316 eq pulses  
	DC.W short_sync,short_sync      ;317
	DC.W short_sync,no_sync         ;318
	DC.W line_sync,no_sync		;319 Field blanking
blankcount	CEQU	320
	REPEAT
	DC.W line_sync,no_sync          ;  ....  
blankcount CEQU {blankcount+1}
	UNTIL {blankcount eq 336}
	DC.W line_sync,no_sync          ;336 Video 
	DC.W line_sync,no_sync          ;337 Video 
videocount	CEQU	338
	REPEAT
	DC.W line_sync,no_sync          ;  .... 
videocount CEQU {videocount+1}
	UNTIL {videocount eq 622}
	DC.W line_sync,no_sync		;622 Video Line 
	DC.W line_sync,short_sync	;623 Half line  
	DC.W short_sync,short_sync	;624 eq pulses  
	DC.W short_sync,short_sync      ;625	
.synccompend.w
	end
	