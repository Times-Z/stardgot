shader_type canvas_item;

uniform float blur_size : hint_range(0.0, 20.0) = 8.0;

void fragment() {
    vec2 size = TEXTURE_PIXEL_SIZE * blur_size;
    vec4 color = vec4(0.0);
    float total = 0.0;
    for (float x = -4.0; x <= 4.0; x++) {
        for (float y = -4.0; y <= 4.0; y++) {
            float weight = 1.0 - length(vec2(x, y)) / 6.0;
            color += texture(TEXTURE, UV + vec2(x, y) * size) * weight;
            total += weight;
        }
    }
    COLOR = color / total;
}
