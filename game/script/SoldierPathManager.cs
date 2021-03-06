using UnityEngine;
using System.Collections;

namespace ja2.script
{
	public sealed class SoldierPathManager
	{
#region Attributes
		//! Soldier controller.
		private SoldierController soldier;
		//! Actions for given soldier.
		private ja2.SoldierActionController soldierActions;
#endregion

#region Properties
		//! Path finished.
		public bool finished
		{
			get
			{
				return soldierActions.finished;
			}
		}
#endregion

#region Operiations
		//! Run actions.
		public void Run()
		{
			soldierActions.Run();
		}

		//! Handle move end.
		private void OnMoveEnd()
		{
			Debug.Log("On move end.");
		}

		//! Cancel actions.
		public void Cancel()
		{
			soldierActions.Cancel();
		}
#endregion

#region Construction
		public SoldierPathManager(SoldierController Soldier, ja2.TerrainTileHandle[] Tiles)
		{
			ja2.TerrainTileHandle previous = null;
			// Last direction
			ja2.Direction last_direction = ja2.Direction.NONE;
			// Create action controller
			soldierActions = new ja2.SoldierActionController(Soldier);
			// Actual vertex
			foreach (var tile in Tiles)
			{
				// Not first tile
				if (previous != null)
				{
					// Get actual direction
					ja2.Direction dir = TerrainManager.GetDirection(previous, tile);
					// Direction change
					if (last_direction != dir)
					{
						// Set rotation
						soldierActions.Add(new ja2.SoldierActionRotate(ja2.LookDirectionConverter.Convert(dir)));
					}
					// Queue move first if needed
					soldierActions.Add(new ja2.SoldierActionMove());
					// Update last direction
					last_direction = dir;
				}
				// Update previous tile
				previous = tile;
			}
		}
#endregion
	}
} /*ja2.script*/