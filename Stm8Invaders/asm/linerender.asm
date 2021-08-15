stm8/
	.tab	0,8,16,60

	#include "characterrom.inc"
	#include "variables.inc"
	#include "constants.inc"
	segment 'ram0'
store.b ds.w
romhi.b ds.b
linemasked.b ds.b
count.b ds.b
temp ds.w
	segment 'rom'
;
; On entry X is where to store the data.
;
.renderline.w
	ldw store,x
	; make x the start of the line
	clrw x
	ld a,{linenumber+1}
	and a,#$f8
	ld xl,a
	sllw x
	sllw x
	
	ld a,{linenumber+1}
	and a,#7
	ld linemasked,a
	add a,#{high charrom}
	ld romhi,a
	
	mov count,#scr_width
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
	ld a,#$00		;Add an elephant
	ld {renderbuff1+$00},a
	ld {renderbuff2+$00},a
	ld {renderbuff1+scr_width+1},a
	ld {renderbuff2+scr_width+1},a	
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
	ld a,#$00		;Add an elephant
	ld {renderbuff1+$00},a
	ld {renderbuff2+$00},a
	ld {renderbuff1+scr_width+1},a
	ld {renderbuff2+scr_width+1},a	
	ret
	end
	