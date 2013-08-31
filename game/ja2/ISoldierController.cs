using System;
using System.Collections;

namespace ja2
{
	//! Interface for soldier controller.
	public interface ISoldierController
	{
#region Operations
		//! Move soldier coroutine.
		IEnumerator Move_Coro(ushort Tiles);
		//! Rotate soldier coroutine.
		IEnumerator Rotate_CoRo(LookDirection Direction);
#endregion
	}
}
