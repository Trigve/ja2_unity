using System;
using System.Collections.Generic;

namespace ja2
{
	//! Vector of texture atlas variants;
	using TerrainAtlasVariantVector_t = List<TextureAtlasInfo>;
	using TerrainTypeMap_t = Dictionary<byte, List<TextureAtlasInfo>>;

	public sealed class TerrainTileSet
	{
#region Fields

		//! Splat used.
		public readonly TerrainTileSplat splatUsed;
		//! Material name used.
		public readonly string materialName;
		//! Map of terrain type id and terrain type.
		private TerrainTypeMap_t m_TerrainTypeMap = new TerrainTypeMap_t();
#endregion

#region  Operations
		//! Add terrain type.
		public void AddType(byte Type, TextureAtlasInfo Variant)
		{
			// There isn't value, create one
			if(!m_TerrainTypeMap.ContainsKey(Type))
				m_TerrainTypeMap[Type] = new TerrainAtlasVariantVector_t();

			var texture_atlas_list = m_TerrainTypeMap[Type];
			texture_atlas_list.Add(Variant);
		}
		//! Get tile type.
		public TextureAtlasInfo GetTileType(byte Type, byte Variant)
		{
			return m_TerrainTypeMap[Type][Variant];
		}
#endregion
#region Construction
		public TerrainTileSet(TerrainTileSplat Splat, string MaterialName)
		{
			splatUsed = Splat;
			materialName = MaterialName;
		}
#endregion
	}
}
