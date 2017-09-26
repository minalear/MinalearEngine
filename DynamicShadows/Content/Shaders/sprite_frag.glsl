#version 420
in VS_OUT {
	vec2 TexCoords;
} fs_in;

out vec4 fragmentColor;

uniform sampler2D sprite;

void main()
{
	fragmentColor = texture(sprite, fs_in.TexCoords);
}