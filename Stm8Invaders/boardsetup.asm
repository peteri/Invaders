stm8/
	.tab	0,8,16,60

	#include "mapping.inc"
	#include "variables.inc"
	#include "stm8l152c6.inc"
	#include "videosync.inc"
	#include "linerender.inc"
ram0_start.b	EQU $ram0_segment_start
ram0_end.b	EQU $ram0_segment_end
ram1_start.w	EQU $ram1_segment_start
ram1_end.w	EQU $ram1_segment_end
; Video data starts 6uS (5.7us is front porch)
; after rising edge
spi_data_start	EQU  $48
line_sync	EQU $4B	;75 decimal	 
TIM2_CC3	EQU {spi_data_start + line_sync +2} ; $AB or 171
TIM4_reload	EQU {{{spi_data_start + $330} / 4}-1}
	segment 'ram1'
disableTim2	DS.B	1	; Sent by DMA channel 1
enableTim2	DS.B	1	; Sent by DMA channel 0
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
; Timer 2 out on PB0 (wired to PB5 SPI-CK)
	bset PB_DDR,#0
	bset PB_CR1,#0
; Timer 3 CC1 out on PB1 Frame
	bset PB_DDR,#1
	bset PB_CR1,#1
; SPI MISO on PB7 Video Data
	bset PB_DDR,#7
	bset PB_CR1,#7
; Timer 3 CC2 out on PD0 Frame
	bset PD_DDR,#0
	bset PD_CR1,#0
; Test for SPI stuff
; SPI MOSI on PB6 Video Data
;	bset PB_DDR,#6
;	bset PB_CR1,#6
; SPI CK on PB6 Video Data
;	bset PB_DDR,#5
;	bset PB_CR1,#5
	
	ret
;=================================================
;
; Setup timers
;
;=================================================
.init_timers.w
;
; Timer 1 - Compare channel 1 is output on PD2 
; Used for video sync.
;
	mov TIM1_CR1,#%10000000   ; TIM1 Control register 1
; Master Mode is OC1 REF
	mov TIM1_CR2,#%01000000	  ; TIM1 Control register 2
	mov TIM1_SMCR,#%00000000  ; TIM1 Slave Mode Control register
	mov TIM1_ETR,#$00	  ; TIM1 external trigger register
; DMA request on capture compare 2 & 3
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
; Gets written two by DMA Channel 3 when
; compare register 2 fires, this is the output signal
; to timer 3 and the SYNC pin
	mov TIM1_CCR1H,#$00	  ; TIM1 Capture/Compare Register 1 High
	mov TIM1_CCR1L,#$00	  ; TIM1 Capture/Compare Register 1 Low
; Compare register 2 triggers DMA channel 3
; which writes to compare register 1
	mov TIM1_CCR2H,#$01	  ; TIM1 Capture/Compare Register 2 High
	mov TIM1_CCR2L,#$f0	  ; TIM1 Capture/Compare Register 2 Low
; Compare register 3 triggers serial clocking
; by writing using DMA Channel 0 to write to...
	mov TIM1_CCR3H,#{high TIM2_CC3}  ; TIM1 Capture/Compare Register 3 High
	mov TIM1_CCR3L,#{low TIM2_CC3}  ; TIM1 Capture/Compare Register 3 High
; Not used at the moment.	
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
; Used for the SPI clock @5.333MHz
;
; ARPE=1, CMS=Edge(00),DIR=Up(0),OPM=0,URS=???, UDIS=???, CEN=1
	mov TIM2_CR1, #%10000000  ; TIM2 Control register 1
	mov disableTim2, #%10000000
	mov enableTim2, #%10000001
; TI1S=0; MMS=000; CCDS=0; 000
	mov TIM2_CR2, #%00000000  ; TIM2 Control register 2
; Not in slave mode
	mov TIM2_SMCR,#%00010100  ; TIM2 Slave Mode Control register
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
; Slave mode Trigger select is TIM1 mode is TIM1 out is Clock 
	mov TIM3_SMCR,#%10010111  ; TIM3 Slave Mode Control register
; No reason for an external trigger	
	mov TIM3_ETR,#$00	  ; TIM3 External trigger register
; No DMA 
	mov TIM3_DER,#%00000000	  ; TIM3 DMA request enable register
; Interrupts on compare and update
	mov TIM3_IER,#%00000110	  ; TIM3 Interrupt enable register
; Set to PWM1, OC1PE, Capture compare is output
	mov TIM3_CCMR1,#%01100000 ; TIM3 Capture/Compare mode register 1
	mov TIM3_CCMR2,#%01100000 ; TIM3 Capture/Compare mode register 2
; Capture compare is active high, output enable	
	mov TIM3_CCER1,#%00010001 ; TIM3 Capture/Compare enable register 1
	mov TIM3_PSCR,#$00	  ; TIM3 Prescaler register
; Reload register at ((625+15)/2)-1
	mov TIM3_ARRH,#$02	  ; TIM3 Auto-Reload Register High
	mov TIM3_ARRL,#$7f	  ; TIM3 Auto-Reload Register Low
; No of lines to compare start video at 32
	mov TIM3_CCR1H,#$00	  ; TIM3 Capture/Compare Register 1 High
	mov TIM3_CCR1L,#$38	  ; TIM3 Capture/Compare Register 1 Low
