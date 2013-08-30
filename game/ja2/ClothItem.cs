using System.Collections;

namespace ja2
{
	//! Defines which part of body does clothing affects and how.
	public struct ClothItem
	{
#region Attributes
		//! Character that is affected part.
		public CharacterPart part;
		//! Which type is needed for given character part.
		public CharacterType type;
#endregion

#region Construction
		public ClothItem(CharacterPart Part, CharacterType Type)
		{
			part = Part;
			type = Type;
		}
#endregion
	}
} /*ja2*/
