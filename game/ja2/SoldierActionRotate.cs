using UnityEngine;
using System.Collections;

namespace ja2
{
	public class SoldierActionRotate : SoldierAction
	{
#region Fields
		//! Look rotation.
		private readonly ja2.LookDirection look;
#endregion


#region Operations
		//! See base.
		protected override IEnumerator DoRun(ISoldierController Controller, bool Final)
		{
			yield return utils.Task.WaitForTask(Controller.Rotate_CoRo(look));
		}
#endregion

#region Construction
		public SoldierActionRotate(ja2.LookDirection Dir)
		{
			look = Dir;
		}
#endregion
	}
}