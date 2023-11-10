#version 330 core

layout (location = 0) in vec3 position;

uniform mat4 viewProjection;
uniform vec3 offset;
uniform float scale;

out vec3 Color;

void main()
{
    vec3 positionScaled = position * scale;
    vec3 model = positionScaled + offset;

    gl_Position = viewProjection * vec4(model, 1);
}
