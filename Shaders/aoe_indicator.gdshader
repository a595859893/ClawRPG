shader_type canvas_item;

uniform vec4 indicator_color : source_color = vec4(0.2, 0.8, 1.0, 0.5);
uniform float fill_amount : hint_range(0.0, 1.0) = 0.3;
uniform float border_width : hint_range(0.0, 0.5) = 0.05;
uniform bool use_pulse = true;
uniform float pulse_speed : hint_range(0.5, 5.0) = 2.0;
uniform bool is_enemy = false;

void fragment() {
	vec2 uv = UV;
	vec2 center = vec2(0.5, 0.5);
	float dist = distance(uv, center);
	
	// Create circular indicator
	float radius = 0.5;
	float inner_radius = radius - border_width;
	
	// Pulse effect
	float pulse = 1.0;
	if (use_pulse) {
		pulse = 0.8 + 0.2 * sin(TIME * pulse_speed * 6.28318);
	}
	
	// Fill gradient from center
	float fill = smoothstep(radius, 0.0, dist) * fill_amount * pulse;
	
	// Border ring
	float border = smoothstep(inner_radius - 0.01, inner_radius, dist) - smoothstep(radius - 0.01, radius, dist);
	
	// Combine
	float alpha = max(fill, border * pulse);
	
	// Color: blue for player skills, red for enemy abilities
	vec4 color = indicator_color;
	if (is_enemy) {
		color = vec4(1.0, 0.2, 0.2, 0.6);
	}
	
	COLOR = vec4(color.rgb, alpha * color.a);
}
