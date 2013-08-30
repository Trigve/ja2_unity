using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace ja2
{
	//! Map between the terrain category and TerrainMaterial.
	using MaterialMap_t = Dictionary<string, TerrainTileSet>;
	//! Map of terrain splats.
	using TerrainSplatMap_t = Dictionary<ushort, TerrainTileSplat>;

	public sealed class TerrainMaterialManager
	{
		// Attributes
		//! Material map.
		private MaterialMap_t  m_MaterialMap = new MaterialMap_t ();
		//! Map of all splats.
		private TerrainSplatMap_t m_TerrainSplatMap = new TerrainSplatMap_t();

#region Operations
	public TerrainTileSet GetTerrainSet(string Category)
	{
	   return m_MaterialMap[Category];
	}
#endregion
#region Construction
		public TerrainMaterialManager(string Path_)
		{
			string full_path = Path_ + '/' + "data" + '/' + "terrain.xml";
			// If file doesn't exist
			XmlReader xml = XmlReader.Create(full_path);
			// Parse file
			xml.Read();
			// Root node
			xml.MoveToContent();
			// Get set
			xml.ReadToDescendant("splat");
			// Move to first child
			if(!xml.ReadToDescendant("type"))
				throw new XmlException("Cannot find element 'type' - " + xml.Name);
			do 
			{
				// Make new splat
				var p_terrain_splat = new TerrainTileSplat();
				m_TerrainSplatMap[Convert.ToUInt16(xml.GetAttribute("id"))] = p_terrain_splat;
				// Get items
				xml.ReadToDescendant("item");
				do 
				{
					p_terrain_splat.AddSplat(Convert.ToUInt16(xml.GetAttribute("id")),
						new TextureAtlasInfo(
							Convert.ToSingle(xml.GetAttribute("woffset")),
							1 - Convert.ToSingle(xml.GetAttribute("hoffset")),
							Convert.ToSingle(xml.GetAttribute("width")),
							 Convert.ToSingle(xml.GetAttribute("height")))
					);
				} while (xml.ReadToNextSibling("item"));
			} while (xml.ReadToNextSibling("type"));
			// Get all sets
			xml.ReadToNextSibling("set");
			do 
			{
				// Get the tile set name
				string set_name = xml.GetAttribute("name");
				// Create tile set instance and save it to map
				var p_terrain_tile_set = new TerrainTileSet(m_TerrainSplatMap[Convert.ToUInt16(xml.GetAttribute("splat"))],
					xml.GetAttribute("material")
				);
				m_MaterialMap[set_name] = p_terrain_tile_set;
				// Get first child
				xml.ReadToDescendant("types");
				do 
				{
					xml.ReadToDescendant("item");
					do 
					{
						// Get the id
						byte type_id = Convert.ToByte(xml.GetAttribute("id"));
						xml.ReadToDescendant("variant");
						do 
						{
							p_terrain_tile_set.AddType(type_id, 
								new TextureAtlasInfo(Convert.ToSingle(xml.GetAttribute("woffset")),
									1 - Convert.ToSingle(xml.GetAttribute("hoffset")),
									Convert.ToSingle(xml.GetAttribute("width")),
									Convert.ToSingle(xml.GetAttribute("height"))
								)
							);
						} while (xml.ReadToNextSibling("variant"));
					} while (xml.ReadToNextSibling("item"));
				} while (xml.ReadToNextSibling("types"));
			} while (xml.ReadToNextSibling("set"));
		}
#endregion
	}
}
