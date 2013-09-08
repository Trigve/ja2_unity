namespace ja2
{
	//! Define what can be held in inventory.
	public sealed class InventoryItemHolder
	{
#region Fields
		//! Class for current holder.
		public readonly InventoryItemClass classType;
#endregion

#region Construction
		public InventoryItemHolder(InventoryItemClass Class_)
		{
			classType = Class_;
		}
#endregion
	}
} /*ja2*/
