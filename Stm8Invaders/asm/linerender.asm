stm8/
	.tab	0,8,16,60

	#include "characterrom.inc"
	#include "variables.inc"
	#include "constants.inc"
	#include "sprite.inc"
	segment 'ram0'
store.b		ds.w
store_copy.b	ds.w
romhi.b		ds.b
linemasked.b	ds.b
count.b		ds.b
	segment 'rom'
;
; On entry X is where to store the data.
;
.renderline.w
	ldw	store,x
	ldw	store_copy,x
	; make x the start of the line
	clrw	x
	ld	a,{linenumber+1}
	and	a,#$f8
	ld	xl,a
	sllw	x
	sllw	x
	
	ld	a,{linenumber+1}
	and	a,#7
	ld	linemasked,a
	add	a,#{high charrom}
	ld	romhi,a
	
	mov	count,#scr_width
; We have a theoretical 1024 cycles to render the screen
; DMA will slice some of that away.
; To render 32 cells takes 18*32 cycles (576)
; For the sprites we have a max of 9 sprites with 4 visible
; at most (three alien bullets or explosions + player)
; cycles = 9*9 + 36 * 4 = 225
; Total cycle count for render loop is ~800 cycles
renderloop:
	ld	a,(screen,x)	;1
	incw	x		;1
	bcp	a,#$e0		;1
	jrne	rendercharacter ;1
renderudg
	clrw	y	  	;1
	sll	a		;1
	sll	a		;1
	sll	a		;1
	add	a,linemasked	;1 
	ld	yl,a		;1
	ld	a,(udg,y)	;1
	ldw	y,store		;2 
	ld	(y),a		;1
	inc	{store+1}	;1
	dec	count		;1
	jrne	renderloop	;1 =14
	jra	rendersprites
rendercharacter
	ld	yl,a		;1
	ld	a,romhi		;1
	ld	yh,a		;1
	ld	a,(y)         	;1
 	ldw	y,store       	;2 
	ld	(y),a		;1
	inc	{store+1}	;1
	dec	count		;1
	jrne	renderloop	;2 =11
rendersprites
	ldw	x,#sprites_start	;1
rendersprite_loop
	ld	a,(x)			;1
	jreq	next_sprite		;1/2
	ld	a,(sprite_x_offs,x)	;1
	sub	a,{linenumber+1}	;1
	jrult	rendersprite_loop	;1/2
	cp	a,(sprite_width_offs,x) ;1
	jruge	rendersprite_loop	;1/2
	pushw	x			;2 (=12) Save how far we are
	; make x=our src data
	sll	a			;1
	ld	{store+1},a		;1
	mov	store,#0		;1
	ld	a,(sprite_y_offs,X)	;1 save for later
	ldw	x,(sprite_data_cur_img_offs,x) ;2
	addw	x,store			;2 (=8)
	;make y where we're storing
	srl	a			;1
	srl	a			;1
	srl	a			;1
	ldw	y,store_copy		;2
	add	a,{store_copy+1}	;1
	ld	yl,a			;1
	ld	a,(y)			;1
	ld	(x),a			;1
	incw	y			;1
	incw	x			;1
	ld	yl,a			;1
	ld	a,(y)			;1
	ld	(x),a			;1
	popw	x			;2 (=16 Restore our sprite ptr
next_sprite
	addw	x,#sprite_size		;2
	cpw	x,#sprites_end		;2
	jrult	rendersprite_loop	;2
;All done add some elephants	
	ld	a,#$00		
	ld	{renderbuff1+$00},a
	ld	{renderbuff2+$00},a
	ld	{renderbuff1+scr_width+1},a
	ld	{renderbuff2+scr_width+1},a	
	ret
	end
	