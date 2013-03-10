Shader "Custom/Border"
{
	Properties
	{
		_Color("Border color", Color) = (1,0,0,1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Color [_Color]
			Material
			{
			}
			Cull Off
			ZWrite Off
			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
			SetTexture[_MainTex]
			{
				constantColor [_Color]
				Combine primary * texture
			}
		}
	
	} 
	FallBack "Diffuse"
}
