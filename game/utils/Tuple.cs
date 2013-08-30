using UnityEngine;
using System.Collections;

public struct Tuple<T1, T2>
{
	public readonly T1 _1;
	public readonly T2 _2;

#region Construction
	public Tuple(T1 T1_, T2 T2_)
	{
		_1 = T1_;
		_2 = T2_;
	}
#endregion
}
