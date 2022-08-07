#version 330 core

in vec3 Color;
in float DitheringFactor;

uniform vec2 windowSize;

out vec4 FragColor;

float dither(float ditherFactor, vec2 st)
{
    float DITHER_THRESHOLDS[16] = float[16]
    (
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    );
    int index = (int(st.x) % 4) * 4 + int(st.y) % 4;
    return DITHER_THRESHOLDS[index] * ditherFactor;
}

void main()
{
    vec2 st = gl_FragCoord.xy * windowSize;
    float d = dither(DitheringFactor, st);
    if (d > .1)
        discard;
    FragColor = vec4(Color, 1);
}