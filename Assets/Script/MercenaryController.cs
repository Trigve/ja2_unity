using UnityEngine;
using System.Collections;
using System;

public class MercenaryController : MonoBehaviourEx
{
#region Constants
	//! This constant is dependent of transition time.
	protected const float MOVE_DIFF = 0.06f;
#endregion
#region Static
	static protected int walkParam = Animator.StringToHash("walk");
#endregion
#region Attributes
	//! Draw path.
	public bool debugPathDraw;
	//! Is rotating.
	public bool isRotating {get; private set;}
	//! Mercenary unit data.
	protected ja2.Mercenary mercenary;
	//! Terrain manager.
	protected TerrainManager terrainManager;
	//! Animator.
	protected Animator animator;
	//! Distance moved.
	private float accumulateTranslate;
	//! Update position in move handler.
	private bool updatePosition = true;
	//! Parent object transform.
	private Transform parentTransform;
#endregion

#region Properties
	//! Set/Get position.
	public ja2.TerrainTile position
	{
		get
		{
			return mercenary.tile;
		}
		set
		{
			mercenary.tile = value;
			UpdatePosition();	
		}
	}
#endregion


#region Operations
	virtual public void Awake()
	{
		isRotating = false;
		terrainManager = GameObject.Find("Map").GetComponent<TerrainManager>();
		animator = GetComponent<Animator>();
		parentTransform = transform.parent.transform;
	}

	protected void Start()
	{
		UpdatePosition();
		UpdateOrientation();
	}

	void OnAnimatorMove()
	{

		// Not in transition or in transition and need to update
		if (updatePosition)
		{
			Vector3 pos_to_translate = new Vector3(0, animator.deltaPosition.y, new Vector3(animator.deltaPosition.x, 0, animator.deltaPosition.z).magnitude);
			// Reset y of parent, don't want to hover
			Vector3 pos_to_translate_without_y = new Vector3(pos_to_translate.x, 0, pos_to_translate.z);
			parentTransform.Translate(pos_to_translate_without_y, Space.Self);
			// Set the y position to  actual object
			transform.Translate(new Vector3(0, pos_to_translate.y, 0), Space.Self);

			accumulateTranslate += pos_to_translate.z;
		}

		parentTransform.rotation *= animator.deltaRotation;
	}

	//! Set mercenary.
	public void SetMercenary(ja2.Mercenary Mercenary)
	{
		mercenary = Mercenary;
	}
	
	//! Move.
	public void Move(ushort NumberOfTiles)
	{
		StartCoroutine(Move_Coro(NumberOfTiles));
	}

