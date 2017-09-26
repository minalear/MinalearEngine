#version 420
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUV;

out VS_OUT {
	vec2 TexCoords;
} vs_out;

uniform int X;
uniform float spriteWidth;
uniform float spriteHeight;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
	vs_out.TexCoords = vec2((aUV.x + X) * spriteWidth, aUV.y * spriteHeight);
	gl_Position = proj * view * model * vec4(aPos, 1.0);
}