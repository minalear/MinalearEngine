#version 420
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;
layout (location = 2) in vec3 normal;

out vec3 Position;
out vec2 UV;
out vec3 Normal;
out vec4 FragPosLightSpace;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;
uniform mat4 lightSpaceMatrix;

void main()
{
	Position = vec3(model * vec4(position, 1));
	UV = uv;
	Normal = transpose(inverse(mat3(model))) * normal;
	FragPosLightSpace = lightSpaceMatrix * vec4(Position, 1.0);

	gl_Position = proj * view * model * vec4(position, 1);
}