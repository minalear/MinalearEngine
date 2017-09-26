#version 420
in vec2 UV;

out vec4 fragmentColor;

uniform vec3 lightPosition;
uniform vec3 lightColor;
uniform vec3 cameraPosition;
uniform float farPlane;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;
uniform samplerCube shadowMap;

float ShadowCalculation(vec3 fragPosition, vec3 normal)
{
	vec3 fragToLight = fragPosition - lightPosition;
	float closestDepth = texture(shadowMap, fragToLight).r;
	closestDepth *= farPlane;
	float currentDepth = length(fragToLight);

	float bias = max(0.00025 * (1.0 - dot(normal, normalize(fragToLight))), 0.00025);

	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
	return shadow;
}

void main()
{
	vec4 positionSample = texture(gPosition, UV);
	vec4 normalSample = texture(gNormal, UV);
	vec4 albedoSample = texture(gAlbedoSpec, UV);

	vec3 position = positionSample.rgb;
	vec3 normal = normalSample.rgb;
	vec3 albedo = albedoSample.rgb;
	float specValue = albedoSample.a;

	//Ambient
	vec3 ambient = lightColor * 0.1;

	//Diffuse
	vec3 lightDir = normalize(lightPosition - position);
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 diffuse = diff * lightColor;

	//Specular
	vec3 viewDir = normalize(cameraPosition - position);
	vec3 halfwayDir = normalize(lightDir + viewDir);
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
	vec3 specular = spec * lightColor * specValue;

	//Shadow
	//float shadow = ShadowCalculation(position, normal, lightDir);

	vec3 lighting = (ambient + diffuse + specular) * albedo;
	fragmentColor = vec4(vec3(specValue), 1.0);
}
