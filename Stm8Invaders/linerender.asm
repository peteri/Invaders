stm8/
	.tab	0,8,16,60

	#include "characterrom.inc"
	#include "variables.inc"
	segment 'ram0'
store.b ds.w
romhi.b ds.b
linemasked.b ds.b
count.b ds.b
	segment 'ram1'
.screen.w ds.b $380
.linenumber.w ds.w 1
.renderbuff1.w ds.b $22
.renderbuff2.w ds.b $22
temp ds.w
	segment 'rom'
;
; On entry X is where to store the data.
;
.renderline.w
	ldw store,x
	; make x the start of the line
	clrw x
	ldw x,linenumber
	sllw x
	sllw x
	ld a,xl
	and a,$e0
	ld xl,a
	
	ld a,{linenumber+1}
	and a,#7
	ld linemasked,a
	add a,#{high charrom}
	ld romhi,a
	
	mov count,#32
renderloop:
	ld a,(screen,x)		; 1
	incw x            ; 1
	bcp a,#$e0        ;1
	jrne rendercharacter
renderudg
	clrw y		;1
	sll a		;1
	sll a		;1
	sll a		;1
	add a,linemasked ;1 
	ld yl,a          ;1
	ld a,(udg,y)     ;1
	ldw y,store      ;2 
	ld (y),a         ;1
	inc {store+1}    ;1
	dec count        ;1
	jrne renderloop  ;1 =14
	ret
rendercharacter
	ld yl,a           ;1
	ld a,romhi        ;1
	ld yh,a           ;1
	ld a,(y)          ;1
 	ldw y,store       ;2 
	ld (y),a          ;1
	inc {store+1}     ;1
	dec count         ;1
	jrne renderloop   ;2 =11
	ret
.setup_screen_diag.w
; Fill screen with Asterix first
	ldw y,#$37f
	ld a,#$4E	; Asterix
blankloop	
	ld (screen,y),a
	decw y
	jrpl blankloop
; Fill render buff with $55,$AA
	ldw y,#$21
fillrenderbuff	
	ld a,#$aa
	ld (renderbuff1,y),a
	ld (renderbuff2,y),a
	decw y
	ld a,#$55
	ld (renderbuff1,y),a
	ld (renderbuff2,y),a
	decw y
	cpw y,#$FFFF
	jrne fillrenderbuff
	ld a,#$42		;Add an elephant
	ld {renderbuff1+$00},a
	ld {renderbuff2+$00},a
	ld {renderbuff1+$21},a
	ld {renderbuff2+$21},a
; Now we put a character map on screen 	
	ldw x,#$ff
fillscreenloop
	; y=(x&0xf)*32 +(x>>4)
	ld a,xl
	and a,#$0f
	clrw y
	ld yl,a
	sllw y
	sllw y
	sllw y
	sllw y
	sllw y
	ld a,xl
	srl a
	srl a
	srl a
	srl a
	ld temp,a
	ld a,yl
	add a,temp
	ld yl,a
	ld a,xl
	ld (screen,y),a
	decw x
	jrpl fillscreenloop
	ldw x,#$ff
filludgloop
	ld a,xl
	ld (udg,x),a
	decw x
	jrpl filludgloop
	ret
	end
	