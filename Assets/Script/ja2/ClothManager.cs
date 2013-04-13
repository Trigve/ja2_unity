using System;
using System.Xml;
using System.Collections.Generic;

namespace ja2
{
	public sealed class ClothManager
	{
#region Attributes
		//! Actual working path.
		private readonly string wrkPath;
		//! Map of all clothes.
		private Dictionary<string, Dictionary<CharacterGroup, ClothItemGroup>> clothes = new Dictionary<string, Dictionary<CharacterGroup, ClothItemGroup>>();
#endregion

#region Operations
		//! Load cloth with given name and for given group.
		public ClothItemGroup load(string Name, CharacterGroup Group)
		{
			// Not loaded yet
			if (!clothes.ContainsKey(Name))
			{
				XmlReader xml = XmlReader.Create(wrkPath + "/" + Name + ".xml");
				// Parse file
				xml.Read();
				// Root node
				xml.MoveToContent();
				// Move to first child
				if (!xml.ReadToDescendant("item"))
					throw new XmlException("Cannot find element 'item' - " + xml.Name);

				// Get item name
				string item_name = xml.GetAttribute("name");
				// Make new group dict
				var group_dict = new Dictionary<CharacterGroup, ClothItemGroup>();
				// Get items
				xml.ReadToDescendant("group");
				do
				{
					// Get group type
					CharacterGroup group = (CharacterGroup)Convert.ToUInt16(xml.GetAttribute("id"));
					// Get prefab name
					string prefab = xml.GetAttribute("prefab");
					// Read all character parts affected
					var char_items = new List<ClothItem>();
					xml.ReadToDescendant("item");
					do
					{
						char_items.Add(new ClothItem(CharacterPartHelper.FromString(xml.GetAttribute("part")), (CharacterType)Convert.ToUInt16(xml.GetAttribute("type"))));
					} while (xml.ReadToNextSibling("item"));
					// Add tuple
					group_dict[group] = new ClothItemGroup(prefab, char_items.ToArray());

				} while (xml.ReadToNextSibling("group"));
				// Add new group
				clothes[item_name] = group_dict;
			}

			return clothes[Name][Group];
		}
#endregion
		
#region Construction
		public ClothManager(string Path)
		{
			wrkPath = Path + '/' + "Clothes";

		}
#endregion
	}
} /*ja2*/