; Start frame 2 at 32+320 
	mov TIM3_CCR2H,#$01	  ; TIM3 Capture/Compare Register 2 High
	mov TIM3_CCR2L,#$78	  ; TIM3 Capture/Compare Register 2 Low
	mov TIM3_BKR,#%11000100	  ; TIM3 Break register
	mov TIM3_OISR,#$00	; TIM3 Output idle state register
	bset TIM3_EGR,#0	; Trigger update event so preload-registers copy
;
;	Timer 4
;
	mov TIM4_CR1, #%10000100 ; TIM4 Control Register 1
	mov TIM4_CR2, #%00000000 ; TIM4 Control Register 2
	mov TIM4_SMCR,#%10010100 ; TIM4 Slave Mode Control Register
	mov TIM4_DER, #%00000001 ; TIM4 DMA request Enable Register
	mov TIM4_IER, #$00	 ; TIM4 Interrupt Enable Register
	mov TIM4_CNTR,#$00	 ; TIM4 Counter
	mov TIM4_PSCR,#$02	 ; TIM4 Prescaler Register divide by 4
	mov TIM4_ARR, #TIM4_reload ; TIM4 Auto-Reload Register
	bset TIM4_EGR, #0	 ; TIM4 Event Generation Register
;
;	Turn on timers
;
;
 	bset TIM4_CR1, #$0  	; Enable timer 4
	bset TIM3_CR1, #$0  	; Enable timer 3
	; Timer 2 is controlled by DMA
	bset TIM1_CR1, #$0	; Enable timer 1
	ret

;=========================================================
;
;	Setup CPU
;
;=========================================================
.init_cpu.w
; Use external clock on PA2
	mov CLK_ECKCR,#%00010001
; Wait for HSERDY
clk_hse_rdy_set
	btjf CLK_ECKCR,#1,clk_hse_rdy_set
	mov CLK_SWR,#%00000100
	bset CLK_SWCR,#1	;Swap clock
; Wait for clock switch busy clear...
clk_sw_busy_clear
	btjt CLK_SWCR,#0,clk_sw_busy_clear
	mov CLK_CKDIVR,#$00	; Full speed 16Mhz
	; Timer 4 DMA is on Channel 1
	mov SYSCFG_RMPCR1,#%0000100
	bset CLK_PCKENR1,#$0	; Send the clock to timer 2
	bset CLK_PCKENR1,#$1	; Send the clock to timer 3
	bset CLK_PCKENR1,#$2	; Send the clock to timer 4
	bset CLK_PCKENR1,#$4	; Send the clock to SPI1
  	bset CLK_PCKENR2,#$1	; Send the clock to timer 1
	bset CLK_PCKENR2,#$4	; Turn on DMA1
	bres ITC_SPR6,#5	; lower priority of Tim 3 capture
	ret
;=========================================================
;
;	Setup SPI 1
;
;=========================================================
.init_spi1.w
;	mov SPI1_CR1,#%10000000
;	mov SPI1_CR2,#%11000011
	;mov SPI1_ICR,#%00000010
	ret
;=========================================================
;	Setup DMA
;
; Channel | Peripheral     | Notes
;    0    | Timer 1 CC3    | SPI Clock (timer 2) Enable
;    1    | Timer 4 Update | SPI Clock (timer 2) Disable
;    2    | SPI TX 1       | Sends the video data
;    3    | Timer 1 CC2    | Update sync PWM every 32us
;=========================================================
.init_dma.w
; Channel 0
	mov DMA1_C0SPR,#%00110000 ; Hi priority 8 bit
	mov DMA1_C0NDTR,#$01	  ; One byte  to transfer
	mov DMA1_C0PARH,#{high TIM2_CR1} ; Timer two control register
	mov DMA1_C0PARL,#{low TIM2_CR1} ; Timer two control register
	mov DMA1_C0M0ARH,#{high enableTim2}
	mov DMA1_C0M0ARL,#{low enableTim2}
	mov DMA1_C0CR,#%00111001  ; Enable channel (Circ)
; Channel 1	
	mov DMA1_C1SPR,#%00110000 ; Hi priority 8 bit
	mov DMA1_C1NDTR,#$01	  ; One byte  to transfer
	mov DMA1_C1PARH,#{high TIM2_CR1} ; Timer two control register
	mov DMA1_C1PARL,#{low TIM2_CR1} ; Timer two control register
	mov DMA1_C1M0ARH,#{high disableTim2}
	mov DMA1_C1M0ARL,#{low disableTim2}
	mov DMA1_C1CR,#%00111001  ; Enable channel (Circ)
; Channel 2
	mov DMA1_C2SPR,#%00110000 ; Hi priority 8 bit
	mov DMA1_C2NDTR,#$44	  ; 32 bytes  to transfer
	mov DMA1_C2PARH,#{high SPI1_DR}  ; SPI Data register
	mov DMA1_C2PARL,#{low SPI1_DR}  ; SPI Data register
	mov DMA1_C2M0ARH,#{high renderbuff1}
	mov DMA1_C2M0ARL,#{low renderbuff1}
	mov DMA1_C2CR,#%00111001  ; Incrementing mem to periph
; Channel 3
	mov DMA1_C3SPR,#%00111000
	mov DMA1_C3NDTR,#$80	; 128 words
	ldw x,#TIM1_CCR1H
	ldw DMA1_C3PARH_C3M1ARH,x
	ldw x,#synccomp
	ldw syncdma,x
	inc syncdma	;Add $0100 for the first row.
	ldw DMA1_C3M0ARH,x
; regular channel, memory increment, circ off,
; mem to periperal, trans comp int, channel enable
	mov DMA1_C3CR,#%00101011
; Turn on DMA
	mov DMA1_GCSR,#%00000001
	ret
	end
