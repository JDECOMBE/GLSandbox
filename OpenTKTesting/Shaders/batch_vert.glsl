#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normals;

layout (location = 2) in vec3 offset;
layout (location = 3) in vec3 color;
layout (location = 4) in float scale;
layout (location = 5) in float random;

uniform mat4 viewProjection;
uniform vec3 viewPosition;

out vec3 Color;
out float DitheringFactor;

void main()
{
    float d = (distance(offset, viewPosition) / 1f) * 0.9 + random * 0.1;
    if (d > 1) {
        gl_Position = vec4(1, 1, 1, 0); // better find another way to discard geometry.
        return;
    }
    
    DitheringFactor = smoothstep(0.9, 1, d) / 2;
    
    
    vec3 positionScaled = position * scale;
    vec3 model = positionScaled + offset;
    
    Color = color * 0.6 + dot(normals, vec3(0.2, 1.0, 0.0)) * 0.4;

    gl_Position = viewProjection * vec4(model, 1);
}