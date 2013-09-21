using UnityEngine;
using System.Collections;
using System;

namespace ja2
{
	public class Soldier
	{
#region Fields
		//! Name/Description.
		public string name;
		//! Terrain which unit occupies, actual position.
		private TerrainTileHandle m_Tile;
		//! Look direction.
		public LookDirection lookDirection { get; set; }
		//! Character group.
		private CharacterGroup group;
		//! Head.
		private Head head;
		//! Torso.
		private Torso torso;
#endregion

#region Properties
		public TerrainTileHandle tile
		{
			get
			{
				return m_Tile;
			}
			set
			{
				m_Tile = value;
			}
		}
#endregion
#region Operations
		//! Add torso clothing.
		public void AddTorsoClothing(InventoryItemInstanceTorsoClothing Clothing)
		{
			torso.clothingInstance = Clothing;
		}

		//! Add head clothing.
		public void AddHeadClothing(InventoryItemInstanceHeadClothing Clothing)
		{
			head.clothingInstance = Clothing;
		}

		//! Get character for given soldier.
		public Character character()
		{
			var character_ = new Character(group);
			// Find all clothing and add it to character
			if (torso.clothingInstance != null)
				character_.AddClothing(torso.clothingInstance.clothing.source);
			if (head.clothingInstance != null)
				character_.AddClothing(head.clothingInstance.clothing.source);


			return character_;
		}
#endregion

#region Construction
		public Soldier()
		{
			// Default direction
			lookDirection = LookDirection.SOUTHEAST;
			torso = new Torso();
			head = new Head();
		}

		public Soldier(TerrainTileHandle Tile, CharacterGroup Group)
			: this()
		{
			m_Tile = Tile;
			group = Group;
		}
#endregion
	}
}
