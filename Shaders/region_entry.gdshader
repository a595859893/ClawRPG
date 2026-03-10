shader_type canvas_item;

// Region entry effect uniforms
uniform float effect_intensity : hint_range(0.0, 1.0) = 0.0;
uniform vec4 effect_color : source_color = vec4(0.2, 0.8, 1.0, 0.6);
uniform float distortion_strength : hint_range(0.0, 0.1) = 0.02;
uniform float vignette_radius : hint_range(0.0, 1.0) = 0.7;
uniform float scanline_count : hint_range(0.0, 100.0) = 50.0;
uniform bool enable_scanlines = false;
uniform bool enable_vignette = true;
uniform bool enable_chromatic = true;

// Animation
uniform float time_offset : hint_range(0.0, 100.0) = 0.0;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
    
    // Apply effect intensity
    float intensity = effect_intensity;
    
    if (intensity > 0.0) {
        // Chromatic aberration effect
        if (enable_chromatic) {
            float aberration = distortion_strength * intensity;
            vec4 color_r = texture(TEXTURE, UV + vec2(aberration, 0.0));
            vec4 color_b = texture(TEXTURE, UV - vec2(aberration, 0.0));
            color.r = mix(color.r, color_r.r, intensity * 0.5);
            color.b = mix(color.b, color_b.b, intensity * 0.5);
        }
        
        // Vignette effect
        if (enable_vignette) {
            vec2 center = vec2(0.5, 0.5);
            float dist = distance(UV, center);
            float vignette = smoothstep(vignette_radius, vignette_radius - 0.3, dist);
            color.rgb = mix(color.rgb * (1.0 - intensity * 0.3), color.rgb, vignette);
        }
        
        // Scanlines effect
        if (enable_scanlines) {
            float scanline = sin((UV.y + time_offset * 0.1) * scanline_count * 3.14159) * 0.5 + 0.5;
            color.rgb = mix(color.rgb, color.rgb * (0.9 + scanline * 0.1), intensity * 0.3);
        }
        
        // Edge glow
        float edge = smoothstep(0.4, 0.5, dist);
        vec4 glow_color = effect_color;
        glow_color.a *= intensity * edge * 0.5;
        color = mix(color, glow_color, glow_color.a);
    }
    
    COLOR = color;
}
