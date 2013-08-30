using System;
using UnityEngine;

namespace ja2
{
	//! Terrain manager interface.
	public interface ITerrainManager
	{
#region Properties
		//! Get map instance.
		Map map {get;}
#endregion
#region Operations
		//! Get position of tile.
		Vector3 GetPosition(ja2.TerrainTile Tile);
#endregion
	}
}
