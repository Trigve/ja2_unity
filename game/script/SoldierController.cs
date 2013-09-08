//#define JA_MERCENARY_CONTROLLER_PRINT_MOVE

using UnityEngine;
using System.Collections;
using System;
using ja2;

public class SoldierController : MonoBehaviourEx, ISoldierController
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
	//! Combined mesh.
	public GameObject combinedMesh { get; set; }
	//! Mercenary unit data.
	protected ja2.Soldier mercenary;
	//! Terrain manager.
	public TerrainManager terrainManager;
	//! Animator.
	protected Animator animator;
	//! Rigid body.
	private Rigidbody rigidBody;
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
		animator = GetComponent<Animator>();
		rigidBody = GetComponent<Rigidbody>();
	}

	protected void Start()
	{
		UpdatePosition();
		UpdateOrientation();
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
	public IEnumerator Move_Coro(ushort NumberOfTiles)
	{
		// Does the position of GO need to be clamped to center of tile
		bool clamp_position = false;
		// Save constraints
		RigidbodyConstraints rigid_body_constrains = rigidBody.constraints;
		// Compute target position
		ja2.TerrainTile target_tile = terrainManager.map.GetTile(mercenary.tile, LookDirToMoveDir(mercenary.lookDirection), NumberOfTiles);
		Vector3 target_pos = terrainManager.GetPosition(target_tile);
		// Beginning position must actual transform position because we will
		// never be in the center of tile
		Vector3 beg_pos = transform.position;
		Vector3 tile_beg_pos = terrainManager.GetPosition(mercenary.tile);

		// Set walk state
		animator.SetBool(walkParam, true);

		// Normal vector of target plane
		Vector3 target_normal_plane = (beg_pos - target_pos).normalized;
		while (true)
		{
			// Actual distance to targetm if < 0 we're beyond the target
			float distance_to_go = utils.Vector3Helper.DistanceSigned(transform.position, target_pos, target_normal_plane);
			// We're in the proximity of error
			if (distance_to_go <= MOVE_DIFF)
			{
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
				print("Stopping walk: " + distance_to_go);
#endif
				// No transition comes to play
				animator.SetBool(walkParam, false);
				yield return new WaitForFixedUpdate();
				// Store actual distance to go
				float distance_to_go_pre = distance_to_go;
				while (true)
				{
					distance_to_go = utils.Vector3Helper.DistanceSigned(transform.position, target_pos, target_normal_plane);
					// We're in proximity of diff transition error or we're beyond the center of tile
					if (distance_to_go < MOVE_DIFF_TRANSITION)
					{
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
						print("To go in transition: " + distance_to_go);
#endif
						break;
					}
					// We're stalling in one place because transition isn't
					// long enough, move a bit closer
					if (animator.GetCurrentAnimatorStateInfo(0).nameHash == idleState)
					{
						Debug.LogWarning("Stall: " + distance_to_go_pre + ", " + distance_to_go);
						rigidBody.MovePosition(target_pos);
						break;
					}
					distance_to_go_pre = distance_to_go;
					// Update tile position
					UpdateTilePosition();
					yield return new WaitForFixedUpdate();
				}
				// Stop updating pos
				rigidBody.constraints |= RigidbodyConstraints.FreezePosition;
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
				print("Distance: " + utils.Vector3Helper.DistanceSigned(transform.position, target_pos, target_normal_plane));
#endif
				// If we're beyond and off at least MOVE_DIFF_TRANSITION, clamp
				// position to center
				clamp_position = (distance_to_go < 0);

				break;
			}
			// Compute angle change from offset; There will be always some
			// offset from center of tile, and therefor we need to always
			// direct the object toward the center because otherwise
			// the accumulated offset could cause that we will end up in between
			// tiles or other weird position
			transform.LookAt(new Vector3(target_pos.x, transform.position.y, target_pos.z));
			// Debug drawing of distance
			if (debugPathDraw)
			{
				Debug.DrawLine(tile_beg_pos, target_pos);
				Debug.DrawLine(transform.position, target_pos, Color.red);
			}
			// Update actual tile
			UpdateTilePosition();
			// Wait till next update
			yield return new WaitForFixedUpdate();
		}
		// Must be in idle and no transition
		while (!(animator.GetCurrentAnimatorStateInfo(0).nameHash == idleState && !animator.IsInTransition(0)))
		{
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
			print("Waiting...");
#endif
			yield return new WaitForFixedUpdate();
		}
#if JA_MERCENARY_CONTROLLER_PRINT_MOVE
		print("Off Distance: " + utils.Vector3Helper.DistanceSigned(transform.position, target_pos, target_normal_plane));
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
		// Reset constraints
		rigidBody.constraints = rigid_body_constrains;
	}

	//! Rotate.
	public void Rotate(ja2.LookDirection Direction)
	{
		StartCoroutine(Rotate_CoRo(Direction));
	}

	//! Rotate as coroutine
	public IEnumerator Rotate_CoRo(ja2.LookDirection Direction)
	{
		isRotating = true;

		float time_to_pass = 0.1f * Mathf.Abs((byte)mercenary.lookDirection - (byte)Direction);
		float start = Time.time;
		Quaternion start_rotation = transform.rotation;
		Quaternion end_rotation = ja2.LookDirectionConverter.DirectionToRotation(Direction);

		while (Time.time - start < time_to_pass)
		{
			// Update rotation of GO
			transform.rotation = Quaternion.Lerp(start_rotation, end_rotation, (Time.time - start) / time_to_pass);
			// Compute actual direction			
			mercenary.lookDirection = OrientationFromAngle(transform.rotation);

			yield return null;
		}
		mercenary.lookDirection = Direction;
		UpdateOrientation();

		isRotating = false;
	}
	

	//! Get orientation from old orientation and angle.
	private ja2.LookDirection OrientationFromAngle(Quaternion Orientation)
	{
		Vector3 vec;
		float angle;
		Orientation.ToAngleAxis(out angle, out vec);
		// Get number of 45 rotations
		float angle_count = (vec.y > 0 ? angle : 360 - angle) / 45;
		// Lower bound
		byte lower_bound = (byte)Mathf.FloorToInt(angle_count);
		// Compute new rotation look
		return (ja2.LookDirection)(((angle_count - lower_bound >= 0.5f ? lower_bound + 1 : lower_bound) % 8));
	}

	//! Update current position.
	protected void UpdatePosition()
	{
		transform.position = new Vector3(terrainManager.GetPosition(mercenary.tile, 1).x, 0, terrainManager.GetPosition(mercenary.tile, 0).z);
	}

	//! Update tile position of mercenary.
	private void UpdateTilePosition()
	{
		RaycastHit hit;
		Ray ray = new Ray(new Vector3(transform.position.x, 1, transform.position.z), Vector3.down);
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, Terrain.LAYER_MASK))
		{
			ja2.TerrainTileHandle tile_handle = hit.transform.gameObject.GetComponent<Terrain>().GetTile(hit.triangleIndex);
			mercenary.tile = terrainManager.map.GetTile(tile_handle);
		}
	}

	//! Update current orientation.
	protected void UpdateOrientation()
	{
		transform.rotation = ja2.LookDirectionConverter.DirectionToRotation(mercenary.lookDirection);
	}

	//! Convert look direction to move direction.
	static protected ja2.Map.Direction LookDirToMoveDir(ja2.LookDirection Direction)
	{
		ja2.Map.Direction move_dir = ja2.Map.Direction.NONE;
		switch(Direction)
		{
			case ja2.LookDirection.EAST:
				move_dir = ja2.Map.Direction.EAST;
				break;
			case ja2.LookDirection.SOUTHEAST:
				move_dir = ja2.Map.Direction.SOUTH_EAST;
				break;
			case ja2.LookDirection.SOUTH:
				move_dir = ja2.Map.Direction.SOUTH;
				break;
			case ja2.LookDirection.SOUTHWEST:
				move_dir = ja2.Map.Direction.SOUTH_WEST;
				break;
			case ja2.LookDirection.WEST:
				move_dir = ja2.Map.Direction.WEST;
				break;
			case ja2.LookDirection.NORTHWEST:
				move_dir = ja2.Map.Direction.NORTH_WEST;
				break;
			case ja2.LookDirection.NORTH:
				move_dir = ja2.Map.Direction.NORTH;
				break;
			case ja2.LookDirection.NORTHEAST:
				move_dir = ja2.Map.Direction.NORTH_EAST;
				break;
		}

		return move_dir;
	}
#endregion

	
}