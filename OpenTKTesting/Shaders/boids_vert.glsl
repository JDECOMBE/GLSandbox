#version 430 core

struct Boid
{
    vec3 position;
    vec3 color;
    vec3 velocity;
    vec3 acceleration;
};

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normals;
layout (std430, binding = 0) restrict buffer ParticlesSSBO
{
    Boid boids[];
};

uniform mat4 viewProjection;
uniform float dts;
uniform bool pause;

out vec3 Color;
out float DitheringFactor;


mat3 crossProdMatrix(vec3 u) {
    
    return mat3(0, -u.z, u.y,
                u.z, 0, -u.x,
                -u.y, u.x, 0);
}
mat3 rotateAlongAxis(vec3 u, float teta) {
    float cosTeta = cos(teta);
    float sinTeta = sin(teta);
    
    return cosTeta * mat3(1.0) + sinTeta * crossProdMatrix(u) + (1 - cosTeta) * outerProduct(u, u);
}

void main()
{
    Boid boid = boids[gl_InstanceID];
    
    vec3 fixedVelocity = boid.velocity * dts;
    if (!pause)
        boid.position = boid.position + fixedVelocity;

    vec3 axis = cross(vec3(1, 0, 0), normalize(fixedVelocity));
    float angle = acos(dot(vec3(1, 0, 0), boid.velocity));
    mat3 rotationMatrix = rotateAlongAxis(axis, angle);
    
    if (boid.position.x > 25.0) 
        boid.position.x = -25.0;
    else if (boid.position.x < -25.0)
        boid.position.x = 25.0;
    
    if (boid.position.y > 25.0)
        boid.position.y = -25.0;
    else if (boid.position.y < -25.0)
        boid.position.y = 25.0;
    
    if (boid.position.z > 25.0)
        boid.position.z = -25.0;
    else if (boid.position.z < -25.0)
        boid.position.z = 25.0;
    
    
    boid.velocity = boid.velocity + boid.acceleration;
    
    boid.acceleration = vec3(0);
    boids[gl_InstanceID] = boid;
    
    vec3 model = (position * rotationMatrix) + boid.position;

    Color = boid.color * 0.6 + dot(normals * rotationMatrix, vec3(0.2, 1.0, 0.0)) * 0.4;
    gl_Position = viewProjection * vec4(model, 1.0);
}