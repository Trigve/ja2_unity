using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
	// Attributes
	
	//! Offset.
	public float Offset = 20F;
	//! World height.
	public float WorldHeightRatio = 70F;
	//! World Width.
	public float WorldWidthRatio = 70F;
	//! Max Y.
	public float MaxY = 20F;
	
	//! Camera angle.
	private const float Angle = 30F;
	
	// Use this for initialization
	void Awake ()
	{
		// Set orientation
		transform.rotation = Quaternion.AngleAxis(90, Vector3.down) * Quaternion.AngleAxis(Angle, Vector3.right);
		
		float world_width = Screen.width / WorldWidthRatio;
		float world_height = Screen.height / WorldWidthRatio;
		
		// Compute length of line of sight AFTER (below) the base plane
		float qx = (world_height * Mathf.Cos(Mathf.Deg2Rad * Angle) + 2 * MaxY)/ (2 * Mathf.Sin(Mathf.Deg2Rad * Angle));
		// Compute the whole line of sight without offset
		float xw = (world_height * Mathf.Cos(Mathf.Deg2Rad * Angle) + 2 * MaxY) / Mathf.Sin(Mathf.Deg2Rad * Angle);
		// The whole length of line of sight from camera till the base plane
		float pos = xw - qx + Offset;
		// Set camera position and frustum
		camera.transform.position = new Vector3(Mathf.Cos(Mathf.Deg2Rad * Angle) * pos, Mathf.Sin(Mathf.Deg2Rad * Angle) * pos, 0);
		camera.nearClipPlane = Offset;
		camera.farClipPlane = Offset + xw;
		// Set the dimensions with wide aspect ration
		camera.orthographicSize = world_width / 2;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKey(KeyCode.LeftArrow))
			transform.Translate(0, 0, -1, Space.World);
		else if (Input.GetKey(KeyCode.RightArrow))
			transform.Translate(0, 0, 1, Space.World);
		else if (Input.GetKey(KeyCode.DownArrow))
			transform.Translate(1, 0, 0, Space.World);
		else if (Input.GetKey(KeyCode.UpArrow))
			transform.Translate(-1, 0, 0, Space.World);
	}
}
