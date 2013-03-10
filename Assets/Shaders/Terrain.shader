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
		Pass
		{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

sampler2D _TerrainTex;
sampler2D _SplatTex;
sampler2D _NoiseTex;
float _NoiseRatio;

struct v2f
{
	float4 pos : SV_POSITION;
	fixed4 color : COLOR;
	float2 uv1 : TEXCOORD0;
	float2 uv2 : TEXCOORD1;
	float2 uv3 : TEXCOORD2;
	float2 uv4 : TEXCOORD3;
};

float4 _TerrainTex_ST;
float4 _NoiseTex_ST;

v2f vert (appdata_full v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.color = v.color;
	o.uv1 = TRANSFORM_TEX (v.texcoord, _TerrainTex);
	o.uv2 = TRANSFORM_TEX (v.texcoord1, _TerrainTex);
	o.uv3 = v.tangent.xy;
	o.uv4 = TRANSFORM_TEX (float2(v.vertex.x / _NoiseRatio, v.vertex.z / _NoiseRatio), _NoiseTex);

	return o;
}

half4 frag (v2f i) : COLOR
{
	half4 mat_1 = tex2D(_TerrainTex, i.uv1);
	half4 mat_2 = tex2D(_TerrainTex, i.uv2);
	half4 splat = tex2D(_SplatTex, i.uv3.xy);
	half4 noise = tex2D(_NoiseTex, i.uv4);
	
	return (i.color * lerp(mat_1, mat_2, splat.a)) * noise * 1.2;

}
ENDCG
		}
	}
}
