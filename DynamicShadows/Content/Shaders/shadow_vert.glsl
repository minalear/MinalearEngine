#version 420
layout(location = 0) in vec3 aPos;

out VS_OUT {
	vec4 FragPos;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
	vs_out.FragPos = model * vec4(aPos, 1.0);
	gl_Position = proj * view * model * vec4(aPos, 1.0);
}