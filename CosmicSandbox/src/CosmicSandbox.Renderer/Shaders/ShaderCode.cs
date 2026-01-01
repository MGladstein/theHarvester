namespace CosmicSandbox.Renderer.Shaders;

/// <summary>
/// HLSL shader code for rendering celestial bodies
/// </summary>
public static class ShaderCode
{
    public const string CelestialBodyVertexShader = @"
cbuffer ConstantBuffer : register(b0)
{
    matrix World;
    matrix View;
    matrix Projection;
    float3 CameraPosition;
    float Time;
};

struct VS_INPUT
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TANGENT;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 WorldPos : POSITION0;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 ViewDir : TEXCOORD1;
    float3 Tangent : TANGENT;
    float3 Bitangent : BITANGENT;
};

VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;

    float4 worldPos = mul(float4(input.Position, 1.0), World);
    output.WorldPos = worldPos.xyz;
    output.Position = mul(mul(worldPos, View), Projection);

    output.Normal = normalize(mul(float4(input.Normal, 0.0), World).xyz);
    output.Tangent = normalize(mul(float4(input.Tangent, 0.0), World).xyz);
    output.Bitangent = cross(output.Normal, output.Tangent);

    output.TexCoord = input.TexCoord;
    output.ViewDir = normalize(CameraPosition - worldPos.xyz);

    return output;
}
";

    public const string CelestialBodyPixelShader = @"
cbuffer MaterialBuffer : register(b0)
{
    float3 EmissiveColor;
    float EmissiveStrength;
    float3 AtmosphereColor;
    float AtmosphereHeight;
    float Metallic;
    float Roughness;
    float2 Padding;
};

cbuffer LightBuffer : register(b1)
{
    float3 SunPosition;
    float SunIntensity;
    float3 SunColor;
    float AmbientIntensity;
};

Texture2D AlbedoTexture : register(t0);
Texture2D NormalTexture : register(t1);
Texture2D RoughnessTexture : register(t2);
SamplerState LinearSampler : register(s0);

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float3 WorldPos : POSITION0;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 ViewDir : TEXCOORD1;
    float3 Tangent : TANGENT;
    float3 Bitangent : BITANGENT;
};

static const float PI = 3.14159265359;

// PBR functions
float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / max(denom, 0.001);
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float3 FresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

float4 main(PS_INPUT input) : SV_TARGET
{
    // Sample textures
    float4 albedo = AlbedoTexture.Sample(LinearSampler, input.TexCoord);
    float3 normalMap = NormalTexture.Sample(LinearSampler, input.TexCoord).xyz * 2.0 - 1.0;
    float roughness = RoughnessTexture.Sample(LinearSampler, input.TexCoord).r;

    // Apply normal mapping
    float3x3 TBN = float3x3(
        normalize(input.Tangent),
        normalize(input.Bitangent),
        normalize(input.Normal)
    );
    float3 N = normalize(mul(normalMap, TBN));

    float3 V = normalize(input.ViewDir);
    float3 L = normalize(SunPosition - input.WorldPos);
    float3 H = normalize(V + L);

    // PBR calculation
    float3 F0 = lerp(float3(0.04, 0.04, 0.04), albedo.rgb, Metallic);

    float NDF = DistributionGGX(N, H, Roughness);
    float G = GeometrySmith(N, V, L, Roughness);
    float3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

    float3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
    float3 specular = numerator / max(denominator, 0.001);

    float3 kS = F;
    float3 kD = (1.0 - kS) * (1.0 - Metallic);

    float NdotL = max(dot(N, L), 0.0);
    float3 radiance = SunColor * SunIntensity;

    float3 Lo = (kD * albedo.rgb / PI + specular) * radiance * NdotL;

    // Ambient
    float3 ambient = float3(0.03, 0.03, 0.03) * albedo.rgb * AmbientIntensity;

    // Emissive
    float3 emissive = EmissiveColor * EmissiveStrength;

    float3 color = ambient + Lo + emissive;

    // HDR tone mapping
    color = color / (color + float3(1.0, 1.0, 1.0));

    // Gamma correction
    color = pow(color, float3(1.0/2.2, 1.0/2.2, 1.0/2.2));

    return float4(color, albedo.a);
}
";

    public const string SkyboxVertexShader = @"
cbuffer ConstantBuffer : register(b0)
{
    matrix ViewProjection;
};

struct VS_INPUT
{
    float3 Position : POSITION;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
};

VS_OUTPUT main(VS_INPUT input)
{
    VS_OUTPUT output;
    output.Position = mul(float4(input.Position, 1.0), ViewProjection).xyww;
    output.TexCoord = input.Position;
    return output;
}
";

    public const string SkyboxPixelShader = @"
TextureCube SkyboxTexture : register(t0);
SamplerState LinearSampler : register(s0);

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
};

float4 main(PS_INPUT input) : SV_TARGET
{
    return SkyboxTexture.Sample(LinearSampler, input.TexCoord);
}
";
}
