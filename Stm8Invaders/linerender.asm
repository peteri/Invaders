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
.linenumber.w ds.w
renderbuff1 ds.b 32
renderbuff2 ds.b 32
	segment 'rom'
.renderline.w
	ldw x,#renderbuff1
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
	end
	