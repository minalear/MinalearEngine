#version 420
in VS_OUT {
	vec4 FragPos;
} fs_in;

uniform vec3 lightPosition;
uniform float farPlane;

void main()
{
	float dist = length(fs_in.FragPos.xyz - lightPosition);
	dist = dist / farPlane;

	gl_FragDepth = dist;
}