Shader "Custom/Terrain"
{
	Properties
	{
		_TerrainTex ("Terrain texture", 2D) = "white"{}
		_SplatTex ("Splat texture", 2D) = "white"{}
		_NoiseTex ("Noise texture", 2D) = "white"{}
		_NoiseRatio ("Noise ratio", float) = 10
	}
	SubShader
	{
		Lighting On
		Tags { "RenderType" = "Opaque" }
CGPROGRAM
#pragma surface surf Lambert vertex:vert

sampler2D _TerrainTex;
sampler2D _SplatTex;
sampler2D _NoiseTex;
float _NoiseRatio;

struct Input
{
	float2 custom_uv1;
	float2 custom_uv2;
	float2 custom_uv3;
	float2 custom_uv4;
};

float4 _TerrainTex_ST;
float4 _NoiseTex_ST;

void vert (inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT(Input, o);

	o.custom_uv1 = TRANSFORM_TEX (v.texcoord, _TerrainTex);
	o.custom_uv2 = TRANSFORM_TEX (v.texcoord1, _TerrainTex);
	o.custom_uv3 = v.tangent.xy;
	o.custom_uv4 = TRANSFORM_TEX (float2(v.vertex.x / _NoiseRatio, v.vertex.z / _NoiseRatio), _NoiseTex);
}

void surf(Input IN, inout SurfaceOutput o)
{
	half4 mat_1 = tex2D(_TerrainTex, IN.custom_uv1);
	half4 mat_2 = tex2D(_TerrainTex, IN.custom_uv2);
	half4 splat = tex2D(_SplatTex, IN.custom_uv3);
	half4 noise = tex2D(_NoiseTex, IN.custom_uv4);
	
	o.Albedo = (lerp(mat_1, mat_2, splat.a) * noise * 1.2).rgb;
}
ENDCG
	}
	Fallback "Diffuse"
}
