Shader "Custom/Border"
{
	Properties
	{
		_Color("Border color", Color) = (1,0,0,1)
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" }
		Offset 0, -1
		Pass
		{
			Color [_Color]
			Material
			{
			}
			Cull Off
			ZWrite Off
			Lighting Off
		}
	
	} 
	FallBack "Diffuse"
}
