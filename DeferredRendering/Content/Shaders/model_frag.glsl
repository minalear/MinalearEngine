#version 420
in vec3 Position;
in vec2 UV;
in vec3 Normal;
in vec4 FragPosLightSpace;

out vec4 fragmentColor;

uniform vec3 lightPosition;
uniform vec3 cameraPosition;

uniform sampler2D diffuseMap;
uniform sampler2D shadowMap;

float ShadowCalculation(vec4 fragPos, vec3 normal, vec3 lightDir)
{
	vec3 projCoords = fragPos.xyz / fragPos.w;
	projCoords = projCoords * 0.5 + 0.5;

	float closestDepth = texture(shadowMap, projCoords.xy).r;
	float currentDepth = projCoords.z;

	float bias = max(0.00025 * (1.0 - dot(normal, lightDir)), 0.00025);
	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	if (projCoords.z > 1.0)
		shadow = 0.0;

	return shadow;
}

void main()
{
	vec3 texColor = texture(diffuseMap, UV).rgb;
	vec3 normal = normalize(Normal);
	vec3 lightColor = vec3(1.0);

	//Ambient
	vec3 ambient = 0.15 * lightColor;

	//Diffuse
	vec3 lightDir = normalize(lightPosition - Position);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;

	//Shadow
	float shadow = ShadowCalculation(FragPosLightSpace, normal, lightDir);
	vec3 lighting = (ambient + (1.0 - shadow) * diffuse) * texColor;

	fragmentColor = vec4(lighting, 1.0);
}
