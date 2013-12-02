#version 330 core
uniform sampler2D colorTexture;
uniform float amount;

in vec4 fcolor;
in vec2 ftexcoord;

out vec4 color;
void main(void) 
{
	vec4 c = texture(colorTexture, ftexcoord);

	float av = (c.r + c.g + c.b) / 3;
	
	if(av > 0.2f)
	{
		color =  c * amount; 
	}
	else
	{ 
		color = vec4(c.r/amount, c.g/amount, c.b*amount, 1.0f);
	}
}