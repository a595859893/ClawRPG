shader_type canvas_item;

// 基础发光参数
uniform float glow_amount : hint_range(0.0, 1.0) = 0.0;
uniform vec4 glow_color : source_color = vec4(0.3, 0.6, 1.0, 1.0);

// 脉冲效果参数
uniform bool enable_pulse = true;
uniform float pulse_speed : hint_range(0.1, 5.0) = 2.0;
uniform float pulse_min : hint_range(0.0, 1.0) = 0.3;
uniform float pulse_max : hint_range(0.0, 1.0) = 0.8;

// 边缘发光参数
uniform bool enable_edge_glow = true;
uniform float edge_threshold : hint_range(0.0, 1.0) = 0.3;
uniform float edge_intensity : hint_range(0.0, 2.0) = 1.0;

// 扭曲效果参数
uniform bool enable_wobble = false;
uniform float wobble_amount : hint_range(0.0, 0.1) = 0.01;
uniform float wobble_speed : hint_range(0.1, 10.0) = 5.0;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    
    // 应用扭曲效果
    vec2 uv_offset = UV;
    if (enable_wobble) {
        float wobble = sin(TIME * wobble_speed + UV.x * 10.0) * wobble_amount;
        uv_offset += vec2(wobble, wobble * 0.5);
        tex_color = texture(TEXTURE, uv_offset);
    }
    
    // 计算边缘发光
    float edge_glow = 0.0;
    if (enable_edge_glow) {
        float alpha = tex_color.a;
        // 使用梯度近似边缘
        float left = texture(TEXTURE, uv_offset + vec2(-0.01, 0.0)).a;
        float right = texture(TEXTURE, uv_offset + vec2(0.01, 0.0)).a;
        float up = texture(TEXTURE, uv_offset + vec2(0.0, -0.01)).a;
        float down = texture(TEXTURE, uv_offset + vec2(0.0, 0.01)).a;
        
        float edge = abs(left - right) + abs(up - down);
        edge_glow = edge * edge_intensity;
    }
    
    // 计算脉冲
    float pulse = glow_amount;
    if (enable_pulse) {
        float pulse_value = (sin(TIME * pulse_speed) + 1.0) * 0.5;
        pulse = mix(pulse_min, pulse_max, pulse_value) * glow_amount;
    }
    
    // 混合发光颜色
    vec3 final_color = mix(tex_color.rgb, glow_color.rgb, pulse);
    
    // 添加边缘发光
    final_color += glow_color.rgb * edge_glow * pulse;
    
    // 应用原始alpha并添加发光alpha
    float final_alpha = tex_color.a + (pulse * glow_color.a * 0.5) + (edge_glow * pulse);
    
    COLOR = vec4(final_color, clamp(final_alpha, 0.0, 1.0));
}
