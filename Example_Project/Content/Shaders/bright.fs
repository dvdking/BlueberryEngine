#version 330 core
uniform sampler2D colorTexture;
uniform float amount;

in vec4 fcolor;
in vec2 ftexcoord;

out vec4 color;
void main(void) 
{
	color = texture(colorTexture, ftexcoord) * amount; 
}