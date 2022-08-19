#version 430 core
#define EPSILON 0.001

struct Particle
{
    vec3 Position;
    vec3 Velocity;
};

layout(std430, binding = 0) restrict buffer ParticlesSSBO
{
    Particle particles[];
} ssbo;

uniform mat4 projViewMatrix;
uniform vec3 centerOfMass;
uniform float mass;

const float DRAG_COEF = log(0.998) * 176.0; // log(0.70303228048)

uniform float dts;

out InOutVars
{
    vec4 Color;
} outData;

void main()
{
    Particle particle = ssbo.particles[gl_VertexID];
    vec3 toMass = centerOfMass - particle.Position;
    float dist = max(length(toMass), 0.01);
    
    vec3 accel = 10.0 * (mass / dist) * (toMass / dist); 
    particle.Velocity *= mix(1.0, exp(DRAG_COEF * dts), 1.0);
    particle.Position += (dts * particle.Velocity + 0.5 * accel * dts * dts);
    particle.Velocity += accel * dts;

    ssbo.particles[gl_VertexID] = particle;
    
    vec3 vel = particle.Velocity * 3;
    float red = 0.0045 * dot(vel, vel);
    float green = clamp(0.08 * max(vel.x, max(vel.y, vel.z)), 0.2, 0.5);
    float blue = 0.7 - red;

    outData.Color = vec4(red, green, blue, 0.25);
    gl_Position = projViewMatrix * vec4(particle.Position, 1.0);
}