using System;

namespace ja2
{
	//! Handle for terrain tile.
	[Serializable]
	public sealed class TerrainTileHandle : IEquatable<TerrainTileHandle>
	{
#region Fields
		//! X.
		public int x;
		//! Y.
		public int y;
		//! Partition x.
		public int partitionX;
		//! Partition y.
		public int partitionY;
#endregion

#region Interface
		public bool Equals(TerrainTileHandle Other)
		{
			return (partitionX == Other.partitionX && partitionY == Other.partitionY && x == Other.x && y == Other.y);
		}

		public override bool Equals(object obj)
		{
			return (obj.GetType() == GetType() ? Equals((TerrainTileHandle)obj) : base.Equals(obj));
		}

		public override int GetHashCode()
		{
			return (partitionX << 24 | partitionY << 16 | x << 8 | y);
		}

		public override string ToString()
		{
			return "(" + partitionX.ToString() + "," + partitionY.ToString() + ") " + x.ToString() + "," + y.ToString();
		}
#endregion
#region Construction
		public TerrainTileHandle(int X, int Y, int PartitionX, int PartitionY)
		{
			x = X;
			y = Y;
			partitionX = PartitionX;
			partitionY = PartitionY;
		}
#endregion
	}
} /*ja2*/
