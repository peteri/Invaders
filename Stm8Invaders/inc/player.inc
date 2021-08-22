score_offs			EQU 0
ships_rem_offs			EQU {score_offs+2}
aliens_offs			EQU {ships_rem_offs+1}
number_of_aliens		EQU 55
ref_alien_delta_x_offs		EQU {aliens_offs+number_of_aliens}
ref_alien_y_offs		EQU {ref_alien_delta_x_offs+1}
ref_alien_x_offs		EQU {ref_alien_y_offs+1}
rack_count_offs			EQU {ref_alien_x_offs+1}
num_aliens_offs			EQU {rack_count_offs+1}
extra_ship_available_offs	EQU {num_aliens_offs+1}
player_end_offs 		EQU {extra_ship_available_offs+1} 
shield_size			EQU {$1c mult 8}
	EXTERN	reset_player.w
	EXTERN	reset_shields.w
	EXTERN	count_aliens.w
	EXTERN	init_aliens.w
	EXTERN	swap_shields.w
	EXTERN	draw_shields.w
	EXTERN	find_column.w
	EXTERN	remove_ship.w
