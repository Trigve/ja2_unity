using UnityEngine;
using System.Collections;
using System;

namespace ja2
{
	public class Soldier
	{
#region Attributes
		//! Name/Description.
		public string name;
		//! Terrain which unit occupies, actual position.
		public TerrainTile tile;
		//! Look direction.
		public LookDirection lookDirection { get; set; }
		//! Character used for given soldier.
		public Character character { get; private set; }
#endregion

#region Construction
		public Soldier()
		{
			// Default direction
			lookDirection = LookDirection.SOUTHEAST;
		}

		public Soldier(TerrainTile Tile, Character Character_)
			: this()
		{
			tile = Tile;
			character = Character_;
		}
#endregion
	}
}
