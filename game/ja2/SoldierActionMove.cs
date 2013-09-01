using UnityEngine;
using System.Collections;

namespace ja2
{
	public class SoldierActionMove : SoldierAction
	{
#region Attributes
		//! Tile to move.
		private readonly ushort tilesToMove;
#endregion
#region Operations
		//! See base.
		protected override IEnumerator DoRun(ISoldierController Controller)
		{
			yield return utils.Task.WaitForTask(Controller.Move_Coro(tilesToMove));
		}
#endregion

#region Construction
		public SoldierActionMove(ushort TilesToMove)
		{
			tilesToMove = TilesToMove;
		}
#endregion
	}
}