namespace ja2
{
	//! Define inventory item type.
	public sealed class InventoryItem
	{
#region Fields
		//! Item class for given item.
		public readonly InventoryItemClass classType;
		//! Size of item.
		public readonly sbyte size;
		//! Weight of item in grams.
		public readonly uint weight;
		//! Name.
		public readonly string name;
#endregion

#region Construction
		public InventoryItem(InventoryItemClass Class_, sbyte Size, uint Weight, string Name)
		{
			classType = Class_;
			size = Size;
			weight = Weight;
			name = Name;
		}
#endregion
	}
} /*ja2*/
