#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    const float Sharpness = 1.8;
    const float Zoom = 0.2;
    float2 uv = input.TextureCoordinates;
	
    float2 ndir = float2(0.0, 1.0);
    float fac1 = abs(dot(uv - 0.5, normalize(ndir.yx))) * 2.0;
    float fac2 = -dot(uv - 0.5, normalize(ndir)) * 2.0;
    float2 new_uv = uv + pow(fac1, Sharpness) * fac2 * Zoom * ndir;
	
    return tex2D(SpriteTextureSampler, new_uv) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};