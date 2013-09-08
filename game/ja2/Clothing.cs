namespace ja2
{
	//! Clothing base class.
	/*!
		All custom clothing class must inherit from this class.
	*/
	public abstract class Clothing
	{
#region Fields
		//! Inventory type the clothing supports.
		public readonly InventoryType inventoryType;
		//! Source to concrete implementation.
		public readonly string source;
#endregion


#region Construction
		protected Clothing(InventoryType Type, string Source)
		{
			inventoryType = Type;
			source = Source;
		}
#endregion
	}
} /*ja2*/
