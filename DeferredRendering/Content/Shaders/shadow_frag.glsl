#version 420
in vec4 FragPos;

uniform vec3 lightPosition;
uniform float farPlane;

void main()
{
    float dist = length(FragPos.xyz - lightPosition);
    dist = dist / farPlane;

    gl_FragDepth = dist;
}  