using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public int a;
	public static GameManager instance
	{
		get { return GetInstance(); }
	}

	protected static GameManager s_instance;

	public static GameManager GetInstance()
	{
		if (s_instance != null)
			return s_instance;

		GameObject go = Resources.Load("GameManager", typeof(GameObject)) as GameObject;
		GameObject inst = Object.Instantiate(go) as GameObject;

		s_instance = inst.GetComponent<GameManager>();
		DontDestroyOnLoad(inst);

		return s_instance;
	}

	// your game manager related stuff goes here
}
