using UnityEngine;
using System.Collections;

namespace ja2
{
	public class SoldierActionMove : SoldierAction
	{
#region Operations
		//! See base.
		protected override IEnumerator DoRun(ISoldierController Controller, bool Final)
		{
			yield return utils.Task.WaitForTask(Controller.Move_Coro(Final));
		}
#endregion
	}
}