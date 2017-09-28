#version 420
in VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 Normal;
	vec4 FragPosLightSpace;
} fs_in;

out vec4 fragmentColor;

struct DirLight {
	vec3 direction;
	vec3 color;
	sampler2D shadowMap;
};
struct PointLight {
	vec3 position;
	vec3 color;
	samplerCube shadowMap;
};

uniform vec3 cameraPosition;
uniform float farPlane;

const int MAX_LIGHTS = 8;

uniform int NumLights;
uniform PointLight lights[MAX_LIGHTS];
uniform DirLight directLight;

uniform sampler2D diffuseMap;
uniform sampler2D specularMap;

float ShadowCalculation(samplerCube shadowMap, vec3 lightPosition, vec3 normal, vec3 lightDir)
{
	vec3 fragToLight = fs_in.FragPos - lightPosition;
	float closestDepth = texture(shadowMap, fragToLight).r;
	closestDepth *= farPlane;
	float currentDepth = length(fragToLight);

	float bias = max(0.00025 * (1.0 - dot(normal, normalize(fragToLight))), 0.00025);

	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
	return shadow;
}

float DirShadowCalculation(vec3 normal, vec3 lightDir)
{
	vec3 projCoords = fs_in.FragPosLightSpace.xyz / fs_in.FragPosLightSpace.w;
	projCoords = projCoords * 0.5 + 0.5;

	float closestDepth = texture(directLight.shadowMap, projCoords.xy).r;
	float currentDepth = projCoords.z;

	float bias = max(0.00025 * (1.0 - dot(normal, lightDir)), 0.00025);
	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	if (projCoords.z > 1.0)
		shadow = 0.0;

	return shadow;
}

vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 diffuseSample, float specSample)
{
	//Light variables
	float lightConstant = 1.0;
	float lightLinear = 0.6;
	float lightQuadratic = 0.04;
	vec3 lightDir = normalize(light.position - fs_in.FragPos);
	vec3 viewDir = normalize(cameraPosition - fs_in.FragPos);

	//Ambient
	vec3 ambient = diffuseSample * 0.1;

	//Diffuse
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 diffuse = diff * light.color;

	//Specular
	vec3 halfwayDir = normalize(lightDir + viewDir);
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
	vec3 specular = vec3(spec * specSample);

	//Attenuation
	float dist = length(light.position - fs_in.FragPos);
	float atten = 1.0 / (lightConstant + lightLinear * dist * lightQuadratic * pow(dist, 2));

	//Shadow
	float shadow = ShadowCalculation(light.shadowMap, light.position, normal, lightDir);

	vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * atten * diffuseSample;
	return lighting;
}

vec3 CalculateDirLight(DirLight light, vec3 normal, vec3 diffuseSample, float specSample)
{
	vec3 lightDir = normalize(-light.direction);
	vec3 viewDir = normalize(cameraPosition - fs_in.FragPos);

	//Ambient
	vec3 ambient = diffuseSample * 0.25;

	//Diffuse
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * light.color;

	//Specular
	vec3 halfwayDir = normalize(lightDir + viewDir);
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
	vec3 specular = vec3(spec * specSample);

	//Shadow
	float shadow = DirShadowCalculation(normal, lightDir);

	vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * diffuseSample;
	return lighting;
}

void main()
{
	//Texture Samples
	vec3 diffuseColor = texture(diffuseMap, fs_in.TexCoords).rgb;
	vec3 specularColor = texture(specularMap, fs_in.TexCoords).rgb;

	vec3 normal = normalize(fs_in.Normal);

	//Lights
	vec3 lighting = CalculateDirLight(directLight, normal, diffuseColor, specularColor.r);
	for (int i = 0; i < NumLights && i < MAX_LIGHTS; i++)
		lighting += CalculatePointLight(lights[i], normal, diffuseColor, specularColor.r);
	
	fragmentColor = vec4(lighting, 1.0);
}