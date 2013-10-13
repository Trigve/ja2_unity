using System;
using System.Collections;

namespace ja2
{
	//! Interface for soldier controller.
	public interface ISoldierController
	{
#region Operations
		//! Move soldier coroutine.
		/*!
			If \param Slowdown is true, slowdown the soldier at the end of
			path.
		*/
		IEnumerator Move_Coro(ushort Tiles, bool SlowDown);
		//! Rotate soldier coroutine.
		IEnumerator Rotate_CoRo(LookDirection Direction);
#endregion
	}
}
