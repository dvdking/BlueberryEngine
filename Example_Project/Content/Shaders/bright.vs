#version 330 core

uniform mat4 WPV;

uniform float time;

in vec2 vposition;
in vec4 vcolor;
in vec2 vtexcoord;

out vec4 fcolor;
out vec2 ftexcoord;

void main(void)
{
	fcolor = vcolor;
	ftexcoord = vtexcoord;
	vec4 pos = vec4(vposition, 0, 1);
	gl_Position = WPV * pos;
}