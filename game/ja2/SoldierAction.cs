using UnityEngine;
using System.Collections;

namespace ja2
{
	public abstract class SoldierAction
	{
#region Operations
		//! Run.
		public IEnumerator Run(ISoldierController Controller)
		{
			// Wait
			yield return null;
			// Run task
			yield return utils.Task.WaitForTask(DoRun(Controller));
		}

		//! Run action implementations.
		protected abstract IEnumerator DoRun(ISoldierController Controller);
#endregion
	}
}