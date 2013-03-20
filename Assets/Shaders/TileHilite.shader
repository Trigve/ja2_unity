Shader "Custom/TileHilite"
{
	Properties
	{
		_Color ("Color", Color) = (1, 0, 0, 1)
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" }
		Lighting On
		Offset 0, -1
		
		pass
		{
			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
		
			fixed4 _Color;

			struct v2f
			{
			  float4 pos : SV_POSITION;
			};
        
			v2f vert (appdata_base v)
      		{
			  v2f o;
			  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			  return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return half4(_Color.rgb, 1);
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
