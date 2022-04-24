#version 330 core

in vec2 TexCoords; 

uniform sampler2D tex;
uniform sampler2D alphaChannel;

out vec4 FragColor;

void main() {
    vec4 color = vec4(texture(tex, TexCoords.xy));
    color.a *= texture(alphaChannel, TexCoords.xy).x;
    FragColor = color;
}
