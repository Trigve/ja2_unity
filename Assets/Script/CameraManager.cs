using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
#region Enums
	public enum Direction
	{
		NONE = 0,
		LEFT,
		RIGHT,
		TOP,
		BOTTOM,
	}
#endregion
	// Attributes
	
	//! Offset.
	public float Offset = 20F;
	//! World height.
	public float WorldHeightRatio = 70F;
	//! World Width.
	public float WorldWidthRatio = 70F;
	//! Max Y.
	public float MaxY = 20F;
	//! Amount to shift camera.
	public float amount;
	//! Camera angle.
	private const float Angle = 30F;
	//! Initial window size.
	private Rect initialWindowSize = new Rect();
	
	// Use this for initialization
	void Awake ()
	{
		RecalculateCamera();
	}
	
	//! Move camera in given direction.
	public void Move(Direction Dir, float Ratio = 1)
	{
		// Recalculate camera on windo size
		if (initialWindowSize.width != camera.pixelWidth || initialWindowSize.height != camera.pixelHeight)
			RecalculateCamera();
		// Check direction as first
		if (CheckDirection(Dir))
		{
			switch(Dir)
			{
				case Direction.LEFT:
					transform.Translate(0, 0, -amount * Ratio, Space.World);
					break;
				case Direction.RIGHT:
					transform.Translate(0, 0, amount * Ratio, Space.World);
					break;
				case Direction.BOTTOM:
					transform.Translate(amount * Ratio, 0, 0, Space.World);
					break;
				case Direction.TOP:
					transform.Translate(-amount * Ratio, 0, 0, Space.World);
					break;
			}
		}
	}

	private bool CheckDirection(Direction Dir)
	{
		bool ret = false;
		Vector3 point_to_check;
		if (Dir == Direction.LEFT || Dir == Direction.BOTTOM)
			point_to_check = new Vector3(0, 0, 0);
		else
			point_to_check = new Vector3(1, 1, 0);
		// Find if we are beyond the terrain
		Ray ray = Camera.main.ViewportPointToRay(point_to_check);
		float point;
		new Plane(Vector3.up, 0).Raycast(ray, out point);
		Vector3 point_on_plane = ray.GetPoint(point);
		var terrain_manager = GameObject.Find("Map").GetComponent<TerrainManager>();
		ja2.Map map = terrain_manager.map;
		ja2.TerrainTile last_tile = map.GetTile(map.width - 1, map.height - 1);
		Vector3 last_tile_pos_1 = terrain_manager.GetPosition(last_tile, 1);
		Vector3 last_tile_pos_2 = terrain_manager.GetPosition(last_tile, 2);

		// Check position
		switch(Dir)
		{
			case Direction.LEFT:
				ret = point_on_plane.z - amount > ja2.TerrainPartition.TILE_WIDTH;
				break;
			case Direction.TOP:
				ret = point_on_plane.x - amount > ja2.TerrainPartition.TILE_HEIGHT;
				break;
			case Direction.BOTTOM:
				ret = point_on_plane.x + amount < last_tile_pos_2.x;
				break;
			case Direction.RIGHT:
				ret = point_on_plane.z + amount < last_tile_pos_2.z;
				break;
		}
		
		return ret;
	}

	private void RecalculateCamera()
	{
		// Update size
		initialWindowSize = camera.pixelRect;
		// Set orientation
		transform.rotation = Quaternion.AngleAxis(90, Vector3.down) * Quaternion.AngleAxis(Angle, Vector3.right);
		
		float world_width = Screen.width / WorldWidthRatio;
		float world_height = Screen.height / WorldHeightRatio;

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
		// Find if we are beyond the terrain
		Ray ray = Camera.main.ViewportPointToRay(new Vector3(0, 0, 0));
		float point;
		new Plane(Vector3.up, 0).Raycast(ray, out point);
		Vector3 point_on_plane = ray.GetPoint(point);
		// Normalize position
		transform.Translate(0, 0, -point_on_plane.z + ja2.TerrainPartition.TILE_WIDTH, Space.World);
		transform.Translate(point_on_plane.x + ja2.TerrainPartition.TILE_HEIGHT, 0, 0, Space.World);
	}
}
