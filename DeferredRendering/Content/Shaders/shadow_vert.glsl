#version 420
layout (location = 0) in vec3 position;

out vec4 FragPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
	FragPos = model * vec4(position, 1);
	gl_Position = proj * view * model * vec4(position, 1);
}