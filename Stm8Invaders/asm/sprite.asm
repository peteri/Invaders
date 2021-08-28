stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	#include "sprite.inc"
	#include "spritedata.inc"
	#include "characterrom.inc"
	segment 'ram0'
sprite_base	ds.w	1
sprite_mod7	ds.b	1
	segment 'ram1'
width_i		ds.b	1
xOffs	ds.b	1
screen_pos	ds.w	1
screen_temp	ds.b	1
	segment 'rom'
;=======================================
;
; Setup all the sprites
;
;=======================================
.sprite_init.w
	ldw	x,#sp_splash_alien
	ldw	y,#alien_moving_y_data
	call	sprite_setup
	ldw	x,#sp_player_shot
	ldw	y,#player_shot_data
	call	sprite_setup
	ldw	x,#sp_player_shot_exp
	ldw	y,#player_shot_exp_data
	call	sprite_setup
	ldw	x,#sp_alien_plunger_shot
	ldw	y,#alien_shot_plunger_data
	call	sprite_setup
	ldw	x,#sp_alien_rolling_shot
	ldw	y,#alien_shot_rolling_data
	call	sprite_setup
	ldw	x,#sp_alien_squigly_shot
	ldw	y,#alien_shot_squigly_data
	call	sprite_setup
	ldw	x,#sp_alien_plunger_exp
	ldw	y,#alien_shot_explode_data
	call	sprite_setup
	ldw	x,#sp_alien_rolling_exp
	ldw	y,#alien_shot_explode_data
	call	sprite_setup
	ldw	x,#sp_alien_squigly_exp
	ldw	y,#alien_shot_explode_data
	call	sprite_setup
	ret
;=======================================
; Enter with x=sprite address
; y=sprite rom address
;=======================================
sprite_setup
	ldw	(sprite_data_offs,x),y
	ld	a,(y)
	ld	(sprite_width_offs,x),a
	ld	a,#0
	ld	(sprite_x_offs,x),a
	ld	(sprite_y_offs,x),a
	ld	(sprite_visible,x),a
;=======================================
; Fall through into setting the image
;
; sprite_set_image
;
; Enter with x=sprite address
; Sets the current image for the 
; image number and y offset modulo 8 
; x & y are saved
;=======================================
.sprite_set_image.w
	pushw	y
	ldw	y,x
	ldw	y,(sprite_data_offs,y)
	incw	y	;add one for width in ROM.
	ldw	sprite_base,y
	; y = (image*8+(sprite_y &0x07))*width*2+sprite_base
	ld	a,(sprite_y_offs,x)
	and	a,#7
	ld	sprite_mod7,a
	ld	a,(sprite_image_offs,x)
	sll	a
	sll	a
	sll	a
	add	a,sprite_mod7
	clrw	y
	ld	yl,a
	ld	a,(sprite_width_offs,x)
	sll	a
	mul	y,a
	addw	y,sprite_base
	ldw	(sprite_data_cur_img_offs,x),y
	popw	y
	ret
;=======================================
; Enter with x=sprite address
; x & y are saved
; returns with non zero value in acc if collides
;=======================================
.sprite_collided.w
	pushw	x
	pushw	y
	ld	a,#0
	ld	width_i,a
	ldw	x,(sprite_data_cur_img_offs,x)
	ldw	sprite_base,x
	ldw	x,(3,sp)
sprite_collided_loop
	;x=sprite
	;a=width_i
	ld	sprite_mod7,a
	call	compute_screen_pos
	ldw	y,screen_pos
	ld	a,(screen,y)
	cp	a,#$20
	jruge	screen_rom_1
	sll	a	;udg?
	sll	a
	sll	a
	add	a,xOffs
	clrw	y
	ld	yl,a
	ld	a,(udg,y)
	jra	test_1_and_inc
screen_rom_1
	ld	yl,a
	ld	a,xOffs
	add	a,#{high charrom}
	ld	yh,a
	ld	a,(y)
test_1_and_inc
	ldw	x,sprite_base
	and	a,(x)
	jrne	sprite_collided_pop_x_y
	incw	x
	ldw	sprite_base,x
;Next cell up the screen	
	ldw	y,screen_pos
	ld	a,({screen+1},y)
	cp	a,#$20
	jruge	screen_rom_2
	sll	a	;udg?
	sll	a
	sll	a
	add	a,xOffs
	clrw	y
	ld	yl,a
	ld	a,(udg,y)
	jra	test_2_and_inc
screen_rom_2
	ld	yl,a
	ld	a,xOffs
	add	a,#{high charrom}
	ld	yh,a
	ld	a,(y)
test_2_and_inc
	ldw	x,sprite_base
	and	a,(x)
	jrne	sprite_collided_pop_x_y
	incw	x
	ldw	sprite_base,x
	; Hit the end yet?
	ldw	x,(3,sp)
	inc	width_i
	ld	a,width_i
	cp	a,(sprite_width_offs,x)
	jrult	sprite_collided_loop
	ld	a,#0
sprite_collided_pop_x_y
	popw	y
	popw	x
	ret
;=========================================
;
;	Compute the screen pos
;	destroys y and accumulator
;	popuplates screen_pos and xOffs
;	adds sprite_mod7 to the x position
;
;=========================================
compute_screen_pos
	ld	a,(sprite_x_offs,x)
	add	a,sprite_mod7
	and	a,#7
	ld	xOffs,a
	
	ld	a,(sprite_y_offs,x)
	clrw	y
	srl	a
	srl	a
	srl	a
	ld	yl,a
	ldw	screen_pos,y
	ld	a,(sprite_x_offs,x)
	add	a,sprite_mod7
	srl	a
	srl	a
	srl	a
	ld	yl,a
	ld	a,#scr_width
	mul	y,a
	addw	y,screen_pos
	ldw	screen_pos,y
	ret
;=======================================
; Enter with x=sprite address
; x & y are saved
;=======================================
.sprite_battle_damage.w	
	pushw	y
	pushw	x
	mov	sprite_mod7,#0
	call	compute_screen_pos
	ld	a,(sprite_width_offs,x)
	ld	width_i,a
	ldw	x,(sprite_data_cur_img_offs,x)
battle_damage_loop
	ldw	y,screen_pos
	ld	a,(screen,y)
	cp	a,#$20
	jruge	battle_next_char
	sll	a
	sll	a
	sll	a
	add	a,xOffs
	clrw	y
	ld	yl,a
	ld	a,(x)
	cpl	a
	and	a,(udg,y)
	ld	(udg,y),a
battle_next_char
	incw	x
	ldw	y,screen_pos
	ld	a,({screen+1},y)
	cp	a,#$20
	jruge	battle_inc_xoffs
	sll	a
	sll	a
	sll	a
	add	a,xOffs
	clrw	y
	ld	yl,a
	ld	a,(x)
	cpl	a
	and	a,(udg,y)
	ld	(udg,y),a
battle_inc_xoffs	
	incw	x
	inc	xOffs
	btjf	xOffs,#3,battle_next_line
	mov	xOffs,#0
	ldw	y,screen_pos
	addw	y,#scr_width
	ldw	screen_pos,y
battle_next_line
	dec	width_i
	jrne	battle_damage_loop
battle_damage_exit	
	popw	x
	popw	y
	ret
;=======================================
;
;	Hides the sprites
;
;=======================================
.sprite_hide_all.w
	ldw	x,#sprites_start
	ld	a,#0
sprite_hide_loop
	ld	(sprite_visible,x),a
	addw	x,#sprite_size
	cpw	x,#sprites_end
	jrne	sprite_hide_loop
	ret
	END
	