using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace ja2
{
	//! Inventory reference struct.
	public struct InventoryItemRef
	{
#region Attributes
		public string type;
		public string src;
#endregion
		public InventoryItemRef(string Type, string Src)
		{
			type = Type;
			src = Src;
		}
	}

	//! Inventory item manager.
	public class InventoryItemManager
	{
#region Attributes
		//! Actual working path.
		private readonly string wrkPath;
		//! Map of all items.
		private Dictionary<string, Tuple<InventoryItem, InventoryItemRef>> items = new Dictionary<string, Tuple<InventoryItem, InventoryItemRef>>();
#endregion

#region Operations
		//! Load an inventory.
		public Tuple<InventoryItem, InventoryItemRef> load(string Name)
		{
			// Not loaded yet
			if (!items.ContainsKey(Name))
			{
				XmlReader xml = XmlReader.Create(new StringReader(((TextAsset)Resources.Load(wrkPath + "/" + Name, typeof(TextAsset))).text));
				// Parse file
				xml.Read();
				// Root node
				xml.MoveToContent();
				// Get all attributes
				byte inventory_class_int = (byte)Convert.ToUInt16(xml.GetAttribute("inventory_class"));
				var inventory_class = (InventoryItemClass)(inventory_class_int == 0 ? 0 : 1 << inventory_class_int);
				sbyte size = (sbyte)Convert.ToUInt16(xml.GetAttribute("size"));
				uint weight = Convert.ToUInt32(xml.GetAttribute("weight"));
				string name = xml.GetAttribute("name");
				// Get the reference to item object
				xml.ReadToDescendant("ref");
				var item_ref = new InventoryItemRef(xml.GetAttribute("type"), xml.GetAttribute("src"));
				// Add new item
				items[Name] = new Tuple<InventoryItem, InventoryItemRef>(new InventoryItem(inventory_class, size, weight, name), item_ref);
			}

			return items[Name];
		}
#endregion

#region Construction
		public InventoryItemManager(string Path)
		{
			wrkPath = Path + "/" + "items";
		}
#endregion
	}
} /*ja2*/
