namespace ja2
{
	//! Head clothing inventory item type.
	public class InventoryItemInstanceHeadClothing : InventoryItemInstance
	{
#region Attributes
		//! Clothing.
		public HeadClothing clothing;
#endregion

#region Construction
		public InventoryItemInstanceHeadClothing(InventoryItem Item, sbyte Status, HeadClothing Clothing)
			: base(Item, Status)
		{
			clothing = Clothing;
		}
#endregion
	}
} /*ja2*/
