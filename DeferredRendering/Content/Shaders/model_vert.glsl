#version 420
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;
layout (location = 2) in vec3 normal;
layout (location = 3) in vec3 tangent;
layout (location = 4) in vec3 bitangent;

out vec3 Position;
out vec2 UV;
out mat3 TBN;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
	UV = uv;
	Position = vec3(model * vec4(position, 1));
	//Normal = transpose(inverse(mat3(model))) * normal;

	mat3 normalMatrix = transpose(inverse(mat3(model)));

	vec3 T = normalize(normalMatrix * tangent);
	vec3 N = normalize(normalMatrix * normal);
	T = normalize(T - dot(T, N) * N);
	vec3 B = cross(T, N);

	TBN = mat3(T, B, N);

	gl_Position = proj * view * model * vec4(position, 1);
}