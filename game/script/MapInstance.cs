using UnityEngine;
using System.Collections;
using System;

[Serializable]
//! Holder for map references.
/*!
 *	We're encapsulating it because we cannot use custom constructor on
 *	ScriptableObjects (if map would inherit from it).
*/
public sealed class MapInstance : ScriptableObject
{
#region Attributes
	//! Map instance.
	[SerializeField]
	public ja2.Map map;
#endregion

#region Operations
#endregion
}
