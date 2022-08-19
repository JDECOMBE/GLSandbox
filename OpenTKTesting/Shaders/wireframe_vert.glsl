#version 330 core

layout (location = 0) in vec3 position;

uniform mat4 viewProjection;
uniform vec3 scale;
uniform vec3 offset;
uniform vec4 color;

out vec4 Color;

void main()
{
    vec3 positionScaled = position * scale;
    vec3 model = positionScaled + offset;
    
    Color = color;
    gl_Position = viewProjection * vec4(model, 1);
}