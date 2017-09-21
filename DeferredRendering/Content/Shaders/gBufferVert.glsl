#version 420
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;
layout (location = 2) in vec3 normal;

out vec2 UV;
out vec3 FragPos;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
	UV = uv;
	FragPos = vec3(model * vec4(position, 1.0));
	Normal = transpose(inverse(mat3(model))) * normal;

	gl_Position = proj * view * model * vec4(position, 1);
}