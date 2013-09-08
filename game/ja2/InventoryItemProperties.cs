using System.Collections.Generic;

namespace ja2
{
	//! Global properties of item as current position etc.
	public sealed class InventoryItemProperties
	{
#region Fields
		//! Referenced inventory item.
		private InventoryItemInstance item;
#endregion

#region Construction
		public InventoryItemProperties(InventoryItemInstance Item)
		{
			item = Item;
		}
#endregion
	}
} /*ja2*/
