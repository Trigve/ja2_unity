using UnityEngine;
using System.Collections;

//! Soldier controller testing helper.
public class SoldierControllerTesting : MonoBehaviour
{
#region Fields
	//! Should debug tile be drawn.
	public bool drawDebug = true;
	//! Hilite tile GO.
	private TileHilite m_HiliteTile;
	//! Soldier controller component.
	private ja2.script.SoldierController m_SoldierController;
#endregion

#region Messages
	void Awake()
	{
		m_SoldierController = GetComponent<ja2.script.SoldierController>();
		// Need to show debug tile
		m_HiliteTile = utils.PrefabManager.Create("TileHilite").GetComponent<TileHilite>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (drawDebug)
			m_HiliteTile.tile = m_SoldierController.soldier.tile;
	}
#endregion
}
