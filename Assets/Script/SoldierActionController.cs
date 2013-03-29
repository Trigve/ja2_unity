using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class SoldierActionController
{
#region Attributes
	//! Soldier controller to which actions apply.
	private SoldierController soldierController;
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
			actualTask = new utils.Task(actions.Dequeue().Run(soldierController));
		// If task is finnished, get the next task
		if (!actualTask.Running)
			actualTask = null;
	}
#endregion

#region Construction
	public SoldierActionController(SoldierController Controller)
	{
		soldierController = Controller;
	}
#endregion
}
