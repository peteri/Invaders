stm8/
	.tab	0,8,16,60

	#include "mapping.inc"
	#include "variables.inc"
	#include "stm8l152c6.inc"
	#include "videosync.inc"
ram0_start.b	EQU $ram0_segment_start
ram0_end.b	EQU $ram0_segment_end
ram1_start.w	EQU $ram1_segment_start
ram1_end.w	EQU $ram1_segment_end	
	segment 'rom'
;
; Setup up the gpio ports
;
.init_gpio.w
; set everything to have a pullup resistor in input mode
	mov PA_CR1,#$FF
	mov PB_CR1,#$FF
	mov PC_CR1,#$FF
	mov PD_CR1,#$FF
	mov PE_CR1,#$FF
	mov PA_CR2,#$00
	mov PB_CR2,#$00
	mov PC_CR2,#$00
	mov PD_CR2,#$00
	mov PE_CR2,#$00
	mov PA_DDR,#$00
	mov PB_DDR,#$00
	mov PC_DDR,#$00
	mov PD_DDR,#$00
	mov PE_DDR,#$00
; setup for LEDs	
	bset PE_DDR,#7
	bset PE_CR1,#7
	bset PC_DDR,#7
	bset PC_CR1,#7
; Timer 1 output on PD2 (Sync)
	bset PD_DDR,#2
	bset PD_CR1,#2
; Timer 2 out on PB0 (used for SPI clock slave pin)
	bset PB_DDR,#0
	bset PB_CR1,#0
; Timer 3 out on PB1 Frame
	bset PB_DDR,#1
	bset PB_CR1,#1
	ret
;=================================================
;
; Setup timers
;
;=================================================
.init_timers.w
;
; Timer 1 - Compare channel 1 is output on PD2 
;
	mov TIM1_CR1,#%10000000   ; TIM1 Control register 1
; Master Mode is OC1 REF
	mov TIM1_CR2,#%01000000	  ; TIM1 Control register 2
	mov TIM1_SMCR,#%00000000  ; TIM1 Slave Mode Control register
	mov TIM1_ETR,#$00	  ; TIM1 external trigger register
; DMA request on capture compare 2	
	mov TIM1_DER,#%00000100	  ; TIM1 DMA request enable register
	mov TIM1_IER,#$00	  ; TIM1 Interrupt enable register
	mov TIM1_SR1,#$00	  ; TIM1 Status register 1
	mov TIM1_SR2,#$00	  ; TIM1 Status register 2
	mov TIM1_EGR,#$00	  ; TIM1 Event Generation register
	mov TIM1_CCMR1,#%01111000 ; TIM1 Capture/Compare mode register 1
	mov TIM1_CCMR2,#$00	  ; TIM1 Capture/Compare mode register 2
	mov TIM1_CCMR3,#$00	  ; TIM1 Capture/Compare mode register 3
	mov TIM1_CCMR4,#$00	  ; TIM1 Capture/Compare mode register 4
	mov TIM1_CCER1,#%00000001 ; TIM1 Capture/Compare enable register 1
	mov TIM1_CCER2,#%01101000  ; TIM1 Capture/Compare enable register 2
	mov TIM1_CNTRH,#$00	  ; TIM1 Counter High
	mov TIM1_CNTRL,#$00	  ; TIM1 Counter Low
	mov TIM1_PSCRH,#$00	  ; TIM1 Prescaler Register High
	mov TIM1_PSCRL,#$00	  ; TIM1 Prescaler Register Low
; Period is 64*16 cycles $200-1	
	mov TIM1_ARRH,#$01	  ; TIM1 Auto-Reload Register High
	mov TIM1_ARRL,#$ff	  ; TIM1 Auto-Reload Register Low
	mov TIM1_RCR,#$00	  ; TIM1 Repetition counter register
; Compare register off the end		
	mov TIM1_CCR1H,#$00	  ; TIM1 Capture/Compare Register 1 High
	mov TIM1_CCR1L,#$00	  ; TIM1 Capture/Compare Register 1 Low
; Compare register 2 triggers DMA channel 3
; which writes to compare register 1	
	mov TIM1_CCR2H,#$00	  ; TIM1 Capture/Compare Register 2 High
	mov TIM1_CCR2L,#$00	  ; TIM1 Capture/Compare Register 2 Low
; Compare register 3 triggers serial clocking
	mov TIM1_CCR3H,#$00	  ; TIM1 Capture/Compare Register 3 High
	mov TIM1_CCR3L,#$80	  ; TIM1 Capture/Compare Register 3 Low
	mov TIM1_CCR4H,#$00	  ; TIM1 Capture/Compare Register 4 High
	mov TIM1_CCR4L,#$00	  ; TIM1 Capture/Compare Register 4 Low
	mov TIM1_BKR,#%11000100	  ; TIM1 Break register
	mov TIM1_DTR,#$00	  ; TIM1 Dead-time register
	mov TIM1_OISR,#$00	  ; TIM1 Output idle state register
	mov TIM1_DCR1,#$00	  ; TIM1 DMA control register 1
	mov TIM1_DCR2,#$00	  ; TIM1 DMA control register 2
	mov TIM1_DMAR,#$00	  ; TIM1 DMA address for burst mode
	
	bset TIM1_EGR,#$0	; Trigger update event so preload-registers copy

;
; Timer 2 - Compare channel 1 is output on PB0
; Used for video sync.
;
; ARPE=1, CMS=Edge(00),DIR=Up(0),OPM=0,URS=???, UDIS=???, CEN=1
	mov TIM2_CR1, #%10000000  ; TIM2 Control register 1
