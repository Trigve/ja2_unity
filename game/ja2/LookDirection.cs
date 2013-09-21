using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ja2
{
	public enum LookDirection
	{
		EAST = 0,
		SOUTHEAST = 1,
		SOUTH = 2,
		SOUTHWEST = 3,
		WEST = 4,
		NORTHWEST = 5,
		NORTH = 6,
		NORTHEAST = 7,
	}

	public static class LookDirectionConverter
	{
#region Operations
		//! Convert Map.Direction to LookDirection.
		public static LookDirection Convert(Direction Dir)
		{
			LookDirection direction = LookDirection.EAST;

			switch(Dir)
			{
				case ja2.Direction.EAST:
					direction = LookDirection.EAST;
					break;
				case ja2.Direction.NORTH:
					direction = LookDirection.NORTH;
					break;
				case ja2.Direction.NORTH_EAST:
					direction = LookDirection.NORTHEAST;
					break;
				case ja2.Direction.NORTH_WEST:
					direction = LookDirection.NORTHWEST;
					break;
				case ja2.Direction.SOUTH:
					direction = LookDirection.SOUTH;
					break;
				case ja2.Direction.SOUTH_EAST:
					direction = LookDirection.SOUTHEAST;
					break;
				case ja2.Direction.SOUTH_WEST:
					direction = LookDirection.SOUTHWEST;
					break;
				case ja2.Direction.WEST:
					direction = LookDirection.WEST;
					break;
			}

			return direction;
		}

		//! Get the direction and angle for given LookAts.
		/*!
			If bool is true, right direction is used. Otherwise left.
		*/
		public static Quaternion DirectionToRotation(LookDirection To)
		{
			return Quaternion.AngleAxis((byte)To * 45, Vector3.up);
		}
#endregion
	}
}
