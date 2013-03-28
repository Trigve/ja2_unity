using UnityEngine;
using System.Collections;

public sealed class Tuple<T1, T2>
{
	public readonly T1 t1;
	public readonly T2 t2;

#region Construction
	public Tuple(T1 T1_, T2 T2_)
	{
		t1 = T1_;
		t2 = T2_;
	}
#endregion
}
