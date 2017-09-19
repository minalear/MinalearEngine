#version 420
layout (location = 0) in vec2 position;

out vec2 UV;

uniform mat4 model;
uniform mat4 view;

void main()
{
	UV = vec2(position.x, 1.0 - position.y);
	gl_Position = view * model * vec4(position, 0, 1);
}