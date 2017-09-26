#version 420
in VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 Normal;
} fs_in;

out vec4 fragmentColor;

uniform float farPlane;
uniform vec3 lightPosition;
uniform vec3 lightColor;
uniform vec3 cameraPosition;

uniform sampler2D diffuseMap;
uniform sampler2D specularMap;
uniform samplerCube shadowMap;

float ShadowCalculation(vec3 normal, vec3 lightDir)
{
	vec3 fragToLight = fs_in.FragPos - lightPosition;
	float closestDepth = texture(shadowMap, fragToLight).r;
	closestDepth *= farPlane;
	float currentDepth = length(fragToLight);

	float bias = max(0.00025 * (1.0 - dot(normal, normalize(fragToLight))), 0.00025);

	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
	return shadow;
}

void main()
{
	//Texture Samples
	vec3 diffuseColor = texture(diffuseMap, fs_in.TexCoords).rgb;
	vec3 specularColor = texture(specularMap, fs_in.TexCoords).rgb;

	//Light variables
	float lightConstant = 1.0;
	float lightLinear = 0.6;
	float lightQuadratic = 0.04;
	vec3 normal = normalize(fs_in.Normal);
	vec3 lightDir = normalize(lightPosition - fs_in.FragPos);
	vec3 viewDir = normalize(cameraPosition - fs_in.FragPos);

	//Ambient
	vec3 ambient = diffuseColor * 0.1;

	//Diffuse
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 diffuse = diff * lightColor;

	//Specular
	vec3 halfwayDir = normalize(lightDir + viewDir);
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
	vec3 specular = spec * specularColor;

	//Attenuation
	float dist = length(lightPosition - fs_in.FragPos);
	float atten = 1.0 / (lightConstant + lightLinear * dist * lightQuadratic * pow(dist, 2));

	//Shadow
	float shadow = ShadowCalculation(normal, lightDir);

	vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * atten * diffuseColor;
	fragmentColor = vec4(lighting, 1.0);
}