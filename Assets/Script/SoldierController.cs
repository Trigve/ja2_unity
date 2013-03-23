using UnityEngine;
using System.Collections;
using System;

public class SoldierController : MonoBehaviourEx
{
#region Constants
	//! This constant is dependent of transition time.
	protected const float MOVE_DIFF = 0.5f;
	protected const float MOVE_DIFF_TRANSITION = 0.03f;
#endregion
#region Static
	static private int idleState = Animator.StringToHash("Base Layer.Idle");
	static protected int walkParam = Animator.StringToHash("walk");
#endregion
#region Attributes
	//! Draw path.
	public bool debugPathDraw;
	//! Is rotating.
	public bool isRotating {get; private set;}
	//! Mercenary unit data.
	protected ja2.Soldier mercenary;
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
			Translate(new Vector3(0, animator.deltaPosition.y, new Vector3(animator.deltaPosition.x, 0, animator.deltaPosition.z).magnitude));
		}

		parentTransform.rotation *= animator.deltaRotation;
	}

	//! Set mercenary.
	public void SetMercenary(ja2.Soldier Mercenary)
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
		// Does the position of GO need to be clamped to center of tile
		bool clamp_position = false;
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

		var line_render_white = GameObject.Find("LinesWhite").GetComponent<LineRenderer>();
		line_render_white.SetVertexCount(2);
		line_render_white.SetWidth(0.05f, 0.05f);

		var line_render_red = GameObject.Find("LinesRed").GetComponent<LineRenderer>();
		line_render_red.SetVertexCount(2);
		line_render_red.SetWidth(0.05f, 0.05f);

		Vector3 old_pos = (target_pos - beg_pos).normalized;
		// Full distance to target
		float distance = Vector3.Distance(beg_pos, target_pos);
		// Actual distance to target
		float distance_to_go = distance;
		while (true)
		{
			// Update the distance to target
			distance_to_go = distance - accumulateTranslate;
			// We're in the proximity of error
			if (distance_to_go <= MOVE_DIFF)
			{
				// No transition comes to play
				animator.SetBool(walkParam, false);
				yield return null;

				// Store actual distance to go
				float distance_to_go_pre = distance_to_go;
				while (true)
				{
					distance_to_go = distance - accumulateTranslate;
					if (distance_to_go <= MOVE_DIFF_TRANSITION)
					{
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
						print("To go in transition: " + distance_to_go);
#endif
						// Stop updating pos
						updatePosition = false;
						break;
					}
					// We're stalling in one place because transition isn't
					// long enough, move a bit closer
					if (!(distance_to_go_pre > distance_to_go))
					{
						print("Stall");
						Translate(new Vector3(0, 0, MOVE_DIFF_TRANSITION));
						break;
					}

					distance_to_go_pre = distance_to_go;
					// Update tile position
					UpdateTilePosition();
					yield return null;
				}
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
				print("Distance: " + Vector3.Distance(target_pos, parentTransform.position));
#endif
				// If we're way off, clamp position to center
				clamp_position = (distance_to_go <= -MOVE_DIFF);

				break;
			}
			// Compute angle change from offset; There will be always some
			// offset from center of tile, and therefor we need to always
			// direct the object toward the center because otherwise
			// the accumulated offset could cause that we will end up in between
			// tiles or other weird position
			parentTransform.LookAt(target_pos);
			// Debug drawing of distance
			if (!line_render_white.enabled || !line_render_red)
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
					line_render_white.SetPosition(0, tile_beg_pos);
					line_render_white.SetPosition(1, target_pos);

					line_render_red.SetPosition(0, parentTransform.position);
					line_render_red.SetPosition(1, target_pos);
				}
			}
			// Update actual tile
			UpdateTilePosition();
			// Wait till next update
			yield return null;
		}
		// Must be in idle and no transition
		while (!(animator.GetCurrentAnimatorStateInfo(0).nameHash == idleState && !animator.IsInTransition(0)))
		{
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
			print("Waiting...");
#endif
			yield return null;
		}
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
		print("Off Distance: " + Vector3.Distance(target_pos, parentTransform.position));
