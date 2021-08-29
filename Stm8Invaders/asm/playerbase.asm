stm8/
	.tab	0,8,16,60
	#include "variables.inc"
	#include "constants.inc"
	#include "playerbase.inc"
	#include "timerobject.inc"
	#include "main.inc"
Right	EQU	$01
None	EQU	$00
Left	EQU	$FF
	segment 'ram1'
demo_command ds.b 1
blow_up_counter ds.b 1
blow_up_changes ds.b 1
	segment 'rom'
demo_commands	dc.b	Right,Right,None,None,Right
		dc.b	None,Left,Right,None,Left
player_base_characters dc.b $56,$5c,$65,$6e,$77,$c0,$d0,$e0
.player_base_init.w
	mov	player_alive,#player_alive_alive
	mov	player_base_x,#$10
        mov	blow_up_counter,#5
        mov	blow_up_changes,#$0c
	ldw	x,#$80
	ldw	{player_base_timer+timer_tick_offs},x
	ldw	x,#player_base_action
	ldw	{player_base_timer+timer_action_offs},x
	ret
player_base_action
	ld	a,player_alive
	cp	a,#player_alive_alive
	jreq	not_blowing_up
	dec	blow_up_counter
	jreq	counter_zero
	ret
counter_zero
	bres	game_flags_1,#flag1_player_ok
	bres	game_flags_2,#flag2_alien_shot_enable
	mov	blow_up_counter,#5
	dec	blow_up_changes
	jrne	swap_blowup
	ld	a,#$23
	ld	yl,a
	call	draw_player_sprite
	call	player_base_init
	btjf	game_flags_1,#flag1_game_mode,blow_up_exit
	jp	player_ship_blown_up
blow_up_exit
	ret
swap_blowup
	clrw	x
	clrw	y
	ld	a,player_alive
	cp	a,#player_alive_blowup_one
	jrne	blowup_is_two
	incw	y
blowup_is_two	
	ld	a,player_base_x
	and	a,#$07
	ld	xl,a
	ld	a,#2
	cpw	x,#0
	jreq	size_is_two
	inc	a
size_is_two
	mov	player_alive,#player_alive_blowup_one
	cpw	y,#$0001
	jrne	blowup_is_two_swap
	sll	a
	mov	player_alive,#player_alive_blowup_two
blowup_is_two_swap
	add	a,(player_base_characters,x)
	ld	yl,a
	jp	draw_player_sprite
not_blowing_up
	bset	game_flags_1,#flag1_player_ok
	btjt	game_flags_2,#flag2_alien_shot_enable,aliens_can_fire
	dec	alien_fire_delay
	jrne	aliens_can_fire
	bset	game_flags_2,#flag2_alien_shot_enable
aliens_can_fire
	btjf	game_flags_1,#flag1_demo_mode,read_switches
	clrw	x
	ld	a,demo_command
	ld	xl,a
	ld	a,(demo_commands,x)
	jra	move_player_base
read_switches
	ld	a,#$00
	;
	;	TODO add code to read switches
	;
move_player_base
	add	a,player_base_x
	cp	a,#$10
	jrult	no_move
	cp	a,#$b9
	jrugt	no_move
	ld	player_base_x,a
no_move
	clrw	x
	and	a,#$07
	ld	xl,a
	ld	a,(player_base_characters,x)
	ld	yl,a
	jp	draw_player_sprite
;==========================================
; yl is the first character of our base
; if the character is #23 (blank) then
; it erases the next 3 characters.
; if the character code is less than $5c
; it draws two characters, otherwise three
; incrementing the character as it goes.
;==========================================
draw_player_sprite
	ld	a,player_base_x
	srl	a
	srl	a
	srl	a
	ld	xl,a
	ld	a,#scr_width
	mul	x,a
	ld	a,xl
	add	a,#$04
	ld	xl,a
	ld	a,yl
	ld	(screen,x),a
	addw	x,#scr_width
	cp	a,#$23
	jrne	not_erase
	ld	(screen,x),a
	addw	x,#scr_width
	ld	(screen,x),a
	ret
not_erase
	inc	a
	ld	(screen,x),a
	;first alien has a width of 2 characters
	;so go home first.
	cp	a,#$5c
	jrult	draw_player_exit
	addw	x,#scr_width
	inc	a
	ld	(screen,x),a
draw_player_exit
	ret
.increment_demo_command.w
	inc	demo_command
	ld	a,#10
	cp	a,demo_command
	jrugt	inc_demo_exit
	mov	demo_command,#0
inc_demo_exit	
	ret
	END
	