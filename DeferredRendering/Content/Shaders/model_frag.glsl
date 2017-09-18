#version 420
in vec3 Position;
in vec2 UV;
in vec3 Normal;
in vec3 Tangent;
in vec3 Bitangent;

out vec4 fragmentColor;

uniform vec3 lightPosition;
uniform vec3 cameraPosition;
uniform float farPlane;

uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
uniform sampler2D specularMap;
uniform samplerCube shadowMap;

float ShadowCalculation(vec3 fragPos, vec3 normal, vec3 lightDir)
{
	vec3 fragToLight = fragPos - lightPosition;
	float closestDepth = texture(shadowMap, fragToLight).r;
	closestDepth *= farPlane;
	float currentDepth = length(fragToLight);

	float bias = max(0.00025 * (1.0 - dot(normal, normalize(fragToLight))), 0.00025);

	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	return shadow;
}

void main()
{
	vec3 texColor = texture(diffuseMap, UV).rgb;
	vec3 normalColor = texture(normalMap, UV).rgb;
	vec3 specColor = texture(specularMap, UV).rgb;

	vec3 normal = normalize(TBN * normalize(normalColor * 2.0 - 1.0));

	vec3 lightColor = vec3(1.0);

	//Ambient
	vec3 ambient = 0.15 * lightColor;

	//Diffuse
	vec3 lightDir = normalize(lightPosition - Position);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;

	//Specular
	vec3 viewDir = normalize(cameraPosition - Position);
	float spec = 0.0;
	vec3 halfwayDir = normalize(lightDir + viewDir);
	spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
	vec3 specular = spec * lightColor * specColor;

	//Shadow
	float shadow = ShadowCalculation(Position, normal, lightDir);
	//vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * texColor;
	vec3 lighting = normal;

	fragmentColor = vec4(normal, 1.0);
}
