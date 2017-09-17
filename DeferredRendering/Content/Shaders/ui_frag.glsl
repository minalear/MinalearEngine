#version 420
in vec2 UV;

out vec4 fragColor;

uniform sampler2D image;

void main()
{
	fragColor = texture(image, UV);
}  