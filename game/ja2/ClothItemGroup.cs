namespace ja2
{
	public sealed class ClothItemGroup
	{
#region Fields
		//! Prefab name.
		public readonly string prefab;
		//! Item parts.
		public readonly ClothItem[] items;
#endregion		

#region Construction
		public ClothItemGroup(string Prefab, ClothItem[] Items)
		{
			prefab = Prefab;
			items = Items;
		}
#endregion
	}
} /*ja2*/
