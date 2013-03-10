using System;

namespace ja2
{
	public struct TextureAtlasInfo
	{
		public float uvOffsetW;
		public float uvOffsetH;
		public float uvWidth;
		public float uvHeight;

#region Construction
		public TextureAtlasInfo(float UvOffsetW, float UvOffsetH, float UvWidth, float UvHeight)
		{
			uvOffsetW = UvOffsetW;
			uvOffsetH = UvOffsetH;
			uvWidth = UvWidth;
			uvHeight = UvHeight;
		}
#endregion
	}
}
