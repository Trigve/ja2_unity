using UnityEngine;
using System.Collections;

public abstract class SoldierAction
{
#region Operations
	//! Run.
	public IEnumerator Run(SoldierController Controller)
	{
		// Wait
		yield return null;
		// Run task
		yield return utils.Task.WaitForTask(DoRun(Controller));
	}

	//! Run action implementations.
	protected abstract IEnumerator DoRun(SoldierController Controller);
#endregion
}
