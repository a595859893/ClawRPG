shader_type canvas_item;

uniform float rage_amount : hint_range(0.0, 1.0) = 0.0;
uniform vec4 rage_color : source_color = vec4(1.0, 0.3, 0.0, 1.0);
uniform float pulse_speed : hint_range(0.5, 5.0) = 2.0;
uniform float distortion_amount : hint_range(0.0, 0.1) = 0.02;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
    
    // Pulsing glow effect based on rage_amount
    float pulse = sin(TIME * pulse_speed) * 0.5 + 0.5;
    float glow_intensity = rage_amount * pulse;
    
    // Distortion effect for intense rage
    vec2 distorted_uv = UV;
    if (rage_amount > 0.5) {
        float dist = (rage_amount - 0.5) * distortion_amount;
        distorted_uv.x += sin(UV.y * 10.0 + TIME * 5.0) * dist;
        distorted_uv.y += cos(UV.x * 10.0 + TIME * 5.0) * dist;
    }
    
    // Mix original color with rage color
    vec4 final_color = texture(TEXTURE, distorted_uv);
    final_color.rgb = mix(final_color.rgb, rage_color.rgb, glow_intensity * 0.6);
    
    // Add rim glow
    float edge = 1.0 - min(min(UV.x, 1.0 - UV.x), min(UV.y, 1.0 - UV.y));
    edge = smoothstep(0.0, 0.3, edge);
    final_color.rgb += rage_color.rgb * edge * glow_intensity * 0.5;
    
    COLOR = final_color;
}
