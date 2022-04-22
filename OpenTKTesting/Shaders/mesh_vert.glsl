#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texCoords;

uniform mat4 lightViewProjection;
uniform mat4 viewProjection;
uniform mat4 transform;
uniform float scale;


out vec3 FragPos;
out vec2 TexCoords;
out vec3 Normal;
out vec4 FragPosLightSpace;

void main()
{
    vec4 model = transform * vec4(position, 1);

    
    FragPos = model.xyz;
    TexCoords = texCoords;
    Normal = (transform * vec4(normal, 1)).xyz;
    FragPosLightSpace = lightViewProjection * model;
    gl_Position = viewProjection * model; 
} 