; TI1S=0; MMS=000; CCDS=0; 000
	mov TIM2_CR2, #%00000000  ; TIM2 Control register 2
; Not in slave mode
	mov TIM2_SMCR,#%00010101  ; TIM2 Slave Mode Control register
; No reason for an external trigger	
	mov TIM2_ETR,#$00	  ; TIM2 External trigger register
; DMA request on capture compare 2	
	mov TIM2_DER,#%00000000	  ; TIM2 DMA request enable register
; No interrupts	
	mov TIM2_IER,#$00	  ; TIM2 Interrupt enable register
; Set to PWM1, OC1PE, Capture compare is output
	mov TIM2_CCMR1,#%01111000 ; TIM2 Capture/Compare mode register 1
	mov TIM2_CCMR2,#%00000000 ; TIM2 Capture/Compare mode register 2
; Capture compare is active high, output enable	
	mov TIM2_CCER1,#%00000001 ; TIM2 Capture/Compare enable register 1
	mov TIM2_PSCR,#$00	  ; TIM2 Prescaler register
; Period is 3
	mov TIM2_ARRH,#$0	  ; TIM2 Auto-Reload Register High
	mov TIM2_ARRL,#$2	  ; TIM2 Auto-Reload Register Low
; Compare register at 2
	mov TIM2_CCR1H,#$00	  ; TIM2 Capture/Compare Register 1 High
	mov TIM2_CCR1L,#$02	  ; TIM2 Capture/Compare Register 1 Low
; Compare register 2 
	mov TIM2_CCR2H,#$00	  ; TIM2 Capture/Compare Register 2 High
	mov TIM2_CCR2L,#$00	  ; TIM2 Capture/Compare Register 2 Low
	mov TIM2_BKR,#%11000100	  ; TIM2 Break register
	mov TIM2_OISR,#$00	; TIM2 Output idle state register
	bset TIM2_EGR,#0	; Trigger update event so preload-registers copy
;	
; Timer 3 counts frames....
; Output is PB1
;
; ARPE=1, CMS=Edge(00),DIR=Up(0),OPM=0,URS=???, UDIS=???, CEN=1
	mov TIM3_CR1, #%10000000  ; TIM3 Control register 1
; TI1S=0; MMS=010; CCDS=1; 000
	mov TIM3_CR2, #%00100000  ; TIM3 Control register 2
; Slave mode clock is TIM1
	mov TIM3_SMCR,#%10010111  ; TIM3 Slave Mode Control register
; No reason for an external trigger	
	mov TIM3_ETR,#$00	  ; TIM3 External trigger register
; No DMA 
	mov TIM3_DER,#%00000000	  ; TIM3 DMA request enable register
; Interrupts on compare and update
	mov TIM3_IER,#%00000010	  ; TIM3 Interrupt enable register
; Set to PWM1, OC1PE, Capture compare is output
	mov TIM3_CCMR1,#%01100000 ; TIM3 Capture/Compare mode register 1
	mov TIM3_CCMR2,#$00	  ; TIM3 Capture/Compare mode register 2
; Capture compare is active high, output enable	
	mov TIM3_CCER1,#%00000001 ; TIM3 Capture/Compare enable register 1
	mov TIM3_PSCR,#$00	  ; TIM3 Prescaler register
; Reload register at (625*2)-1
	mov TIM3_ARRH,#$02	  ; TIM3 Auto-Reload Register High
	mov TIM3_ARRL,#$7f	  ; TIM3 Auto-Reload Register Low
; No of lines to compare
	mov TIM3_CCR1H,#$00	  ; TIM3 Capture/Compare Register 1 High
	mov TIM3_CCR1L,#$1	  ; TIM3 Capture/Compare Register 1 Low
	mov TIM3_CCR2H,#$00	  ; TIM3 Capture/Compare Register 2 High
	mov TIM3_CCR2L,#$00	  ; TIM3 Capture/Compare Register 2 Low
	mov TIM3_BKR,#%11000100	  ; TIM3 Break register
	mov TIM3_OISR,#$00	; TIM3 Output idle state register
	bset TIM3_EGR,#0	; Trigger update event so preload-registers copy
;
;	Turn on timers
;
	bset TIM2_CR1, #$0  	; Enable timer 2
	bset TIM3_CR1, #$0  	; Enable timer 3
	bset TIM1_CR1, #$0	; Enable timer 1
	ret
;
; Setup CPU
;
.init_cpu.w
	mov CLK_CKDIVR,#$00	; Full speed 16Mhz
	bset CLK_PCKENR2,#$1	; Send the clock to timer 1
	bset CLK_PCKENR1,#$0	; Send the clock to timer 2
	bset CLK_PCKENR1,#$1	; Send the clock to timer 3
	bset CLK_PCKENR2,#$4	; Turn on DMA1
	ret
;
;	Setup DMA
;
.init_dma.w
; Timer 1 CC2 is on channel 3
	mov DMA1_C3SPR,#%00111000
	mov DMA1_C3NDTR,#$80	; 128 words
	ldw x,#TIM1_CCR1H
	ldw DMA1_C3PARH_C3M1ARH,x
	ldw x,#synccomp
	ldw syncdma,x
	inc syncdma	;Add $0100 for the first row.
	ldw DMA1_C3M0ARH,x
; regular channel, memory increment, circ off,
; mem to periperal, no interrupts, channel enable
	mov DMA1_C3CR,#%00101011
; Turn on DMA
	mov DMA1_GCSR,#%00000001
	ret
	end
