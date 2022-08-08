#version 330 core

in vec3 Color;
in float DitheringFactor;

uniform vec2 windowSize;

out vec4 FragColor;



void main()
{
    vec2 st = gl_FragCoord.xy * windowSize;
    int d = int(1 / DitheringFactor);
    if (int(st.x) % d == 0 && int(st.y) % d == 0)
        discard;
    FragColor = vec4(Color, 1);
}