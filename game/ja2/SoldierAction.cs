using UnityEngine;
using System.Collections;

namespace ja2
{
	public abstract class SoldierAction
	{
#region Operations
		//! Run.
		/*!
			If \param Final is true, it means that given action is the last one
			in tha action list processed.
		*/
		public IEnumerator Run(ISoldierController Controller, bool Final)
		{
			// Wait
			yield return null;
			// Run task
			yield return utils.Task.WaitForTask(DoRun(Controller, Final));
		}

		//! Run action implementations.
		/*!
			See Run();
		*/
		protected abstract IEnumerator DoRun(ISoldierController Controller, bool Final);
#endregion
	}
}