shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float shockwave_strength : hint_range(0.0, 1.0) = 0.0;
uniform float shockwave_radius : hint_range(0.0, 1.0) = 0.5;
uniform float shockwave_speed : hint_range(0.1, 5.0) = 2.0;
uniform vec2 shockwave_center = vec2(0.5, 0.5);
uniform float time : hint_range(0.0, 100.0) = 0.0;
uniform float chromatic_amount : hint_range(0.0, 0.1) = 0.0;

void fragment() {
    vec2 uv = SCREEN_UV;
    vec2 center = shockwave_center;
    
    // Calculate distance from center
    float dist = distance(uv, center);
    
    // Create shockwave ring effect
    float ring_width = 0.1;
    float ring = smoothstep(shockwave_radius - ring_width, shockwave_radius, dist) - 
                smoothstep(shockwave_radius, shockwave_radius + ring_width, dist);
    
    // Calculate distortion direction (outward from center)
    vec2 dir = normalize(uv - center);
    
    // Apply shockwave distortion
    float distortion = ring * shockwave_strength * sin(dist * 20.0 - time * shockwave_speed);
    vec2 distorted_uv = uv - dir * distortion * 0.1;
    
    // Chromatic aberration
    vec3 color;
    if (chromatic_amount > 0.0) {
        vec2 r_offset = dir * chromatic_amount * ring;
        vec2 b_offset = -dir * chromatic_amount * ring;
        
        color.r = texture(screen_texture, distorted_uv + r_offset).r;
        color.g = texture(screen_texture, distorted_uv).g;
        color.b = texture(screen_texture, distorted_uv + b_offset).b;
    } else {
        color = texture(screen_texture, distorted_uv).rgb;
    }
    
    // Add ripple highlight at ring edge
    float highlight = ring * shockwave_strength * 0.5;
    color += vec3(highlight);
    
    COLOR = vec4(color, 1.0);
}
