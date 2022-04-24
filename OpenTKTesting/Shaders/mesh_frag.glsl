#version 330 core

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;
in vec4 FragPosLightSpace;

uniform vec3 viewOrigin;

struct Material {
    sampler2D diffuse[10];
    sampler2D specular[10];
    vec3 fallbackDiffuse;
    vec3 fallbackSpecular;
    int diffuseCount;
    int specularCount;
    float shininess;
}; 
uniform int isLightSource;

uniform Material material;

struct Light {
    vec3 position;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform Light light;  

uniform sampler2D shadowMap;

out vec4 FragColor;


float ShadowCalculation(vec4 fragPosLightSpace)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    
    bvec2 bigger = greaterThan(projCoords.xy, vec2(1));
    bvec2 smaller = lessThan(projCoords.xy, vec2(0));
    if (any(bigger) || any(smaller) || length(vec2(0.5) - projCoords.xy) > 0.5)
        return 1.0;
    
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(shadowMap, projCoords.xy).x; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // check whether current frag pos is in shadow 
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -5; x <= 5; ++x)
    {
        for(int y = -5; y <= 5; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth > pcfDepth ? 1.0 : 0.0;        
        }    
    }
    shadow /= 121.0;

    return shadow;
}  

void main()
{
    // diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    vec3 ambient = vec3(0);
    if (material.diffuseCount == 0) {
        ambient = light.ambient * material.fallbackDiffuse;
    }
    else {
        for (int i = 0; i < material.diffuseCount; i++) {
            ambient += light.ambient * texture(material.diffuse[i], TexCoords).xyz;
        }
        ambient = ambient / float(material.diffuseCount);
    }
    
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = vec3(0);
    if (material.diffuseCount == 0) {
        diffuse = light.diffuse * diff * material.fallbackDiffuse;
    }
    else {
        for (int i = 0; i < material.diffuseCount; i++) {
            diffuse += light.diffuse * (diff * texture(material.diffuse[i], TexCoords).xyz);
        }
        diffuse = diffuse / float(material.diffuseCount);
    }
    // specular
    vec3 viewDir = normalize(viewOrigin - FragPos);
    
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(norm, halfwayDir), 0.0), max(2, 32 * material.shininess));

    vec3 specular = vec3(0);
    if (material.specularCount == 0) {
        specular = light.specular * spec * material.fallbackSpecular;
    }
    else {
        for (int i = 0; i < material.specularCount; i++) {
            specular += light.specular * (spec * texture(material.specular[i], TexCoords).xyz);  
        }
        specular = specular / float(material.specularCount);
    }
    
    float revertShadow = 0.0;        
    if (isLightSource == 0.0)
    {
        float shadow = ShadowCalculation(FragPosLightSpace);   
        revertShadow = (1.0 - shadow);    
    }    
    vec3 result = ((revertShadow + 0.4) * ambient + revertShadow *  (diffuse + specular));
    FragColor = vec4(result, 1.0);
}