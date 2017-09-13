#version 420
in vec3 Position;
in vec2 UV;
in vec3 Normal;

out vec4 fragmentColor;

uniform vec3 cameraPosition;
uniform mat4 model;

uniform sampler2D texture0; //Diffuse
uniform sampler2D texture1; //Normal
uniform sampler2D texture2; //Specular

void main()
{
	vec3 normal = normalize(mat3(transpose(inverse(model))) * Normal);
	vec3 lightDir = normalize(-vec3(-1, -1, -0.5));
	
	vec3 viewDir = normalize(cameraPosition - Position);

	float diff = max(dot(normal, lightDir), 0.0);
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 16);

	vec3 ambient = vec3(0.68, 0.8, 0.85) * vec3(texture(texture0, UV));
	vec3 diffuse = vec3(1.0, 0.8, 0.65) * diff * vec3(texture(texture0, UV));

	vec3 result = (ambient + diffuse);
	fragmentColor = vec4(result, 1);
}
