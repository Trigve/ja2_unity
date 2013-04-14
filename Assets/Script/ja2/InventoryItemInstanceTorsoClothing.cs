namespace ja2
{
	//! Torso clothing inventory item implementation.
	public class InventoryItemInstanceTorsoClothing : InventoryItemInstance
	{
#region Attributes
		//! Current clothing.
		public readonly TorsoClothing clothing;
#endregion

#region Construction
		public InventoryItemInstanceTorsoClothing(InventoryItem Item, sbyte Status, TorsoClothing Clothing)
			: base(Item, Status)
		{
			clothing = Clothing;
		}
#endregion
	}
} /*ja2*/
