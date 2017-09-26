#version 420
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUV;
layout(location = 2) in vec3 aNorm;

out VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 Normal;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
	vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
	vs_out.TexCoords = aUV;
	vs_out.Normal = transpose(inverse(mat3(model))) * aNorm;

	gl_Position = proj * view * model * vec4(aPos, 1.0);
}