#endif
		// Need to adjust position of mercenary
		mercenary.tile = target_tile;
		// Need to update position
		if (clamp_position)
		{
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
			print("Clamping position.");
#endif
			UpdatePosition();
		}
	}

	//! Rotate.
	public void Rotate(ja2.Soldier.LookDirection Direction)
	{
		StartCoroutine(Rotate_CoRo(Direction));
	}

	//! Rotate as coroutine
	public IEnumerator Rotate_CoRo(ja2.Soldier.LookDirection Direction)
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
	private Quaternion DirectionToRotation(ja2.Soldier.LookDirection To)
	{
		return Quaternion.AngleAxis((byte)To * 45, Vector3.up);
	}

	//! Get orientation from old orientation and angle.
	private ja2.Soldier.LookDirection OrientationFromAngle(Quaternion Orientation)
	{
		Vector3 vec;
		float angle;
		Orientation.ToAngleAxis(out angle, out vec);
		// Get number of 45 rotations
		float angle_count = (vec.y > 0 ? angle : 360 - angle) / 45;
		// Lower bound
		byte lower_bound = (byte)Mathf.FloorToInt(angle_count);
		// Compute new rotation look
		return (ja2.Soldier.LookDirection)(((angle_count - lower_bound >= 0.5f ? lower_bound + 1 : lower_bound) % 8));
	}

	//! Implementation of actual translation.
	private void Translate(Vector3 Translation)
	{
		// Reset y of parent, don't want to hover
		Vector3 pos_to_translate_without_y = new Vector3(Translation.x, 0, Translation.z);
		parentTransform.Translate(pos_to_translate_without_y, Space.Self);
		// Set the y position to  actual object
		transform.Translate(new Vector3(0, Translation.y, 0), Space.Self);

		accumulateTranslate += Translation.z;
	}

	//! Update current position.
	protected void UpdatePosition()
	{
		parentTransform.position = new Vector3(terrainManager.GetPosition(mercenary.tile, 1).x, 0, terrainManager.GetPosition(mercenary.tile, 0).z);
	}

	//! Update tile position of mercenary.
	private void UpdateTilePosition()
	{
		RaycastHit hit;
		Ray ray = new Ray(new Vector3(parentTransform.position.x, 1, parentTransform.position.z), Vector3.down);
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, Terrain.LAYER_MASK))
		{
			ja2.TerrainPartition.TriangleMap tile_x_y = hit.transform.gameObject.GetComponent<Terrain>().GetTile(hit.triangleIndex);
			mercenary.tile = terrainManager.map.GetTile(tile_x_y.x, tile_x_y.y);
		}
	}

	//! Update current orientation.
	protected void UpdateOrientation()
	{
		parentTransform.rotation = DirectionToRotation(mercenary.lookDirection);
	}

	//! Convert look direction to move direction.
	static protected ja2.Map.Direction LookDirToMoveDir(ja2.Soldier.LookDirection Direction)
	{
		ja2.Map.Direction move_dir;
		switch(Direction)
		{
			case ja2.Soldier.LookDirection.EAST:
				move_dir = ja2.Map.Direction.EAST;
				break;
			case ja2.Soldier.LookDirection.SOUTHEAST:
				move_dir = ja2.Map.Direction.SOUTH_EAST;
				break;
			case ja2.Soldier.LookDirection.SOUTH:
				move_dir = ja2.Map.Direction.SOUTH;
				break;
			case ja2.Soldier.LookDirection.SOUTHWEST:
				move_dir = ja2.Map.Direction.SOUTH_WEST;
				break;
			case ja2.Soldier.LookDirection.WEST:
				move_dir = ja2.Map.Direction.WEST;
				break;
			case ja2.Soldier.LookDirection.NORTHWEST:
				move_dir = ja2.Map.Direction.NORTH_WEST;
				break;
			case ja2.Soldier.LookDirection.NORTH:
				move_dir = ja2.Map.Direction.NORTH;
				break;
			case ja2.Soldier.LookDirection.NORTHEAST:
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
