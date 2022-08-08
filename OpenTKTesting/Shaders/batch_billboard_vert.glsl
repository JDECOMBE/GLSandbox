#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in float sampler;
layout (location = 2) in vec2 uv;

layout (location = 3) in vec3 offset;
layout (location = 4) in vec3 color;
layout (location = 5) in float scale;
layout (location = 6) in float random;
layout (location = 7) in float ditheringFactor;

uniform mat4 viewProjection;
uniform vec3 viewPosition;

out vec2 UV;
out vec3 Color;


void main()
{
    UV = uv;
    Color = color;
    vec3 positionScaled = position * scale * vec3(0.27);
    vec3 model = positionScaled + offset;
    gl_Position = viewProjection * vec4(model, 1);
}