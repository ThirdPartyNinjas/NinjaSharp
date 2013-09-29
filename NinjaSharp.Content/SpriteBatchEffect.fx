float4x4 TransformMatrix;
float2 Viewport;

sampler TextureSampler;

void VertexShaderFunction(inout float4 position : POSITION0,
		  				inout float4 color    : COLOR0,
						inout float2 texCoord : TEXCOORD0)
{
    position = mul(position, TransformMatrix);
    
	position.xy -= 0.5;

	position.xy /= Viewport;
	position.xy *= float2(2, -2);
	position.xy -= float2(1, -1);
}

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	return tex2D(TextureSampler, coords);
}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
