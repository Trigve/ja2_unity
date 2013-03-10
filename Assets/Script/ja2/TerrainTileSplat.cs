using System;
using System.Collections.Generic;

namespace ja2
{
	//! Terrain splat.
	public sealed class TerrainTileSplat
	{
#region Constants
		//! Max splat type variants.
		private const ushort MAX_SPLAT_TYPE_VARIANT_SIZE = 8;
#endregion
	
		// Attributes
		//! Vector of texture coordinates of given splat tile. Index is splatting sub-type(1..15).
		private TextureAtlasInfo[] tileSplatInfo_ = new TextureAtlasInfo[MAX_SPLAT_TYPE_VARIANT_SIZE];

		
		// Operations
		//! Add new splatting info.
		public void AddSplat(ushort SplatIndex, TextureAtlasInfo Info)
		{
			tileSplatInfo_[SplatIndex] = Info;
		}

		public TextureAtlasInfo GetSplat(ushort Index)
		{
			return tileSplatInfo_[Index];
		}
	}
}
