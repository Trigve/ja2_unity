using UnityEngine;
using System.Collections;

public class SoldierActionMove : SoldierAction
{
#region Attributes
	//! Tile to move.
	private readonly ushort tilesToMove;
#endregion
#region Operations
	//! See base.
	protected override IEnumerator DoRun(SoldierController Controller)
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
