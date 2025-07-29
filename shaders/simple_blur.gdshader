/*
    Simple Blur Shader

    This shader applies a basic box blur effect to a canvas item in Godot.

    Uniforms:
        - blur_amount (float, default: 1.5, range: 0.0 to 5.0)
            Controls the intensity of the blur. Higher values result in a stronger blur effect.

    Implementation Details:
        - The shader samples the surrounding pixels in a 5x5 grid centered on the current pixel.
        - Each sample is offset by blur_amount, scaled by the texture size to ensure consistent blur regardless of resolution.
        - The color of each sample is accumulated and then averaged by dividing by 25 (the total number of samples).
        - The final blurred color is output to the fragment.

    Usage:
        - Attach this shader to a CanvasItem (e.g., Sprite, TextureRect).
        - Adjust the blur_amount uniform to control the blur strength.
*/
shader_type canvas_item;

uniform float blur_amount : hint_range(0.0, 5.0) = 1.5;

void fragment() {
    vec2 texture_size = vec2(textureSize(TEXTURE, 0));
    vec2 blur_size = blur_amount / texture_size;
    vec4 color = vec4(0.0);

    for (int x = -2; x <= 2; x++) {
        for (int y = -2; y <= 2; y++) {
            color += texture(TEXTURE, UV + vec2(float(x), float(y)) * blur_size);
        }
    }
    
    COLOR = color / 25.0;
}
