using System.Collections.Generic;

namespace ja2
{
	//! Character representation
	public sealed class Character
	{
#region Attributes
		//! Character group.
		public readonly CharacterGroup group;
		//! All clothes attached.
		private List<string> clothes_ = new List<string>();
#endregion

#region Properties
		public List<string> clothes
		{
			get
			{
				return clothes_;
			}
		}
#endregion

#region Operations
		//! Add clothing.
		public void AddClothing(string Name)
		{
			clothes_.Add(Name);
		}
#endregion

#region Construction
		public Character(CharacterGroup Group)
		{
			group = Group;
		}
#endregion
	}
} /*ja2*/
