using System;

namespace ja2
{
	//! Flags for inventory class.
	/*!
		It is used in inventory items and holders and define which item could
		be passed to which holder. If holder has class of None, everything
		could be passed there. Otherwise only items with given class.
		The number used in xml are indices in enum definition.
	*/
	[Flags]
	public enum InventoryItemClass
	{
		None =			0,
		Earwear =		1 << 0,
		Facewear =		1 << 1,
		Helmet =		1 << 2,
		Torso =			1 << 3,
	}
} /*ja2*/
