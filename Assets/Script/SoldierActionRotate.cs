using UnityEngine;
using System.Collections;

public class SoldierActionRotate : SoldierAction
{
#region Attributes
	//! Look rotation.
	private readonly ja2.LookDirection look;
#endregion


#region Operations
	//! See base.
	protected override IEnumerator DoRun(SoldierController Controller)
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