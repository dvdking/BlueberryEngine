#version 330 core

uniform mat4 WPV;
in vec2 vposition;
in vec4 vcolor;
in vec2 vtexcoord;

out vec4 fcolor;
out vec2 ftexcoord;

void main(void)
{
	fcolor = vcolor;
	ftexcoord = vtexcoord;
	gl_Position = WPV * vec4(vposition, 0, 1); 
}