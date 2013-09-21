using UnityEngine;
using System.Collections.Generic;
using System;

namespace ja2.script
{
	public class AStarPathManager : MonoBehaviourEx
	{
#region Attributes
		private LevelManager levelManager;
		private TerrainManager terrainManager;
		//! Actual task.
		utils.Task actualTask;
		//! Last created path.
		/*!
			This object is also gizmo-drawed. Do not mix debug functions or bad
			things will happen.
		*/
		private AStarPathMap lastPath;
		//! Time to sleep when drawing.
		public float time = 0.2f;
#endregion

#region Operations
		void Awake()
		{
			levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
			terrainManager = levelManager.terrainManager;
		}

		//! Create A* path.
		public ja2.TerrainTileHandle[] CreatePath(TerrainManager TerrainManager_, ja2.TerrainTileHandle From, ja2.TerrainTileHandle To)
		{
			lastPath = new script.AStarPathMap(TerrainManager_, From, To);
			while (lastPath.RunOnce() == script.AStarPathMap.State.WAIT)
				;

			return lastPath.Path();

		}

		public void CreatePathDebug(TerrainManager TerrainManager_, ja2.TerrainTileHandle From, ja2.TerrainTileHandle To)
		{
			if (actualTask != null)
				actualTask.Stop();

			actualTask = new utils.Task(CreatePathDebug_CoRo(TerrainManager_, From, To));
		}

		public System.Collections.IEnumerator CreatePathDebug_CoRo(TerrainManager TerrainManager_, ja2.TerrainTileHandle From, ja2.TerrainTileHandle To)
		{
			lastPath = new script.AStarPathMap(TerrainManager_, From, To);
			script.AStarPathMap.State state;
			do
			{
				yield return new WaitForSeconds(time);
				state = lastPath.RunOnce();
			} while (state == script.AStarPathMap.State.WAIT);
		}

		void OnDrawGizmosSelected()
		{
			if (lastPath != null)
			{
				Gizmos.color = new Color(1, 1, 1);
				foreach (var tile in lastPath.ClosedSet())
				{
					Vector3 tile_from = terrainManager.GetPosition(tile);
					tile_from.y = 0.1f;

					Gizmos.DrawCube(tile_from, new Vector3(0.5f, 0.5f, 0.5f));
				}
				foreach (var node in lastPath.OpenSet())
				{
					Gizmos.color = new Color(node._1 / lastPath.f_x_End, lastPath.f_x_End / node._1, 0);

					Vector3 tile_from = terrainManager.GetPosition(node._2);
					tile_from.y = 0.1f;

					Gizmos.DrawCube(tile_from, new Vector3(0.1f, 0.1f, 0.1f));
				}
				{
					Gizmos.color = new Color(0, 1, 0);
					Vector3 tile_from = terrainManager.GetPosition(lastPath.end);
					tile_from.y = 0.1f;
					Gizmos.DrawCube(tile_from, new Vector3(0.3f, 0.3f, 0.3f));
				}
			}
		}
#endregion
	}
} /*ja2.script*/