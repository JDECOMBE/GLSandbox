#version 330 core

in vec3 Color;
in float DitheringFactor;

uniform vec2 windowSize;

out vec4 FragColor;


void main()
{
    int dither[16] = int[16](
        2, 12, 8, 15,
        9, 1, 13, 6,
        5, 10, 4, 14,
        16, 7, 11, 3
    );
    vec2 st = gl_FragCoord.xy * vec2(126);
    int ditheringIndex = int(st.x) % 4 + 4 * (int(st.y) % 4);
    float threshold = float(dither[ditheringIndex]) / 16.0;
    if (threshold < DitheringFactor)
        discard;
    FragColor = vec4(Color, 1);
}