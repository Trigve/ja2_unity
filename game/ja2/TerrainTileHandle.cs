using System;

namespace ja2
{
	//! Handle for terrain tile.
	[Serializable]
	public struct TerrainTileHandle
	{
		//! X.
		public int x;
		//! Y.
		public int y;

		public TerrainTileHandle(int X, int Y)
		{
			x = X;
			y = Y;
		}
	}
}
