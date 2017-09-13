#version 420
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 texcoord;

out vec2 TexCoord;

uniform vec2 uvOffset;
uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
    TexCoord = texcoord + uvOffset;
    gl_Position = proj * view * model * vec4(position, 1);
}