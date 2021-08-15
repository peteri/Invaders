sprite_visible		EQU 0
sprite_data_offs	EQU	{sprite_visible+1}
sprite_data_cur_img_offs	EQU	{sprite_data_offs+2}
sprite_width_offs	EQU	{sprite_data_cur_img_offs+1}
sprite_image_offs	EQU	{sprite_width_offs+1}
sprite_x_offs		EQU	{sprite_image_offs+1}
sprite_y_offs		EQU	{sprite_x_offs+1}
sprite_size		EQU	{sprite_y_offs+1}
	EXTERN	sprite_init.w
	EXTERN	sprite_set_image.w
	EXTERN	sprite_draw.w
	EXTERN	sprite_collided.w
	EXTERN	sprite_battle_damage.w
