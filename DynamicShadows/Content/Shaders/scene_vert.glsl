#version 420
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUV;
layout(location = 2) in vec3 aNorm;

out VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 Normal;
	vec4 FragPosLightSpace;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;
uniform mat4 lightSpaceMatrix;

void main()
{
	vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
	vs_out.TexCoords = aUV;
	vs_out.Normal = transpose(inverse(mat3(model))) * aNorm;
	vs_out.FragPosLightSpace = lightSpaceMatrix * vec4(vs_out.FragPos, 1.0);

	gl_Position = proj * view * model * vec4(aPos, 1.0);
}