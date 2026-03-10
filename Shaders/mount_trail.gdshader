shader_type canvas_item;

// Mount trail effect uniforms
uniform float trail_intensity : hint_range(0.0, 1.0) = 0.5;
uniform vec4 trail_color : source_color = vec4(0.3, 0.7, 1.0, 0.8);
uniform float pulse_speed : hint_range(0.1, 5.0) = 2.0;
uniform float distortion_amount : hint_range(0.0, 0.1) = 0.02;
uniform float glow_radius : hint_range(0.0, 0.5) = 0.2;

// Animation
uniform float time_offset : hint_range(0.0, 100.0) = 0.0;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
    
    // Calculate pulse effect
    float pulse = sin(TIME * pulse_speed + time_offset) * 0.5 + 0.5;
    
    // Add glow effect around the sprite
    float dist = distance(UV, vec2(0.5));
    float glow = smoothstep(glow_radius + pulse * 0.1, glow_radius, dist);
    
    // Apply trail color with glow
    vec4 glow_color = trail_color;
    glow_color.a *= trail_intensity * (1.0 - dist * 2.0) * pulse;
    
    // Mix original color with trail effect
    color.rgb = mix(color.rgb, trail_color.rgb, glow_color.a * 0.3);
    color.a = max(color.a, glow_color.a * 0.5);
    
    // Add subtle distortion
    vec2 distorted_uv = UV;
    distorted_uv.x += sin(UV.y * 10.0 + TIME * 2.0) * distortion_amount;
    distorted_uv.y += cos(UV.x * 10.0 + TIME * 2.0) * distortion_amount;
    
    COLOR = color;
}
