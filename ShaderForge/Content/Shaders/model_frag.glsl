#version 420
in vec3 Position;
in vec2 UV;
in vec3 Normal;

out vec4 fragmentColor;

uniform sampler2D texture0; //Diffuse
uniform sampler2D texture1; //Normal
uniform sampler2D texture2; //Specular

void main()
{
	fragmentColor = texture(texture0, UV);
}
