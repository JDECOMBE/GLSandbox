#version 330 core

in vec2 UV;
in vec3 Color;

uniform sampler2D texture1;

out vec4 FragColor;

void main()
{
    vec4 color = texture(texture1, UV);
    if (color.a < 0.9)
        discard;
    FragColor = color * vec4(Color, 1.0);
}