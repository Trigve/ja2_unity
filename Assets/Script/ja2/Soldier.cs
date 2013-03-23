using UnityEngine;
using System.Collections;
using System;

namespace ja2
{
	public class Soldier
	{
#region Enums
		public enum LookDirection
		{
			EAST = 0,
			SOUTHEAST = 1,
			SOUTH = 2,
			SOUTHWEST = 3,
			WEST = 4,
			NORTHWEST = 5,
			NORTH = 6,
			NORTHEAST = 7,
		}
#endregion

#region Attributes
		//! Name/Description.
		public string name;
		//! Terrain which unit occupies, actual position.
		public TerrainTile tile;
		//! Look direction.
		public LookDirection lookDirection { get; set; }
#endregion

#region Construction
		public Soldier()
		{
			// Default direction
			lookDirection = LookDirection.SOUTHEAST;
		}

		public Soldier(TerrainTile Tile)
			: this()
		{
			tile = Tile;
		}
#endregion
	}
}
