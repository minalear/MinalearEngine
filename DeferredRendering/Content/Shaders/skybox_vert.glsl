#version 400
layout (location = 0) in vec3 position;

out vec3 TexCoords;

uniform mat4 view;
uniform mat4 proj;

void main()
{
    TexCoords = position;
    vec4 pos = proj * view * vec4(position, 1.0);

    //Set depth to max value 1.0
    gl_Position = pos.xyww;
}