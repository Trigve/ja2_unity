Shader "Custom/FootStep"
{
	Properties {
		_Color("Color", COLOR) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags {"RenderType"="Transparent" "Queue"="Transparent+1"}
		Offset 0, -1
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
    	Pass
    	{
    		SetTexture[_MainTex]
			{
				constantColor [_Color]
				Combine Constant  * texture
			}
    	}
	} 
	FallBack "Diffuse"
}
