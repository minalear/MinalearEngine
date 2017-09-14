#version 420
in vec3 Position;
in vec2 UV;
in vec3 Normal;
in vec4 FragPosLightSpace;

out vec4 fragmentColor;

uniform vec3 cameraPosition;
uniform vec3 lightDirection;

uniform sampler2D texture0; //Diffuse
uniform sampler2D texture1; //Normal
uniform sampler2D texture2; //Specular
uniform sampler2D shadowMap; //Shadow Map

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
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
	vec3 color = texture(texture0, UV).rgb;
	vec3 normal = normalize(Normal);
	vec3 lightColor = vec3(1.0);

	//Ambient
	vec3 ambient = 0.15 * color;
	
	//Diffuse
	vec3 lightDir = normalize(-lightDirection);
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 diffuse = diff * lightColor;

	//Specular
	vec3 viewDir = normalize(cameraPosition - Position);
	float spec = 0.0;
	vec3 halfwayDir = normalize(lightDir + viewDir);
	//spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
	vec3 specular = spec * lightColor;

	//Calculate Shadow
	float shadow = ShadowCalculation(FragPosLightSpace, normal, lightDir);
	vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;

	fragmentColor = vec4(lighting, 1);
}
