#version 330 core
uniform sampler2D colorTexture;
uniform sampler2D LightTex;
uniform float amount;

in vec4 fcolor;
in vec2 ftexcoord;

out vec4 color;
void main(void) 
{
	vec4 c = texture(colorTexture, ftexcoord);
	vec4 c2 = texture(LightTex, ftexcoord);

	float av = (c.r + c.g + c.b) / 3;
	
	if(av > 0.2f)
	{
		color =  c * amount; 
	}
	else
	{ 
		color = (c2*0.8f + c*0.2f);
	}
}