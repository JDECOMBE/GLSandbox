#version 330 core

in vec2 TexCoords; 

uniform sampler2D tex;

out vec4 FragColor;

void main() {
    vec4 color = vec4(texture(tex, TexCoords.xy));
    FragColor = color;
}
