using System;

namespace ja2
{
	public enum CharacterPart
	{
		Head,
		Torso,
	}

	//! Helper class for CharacterPart.
	public static class CharacterPartHelper
	{
#region Operations
		//! Get character part enum based on string.
		public static CharacterPart FromString(string Part)
		{
			switch (Part)
			{
				case "head":
					return CharacterPart.Head;
				case "torso":
					return CharacterPart.Torso;
			}

			throw new ArgumentException("Bad character part '" + Part + "'");
		}
#endregion
	}
} /*ja2*/
