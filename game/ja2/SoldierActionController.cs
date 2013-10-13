using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ja2
{
	public sealed class SoldierActionController
	{
#region Fields
		//! Soldier controller to which actions apply.
		private ISoldierController soldierController;
		//! All actions.
		private Queue<SoldierAction> actions = new Queue<SoldierAction>();
		//! Actual running task.
		private utils.Task actualTask;
#endregion

#region Properties
		//! Is finished.
		public bool finished
		{
			get
			{
				return (actions.Count == 0 && actualTask != null && !actualTask.Running);
			}
		}
#endregion

#region Operations
		//! Add action.
		public void Add(SoldierAction Action)
		{
			actions.Enqueue(Action);
		}

		//! Run all actions.
		public void Run()
		{
			// Task not started
			if (actualTask == null)
			{
				// Is final action
				bool final = (actions.Count == 1);
				actualTask = new utils.Task(actions.Dequeue().Run(soldierController, final));
			}
			// If task is finnished, get the next task
			if (!actualTask.Running)
				actualTask = null;
		}
#endregion

#region Construction
		public SoldierActionController(ISoldierController Controller)
		{
			soldierController = Controller;
		}
#endregion
	}
}