using UnityEngine;
using System.Collections;
using System;

/*
public sealed class World : Singleton<World>
{
	public int a;
}
*/

public sealed class World : MonoSingleton<World>
{
#region Attributes
	//! Main camera.
	private CameraManager cameraMain;
	//! Camera move in progress.
	private bool cameraMove;
	//! Game cursor.
	private GameCursor cursor;
#endregion

#region Operations
	public override void Init()
	{
		cameraMain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
	}

	void Awake()
	{
		// Instantiate cursor if not found
		GameObject cursor_go;
		if ((cursor_go = GameObject.Find("Cursor")) == null)
		{
			var prefab_class = Resources.Load("Prefabs/Cursor", typeof(GameObject));
			cursor_go = (GameObject)Instantiate(prefab_class);
			cursor_go.name = prefab_class.name;
			// Save it
			cursor = cursor_go.GetComponent<GameCursor>();
		}
	}

	void Start()
	{
		cameraMove = false;
		
	}

	void FixedUpdate()
	{
		// Camera not moving and need to move
		if (!cameraMove)
		{
			CameraManager.Direction dir = CameraMoveDirection();
			// Need to move
			if (dir != CameraManager.Direction.NONE)
				StartCoroutine(MoveCamera_Coro(dir));
		}
	}

	private IEnumerator MoveCamera_Coro(CameraManager.Direction Dir)
	{
		cameraMove = true;
		// Start moving
		yield return StartCoroutine(MoveCameraFadeIn_Coro(Dir));
		// Stop move
		cameraMove = false;
	}

	private IEnumerator MoveCameraFadeIn_Coro(CameraManager.Direction StartDirection)
	{
		float ratio = 0;
		float time = 0;
		while (true)
		{
			// Can continue fade
			if (CameraMoveDirection() == StartDirection)
			{
				time += Time.fixedDeltaTime;
				ratio = Mathf.Lerp(0, 1.1f, time);
				float ratio_time = Mathf.Clamp01(ratio) * Time.fixedDeltaTime;
				// No place to move
				if (!cameraMain.CheckDirection(StartDirection, ratio_time))
					yield break;
				// Still need to fade
				if (ratio < 1)
					cameraMain.Move(StartDirection, ratio_time);
				// Fade in finished
				else
				{
					yield return StartCoroutine(MoveCameraLinear_CoRo(StartDirection));
					break;
				}
			}
			// Fade interrupted, fade out
			else
			{
				yield return StartCoroutine(MoveCameraFadeOut_Coro(StartDirection));

				break;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator MoveCameraLinear_CoRo(CameraManager.Direction Direction)
	{
		while (true)
		{
			// Can continue to move
			if (CameraMoveDirection() == Direction)
			{
				// No place to move
				if (!cameraMain.CheckDirection(Direction, Time.fixedDeltaTime))
					yield break;
				// Move
				cameraMain.Move(Direction, Time.fixedDeltaTime);
			}
			// Stop move
			else
			{
				// No place to move
				if (!cameraMain.CheckDirection(Direction, 1))
					yield break;
				// Continue with Fade out
				yield return StartCoroutine(MoveCameraFadeOut_Coro(Direction));
				
				break;
			}
			
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator MoveCameraFadeOut_Coro(CameraManager.Direction StartDirection)
	{
		float ratio = 1;
		float time = 0;
		while (true)
		{
			time += Time.fixedDeltaTime * 2;
			ratio = Mathf.Lerp(1, 0, time);
			float ratio_time = Mathf.Clamp01(ratio) * Time.fixedDeltaTime;
			// No place to move
			if (!cameraMain.CheckDirection(StartDirection, ratio_time))
				yield break;

			if (ratio > 0.1)
				cameraMain.Move(StartDirection, ratio_time);
			// End of fade
			else
				break;

			yield return new WaitForFixedUpdate();
		}
	}

	private CameraManager.Direction CameraMoveDirection()
	{
		CameraManager.Direction dir = CameraManager.Direction.NONE;

		if ((Input.mousePosition.x >= 0 && Input.mousePosition.x <= 40) || Input.GetKey(KeyCode.LeftArrow))
			dir = CameraManager.Direction.LEFT;
		else if ((Input.mousePosition.x >= Screen.width - 40 && Input.mousePosition.x < Screen.width) || Input.GetKey(KeyCode.RightArrow))
			dir = CameraManager.Direction.RIGHT;
		else if ((Input.mousePosition.y >= 0 && Input.mousePosition.y <= 40) || Input.GetKey(KeyCode.DownArrow))
			dir = CameraManager.Direction.BOTTOM;
		else if ((Input.mousePosition.y >= Screen.height - 40 && Input.mousePosition.y < Screen.height) || Input.GetKey(KeyCode.UpArrow))
			dir = CameraManager.Direction.TOP;

		return dir;
	}
#endregion
	
}

/*
//! World class
[Serializable]
public sealed class World : ScriptableObject
{
	public int a;
	//! Static instance.
	private static World m_Instance;

	//! Accessor for outer world.
	public static World Instance
	{
		get
		{
			// Not found, create
			if (m_Instance == null)
			{
				m_Instance = ScriptableObject.CreateInstance<World>();
				m_Instance.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_Instance;
		}
	}

#region Attributes
#endregion

#region Operations
	public void OnEnabled()
	{
		hideFlags = HideFlags.HideAndDontSave;
	}
	public void OnApplicationQuit()
	{
		m_Instance = null;
	}
#endregion

	void Awake()
	{
		// Don't destroy object
//		DontDestroyOnLoad(gameObject);
	}

}
*/