shader_type canvas_item;

// 攻击拖尾效果参数
uniform vec4 trail_color : source_color = vec4(1.0, 0.8, 0.3, 0.6);
uniform float trail_width : hint_range(1.0, 50.0) = 20.0;
uniform float fade_speed : hint_range(0.1, 10.0) = 3.0;
uniform float pulse_intensity : hint_range(0.0, 1.0) = 0.3;

// 动画参数
uniform float time_offset : hint_range(0.0, 100.0) = 0.0;
uniform bool enable_pulse = true;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    
    // 基础颜色
    vec4 base_color = tex_color;
    
    // 边缘发光效果
    float edge_distance = min(min(UV.x, 1.0 - UV.x), min(UV.y, 1.0 - UV.y));
    float edge_glow = 1.0 - smoothstep(0.0, 0.2, edge_distance);
    
    // 脉动效果
    float pulse = 1.0;
    if (enable_pulse) {
        pulse = 1.0 + pulse_intensity * sin(TIME * fade_speed + time_offset);
    }
    
    // 边缘高亮
    vec4 glow_color = trail_color * pulse;
    
    // 混合边缘发光
    vec4 final_color = mix(base_color, glow_color, edge_glow * trail_color.a);
    
    COLOR = final_color;
}

void vertex() {
    // 可以添加顶点动画用于拖尾摇摆效果
}