	//! Move as coroutine.
	virtual public IEnumerator Move_Coro(ushort NumberOfTiles)
	{
		// Start updating position
		updatePosition = true;
		// Reset accumulate translation
		accumulateTranslate = 0;
		// Compute target position
		ja2.TerrainTile target_tile = terrainManager.map.GetTile(mercenary.tile, LookDirToMoveDir(mercenary.lookDirection), NumberOfTiles);
		Vector3 target_pos = terrainManager.GetPosition(target_tile);
		// Beginning position must actual transform position because we will
		// never be in the center of tile
		Vector3 beg_pos = parentTransform.position;
		Vector3 tile_beg_pos = terrainManager.GetPosition(mercenary.tile);

		// Set walk state
		animator.SetBool(walkParam, true);

		var line_render = GameObject.Find("Lines").GetComponent<LineRenderer>();
		line_render.SetVertexCount(4);
		line_render.SetColors(Color.white, Color.white);
		line_render.SetWidth(0.1f, 0.1f);

		Vector3 old_pos = (target_pos - beg_pos).normalized;
		// Full distance to target
		float distance = Vector3.Distance(beg_pos, target_pos);
		// Actual distance to target
		float distance_to_go = distance;
		while (true)
		{
			// Update the distance to target
			distance_to_go = distance - accumulateTranslate;
			// Compute angle change from offset; There will be always some
			// offset from center of tile, and therefor we need to always
			// direct the object toward the center because otherwise
			// the accumulated offset could cause that we will end up in between
			// tiles or other weird position
			parentTransform.LookAt(target_pos);
			// Debug drawing of distance
			if (!line_render.enabled)
			{
				if (debugPathDraw)
				{
					Debug.DrawLine(tile_beg_pos, target_pos);
					Debug.DrawLine(parentTransform.position, target_pos, Color.red);
				}
			}
			else
			{
				if (debugPathDraw)
				{
					line_render.SetPosition(0, tile_beg_pos);
					line_render.SetPosition(1, target_pos);

					line_render.SetPosition(2, parentTransform.position);
					line_render.SetPosition(3, target_pos);
				}
			}


			if (distance_to_go <= MOVE_DIFF)
			{
				break;
			}
			// Update actual tile
			RaycastHit hit;
			Ray ray = new Ray(new Vector3(parentTransform.position.x, 1, parentTransform.position.z), Vector3.down);
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, Terrain.LAYER_MASK))
			{
				ja2.TerrainPartition.TriangleMap tile_x_y = hit.transform.gameObject.GetComponent<Terrain>().GetTile(hit.triangleIndex);
				mercenary.tile = terrainManager.map.GetTile(tile_x_y.x, tile_x_y.y);
			}
			// Wait till next update
			yield return null;
		}
		// Stop animations
		animator.SetBool(walkParam, false);

		// Wait for transition to happen
		while(!animator.IsInTransition(0))
			yield return null;
		// Don't update position anymore
		updatePosition = false;
		// Wait till transition end
		while(animator.IsInTransition(0))
		{
			yield return null;
		}

		// Need to adjust position of mercenary
		mercenary.tile = target_tile;
	}

	//! Rotate.
	public void Rotate(ja2.Mercenary.LookDirection Direction)
	{
		StartCoroutine(Rotate_CoRo(Direction));
	}

	//! Rotate as coroutine
	public IEnumerator Rotate_CoRo(ja2.Mercenary.LookDirection Direction)
	{
		isRotating = true;

		float time_to_pass = 0.1f * Mathf.Abs((byte)mercenary.lookDirection - (byte)Direction);
		float start = Time.time;
		Quaternion start_rotation = parentTransform.rotation;
		Quaternion end_rotation = DirectionToRotation(Direction);

		while (Time.time - start < time_to_pass)
		{
			// Update rotation of GO
			parentTransform.rotation = Quaternion.Lerp(start_rotation, end_rotation, (Time.time - start) / time_to_pass);
			// Compute actual direction			
			mercenary.lookDirection = OrientationFromAngle(parentTransform.rotation);

			yield return null;
		}
		mercenary.lookDirection = Direction;
		UpdateOrientation();

		isRotating = false;
	}
	//! Get the direction and angle for given LookAts.
	/*!
		If bool is true, right direction is used. Otherwise left.
	*/
	private Quaternion DirectionToRotation(ja2.Mercenary.LookDirection To)
	{
		return Quaternion.AngleAxis((byte)To * 45, Vector3.up);
	}

	//! Get orientation from old orientation and angle.
	private ja2.Mercenary.LookDirection OrientationFromAngle(Quaternion Orientation)
	{
		Vector3 vec;
		float angle;
		Orientation.ToAngleAxis(out angle, out vec);
		// Get number of 45 rotations
		float angle_count = (vec.y > 0 ? angle : 360 - angle) / 45;
		// Lower bound
		byte lower_bound = (byte)Mathf.FloorToInt(angle_count);
		// Compute new rotation look
		return (ja2.Mercenary.LookDirection)(((angle_count - lower_bound >= 0.5f ? lower_bound + 1 : lower_bound) % 8));
	}

	//! Update current position.
	protected void UpdatePosition()
	{
		parentTransform.position = new Vector3(terrainManager.GetPosition(mercenary.tile, 1).x, 0, terrainManager.GetPosition(mercenary.tile, 0).z);
	}

	//! Update current orientation.
	protected void UpdateOrientation()
	{
		parentTransform.rotation = DirectionToRotation(mercenary.lookDirection);
	}

	//! Convert look direction to move direction.
	static protected ja2.Map.Direction LookDirToMoveDir(ja2.Mercenary.LookDirection Direction)
	{
		ja2.Map.Direction move_dir;
		switch(Direction)
		{
			case ja2.Mercenary.LookDirection.EAST:
				move_dir = ja2.Map.Direction.EAST;
				break;
			case ja2.Mercenary.LookDirection.SOUTHEAST:
				move_dir = ja2.Map.Direction.SOUTH_EAST;
				break;
			case ja2.Mercenary.LookDirection.SOUTH:
				move_dir = ja2.Map.Direction.SOUTH;
				break;
			case ja2.Mercenary.LookDirection.SOUTHWEST:
				move_dir = ja2.Map.Direction.SOUTH_WEST;
				break;
			case ja2.Mercenary.LookDirection.WEST:
				move_dir = ja2.Map.Direction.WEST;
				break;
			case ja2.Mercenary.LookDirection.NORTHWEST:
				move_dir = ja2.Map.Direction.NORTH_WEST;
				break;
			case ja2.Mercenary.LookDirection.NORTH:
				move_dir = ja2.Map.Direction.NORTH;
				break;
			case ja2.Mercenary.LookDirection.NORTHEAST:
				move_dir = ja2.Map.Direction.NORTH_EAST;
				break;
			default:
				throw new System.ArgumentException();
				break;
		}

		return move_dir;
	}
#endregion

	
}
