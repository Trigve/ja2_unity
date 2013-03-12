Shader "Custom/Glow" {
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Glow", Color) = (1, 0, 0, 1)
		_Ratio ("Ratio", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		fixed4 _Color;
		fixed _Ratio;
				
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = ((1 -_Ratio) * c.rgb) + (_Color.rgb * _Ratio);
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
