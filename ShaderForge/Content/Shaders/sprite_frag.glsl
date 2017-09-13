#version 420
in vec2 TexCoord;

out vec4 fragmentColor;

uniform sampler2D sprite;

void main()
{
    fragmentColor = texture(sprite, TexCoord);
}