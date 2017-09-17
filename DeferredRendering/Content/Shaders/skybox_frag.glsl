#version 400
in vec3 TexCoords;

out vec4 fragColor;

uniform samplerCube texture0;

void main()
{
    fragColor = texture(texture0, TexCoords);
}