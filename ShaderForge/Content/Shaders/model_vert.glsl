#version 400
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;
layout (location = 2) in vec3 normal;

out vec3 Position;
out vec2 UV;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
	Position = vec3(model * vec4(position, 1));
	UV = uv;
	Normal = normal;

	gl_Position = proj * view * model * vec4(position, 1);
}