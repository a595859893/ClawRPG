shader_type canvas_item;

// Flash effect parameters
uniform float flash_amount : hint_range(0.0, 1.0) = 0.0;
uniform vec4 flash_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);

void fragment() {
    vec4 color = texture(TEXTURE, UV);
    // Mix the original color with flash color based on flash_amount
    color.rgb = mix(color.rgb, flash_color.rgb, flash_amount);
    COLOR = color;
}
