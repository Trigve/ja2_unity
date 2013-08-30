using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;

namespace ja2
{
	public sealed class CharacterDefinitionManager
	{
#region Attributes
		//! Dict for all character parts.
		private Dictionary<CharacterPart, Dictionary<CharacterGroup, CharacterPartItem[]>> characterParts = new Dictionary<CharacterPart, Dictionary<CharacterGroup, CharacterPartItem[]>>();
#endregion

#region Operations
		//! Get part prefab name.
		public string PartPrefab(CharacterPart Part, CharacterGroup Group, CharacterType Type)
		{
			return characterParts[Part][Group][(int)Type].prefab;
		}

		//! Parse group tag.
		private void ParseTag(XmlReader Xml, Dictionary<CharacterGroup, CharacterPartItem[]> Definition)
		{
			// Move to first child
			if (!Xml.ReadToDescendant("group"))
				throw new XmlException("Cannot find element 'group' - " + Xml.Name);
			do
			{
				// Make new character part list
				var character_parts = new List<CharacterPartItem>();
				// Get group type
				CharacterGroup group = (CharacterGroup)Convert.ToUInt16(Xml.GetAttribute("id"));
				// Get items
				Xml.ReadToDescendant("item");
				ushort type = 0;
				do
				{
					character_parts.Add(
						new CharacterPartItem((CharacterType)type++, Xml.GetAttribute("prefab"))
					);
				} while (Xml.ReadToNextSibling("item"));
				// Add new group
				Definition[group] = character_parts.ToArray();
			} while (Xml.ReadToNextSibling("group"));
		}
#endregion

#region Construction
		public CharacterDefinitionManager(String Path_)
		{
			string full_path = Path_ + '/' + "character";
			// If file doesn't exist
			XmlReader xml = XmlReader.Create(new StringReader(((TextAsset)Resources.Load(full_path, typeof(TextAsset))).text));
			// Parse file
			xml.Read();
			// Root node
			xml.MoveToContent();
			xml.ReadToDescendant("head");
			// Heads
			{
				var heads = new Dictionary<CharacterGroup, CharacterPartItem[]>();
				ParseTag(xml, heads);
				characterParts[CharacterPart.Head] = heads;
			}
			// Torsos
			xml.ReadToNextSibling("torso");
			{
				var torsos = new Dictionary<CharacterGroup, CharacterPartItem[]>();
				ParseTag(xml, torsos);
				characterParts[CharacterPart.Torso] = torsos;
			}
		}
#endregion
	}
} /*ja2*/
