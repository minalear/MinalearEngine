#version 420
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;
layout (location = 2) in vec3 normal;
layout (location = 3) in vec3 tangent;

out VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentLightPos;
	vec3 TangentViewPos;
	vec3 TangentFragPos;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

uniform vec3 lightPosition;
uniform vec3 cameraPosition;

void main()
{
	vs_out.FragPos = vec3(model * vec4(position, 1.0));
	vs_out.TexCoords = uv;

	mat3 normalMatrix = transpose(inverse(mat3(model)));
	vec3 T = normalize(normalMatrix * tangent);
	vec3 N = normalize(normalMatrix * normal);
	T = normalize(T - dot(T, N) * N);
	vec3 B = cross(N, T);

	mat3 TBN = transpose(mat3(T, B, N));
	vs_out.TangentLightPos = TBN * lightPosition;
	vs_out.TangentViewPos = TBN * cameraPosition;
	vs_out.TangentFragPos = TBN * vs_out.FragPos;

	gl_Position = proj * view * model * vec4(position, 1);
}