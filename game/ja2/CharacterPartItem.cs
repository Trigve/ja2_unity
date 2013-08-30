namespace ja2
{
	public struct CharacterPartItem
	{
#region Attributes
		//! Type for character.
		public CharacterType type;
		//! Used prefab.
		public string prefab;
#endregion

#region Construction
		public CharacterPartItem(CharacterType Type, string Prefab)
		{
			type = Type;
			prefab = Prefab;
		}
#endregion
	}
} /*ja2*/
