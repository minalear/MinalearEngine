#version 420
in VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 Normal;
} fs_in;

out vec4 fragmentColor;

uniform vec3 cameraPosition;
uniform float farPlane;

const int NUM_LIGHTS = 3;

uniform vec3 lightPositions[NUM_LIGHTS];
uniform vec3 lightColors[NUM_LIGHTS];

uniform sampler2D diffuseMap;
uniform sampler2D specularMap;
uniform samplerCube shadowMaps[NUM_LIGHTS];

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

vec3 LightCalculation(samplerCube shadowMap, vec3 normal, vec3 lightPosition, vec3 lightColor, vec3 diffuseSample, float specSample)
{
	//Light variables
	float lightConstant = 1.0;
	float lightLinear = 0.6;
	float lightQuadratic = 0.04;
	vec3 lightDir = normalize(lightPosition - fs_in.FragPos);
	vec3 viewDir = normalize(cameraPosition - fs_in.FragPos);

	//Ambient
	vec3 ambient = diffuseSample * 0.1;

	//Diffuse
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 diffuse = diff * lightColor;

	//Specular
	vec3 halfwayDir = normalize(lightDir + viewDir);
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
	vec3 specular = vec3(spec * specSample);

	//Attenuation
	float dist = length(lightPosition - fs_in.FragPos);
	float atten = 1.0 / (lightConstant + lightLinear * dist * lightQuadratic * pow(dist, 2));

	//Shadow
	float shadow = ShadowCalculation(shadowMap, lightPosition, normal, lightDir);

	vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * atten * diffuseSample;
	return lighting;
}

void main()
{
	//Texture Samples
	vec3 diffuseColor = texture(diffuseMap, fs_in.TexCoords).rgb;
	vec3 specularColor = texture(specularMap, fs_in.TexCoords).rgb;

	vec3 normal = normalize(fs_in.Normal);

	//Lights
	vec3 lighting = vec3(0);
	for (int i = 0; i < NUM_LIGHTS; i++)
		lighting += LightCalculation(shadowMaps[i], normal, lightPositions[i], lightColors[i], diffuseColor, specularColor.r);
	
	fragmentColor = vec4(lighting, 1.0);
}