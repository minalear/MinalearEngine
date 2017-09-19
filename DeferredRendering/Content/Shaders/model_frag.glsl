#version 420
in VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentLightPos;
	vec3 TangentViewPos;
	vec3 TangentFragPos;
} fs_in;

out vec4 fragmentColor;

uniform float ambientMod;
uniform vec3 lightColor;
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
	vec3 texColor = texture(diffuseMap, fs_in.TexCoords).rgb;
	vec3 normalColor = texture(normalMap, fs_in.TexCoords).rgb;
	vec3 specColor = texture(specularMap, fs_in.TexCoords).rgb;

	vec3 normal = texture(normalMap, fs_in.TexCoords).rgb;
	normal = normalize(normal * 2.0 - 1.0);

	//Ambient
	vec3 ambient = vec3(ambientMod);

	//Diffuse
	vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 diffuse = diff * lightColor;

	//Specular
	vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
	vec3 halfwayDir = normalize(lightDir + viewDir);
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
	vec3 specular = spec * lightColor * specColor;

	//Shadow
	float shadow = ShadowCalculation(fs_in.FragPos, normal, lightDir);

	vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * texColor;

	if (ambient == 0.0)
		fragmentColor = vec4(lighting, 1.0 - shadow);
	else
		fragmentColor = vec4(lighting, 1.0);
}
