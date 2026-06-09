
matrix xWorldViewProjection;
TextureCube xTextureCube;
sampler TextureSampler : register(s0);

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Position3D : TEXCOORD0;
};

VertexShaderOutput Vertex_Shader(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, xWorldViewProjection);
    output.Position3D = input.Position;
    return output;
}

float4 Pixel_Shader(VertexShaderOutput input) : SV_Target0
{
    return xTextureCube.Sample(TextureSampler, float3(input.Position3D.x, -input.Position3D.y, -input.Position3D.z));
}

technique Technique0
{
    pass pass0
    {
        VertexShader = compile vs_4_0 Vertex_Shader();
        PixelShader = compile ps_4_0 Pixel_Shader();
    }
};
