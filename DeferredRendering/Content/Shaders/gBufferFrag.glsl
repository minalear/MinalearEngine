#version 420
in vec2 UV;
in vec3 FragPos;
in vec3 Normal;

layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;

uniform sampler2D diffuseMap;
uniform sampler2D specularMap;

void main()
{
	vec4 diffuseSample = texture(diffuseMap, UV);
	vec4 specularSample = texture(specularMap, UV);

	//Store fragment position into the gBuffer
	gPosition = FragPos;

	//Store per-fragment normal into the gBuffer
	gNormal = normalize(Normal);

	//Add diffuse and specular into the gBuffer, with specular in the alpha component
	gAlbedoSpec.rgb = diffuseSample.rgb;
	gAlbedoSpec.a = specularSample.r;
}
