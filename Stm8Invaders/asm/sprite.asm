stm8/
	.tab	0,8,16,60

	#include "variables.inc"
	#include "constants.inc"
	#include "sprite.inc"
	#include "spritedata.inc"
	segment 'ram0'
sprite_base	ds.w	1
sprite_mod7	ds.b	1
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
; Enter with x=sprite address
; y=sprite rom address
sprite_setup
	ldw	(sprite_data_offs,x),y
	ld	a,(y)
	ld	(sprite_width_offs,x),a
	ld	a,#0
	ld	(sprite_x_offs,x),a
	ld	(sprite_y_offs,x),a
	ld	(sprite_visible,x),a
; Fall through into setting the image
;
; sprite_set_image
;
; Enter with x=sprite address
; Sets the current image for the 
; image number and y offset modulo 8 
.sprite_set_image.w
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
	ret
.sprite_collided.w
.sprite_battle_damage.w	
	ret
	END
	