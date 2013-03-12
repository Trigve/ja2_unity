using UnityEngine;
using System.Collections;

public class GlowShader : MonoBehaviour
{
#region Attribute
	//! Actual time.
	private float time;
	//! Glow duration.
	public float duration = 2.5f;
#endregion
	
#region Operations
	void Start ()
	{
		// Update time
		time = Time.time;
	}
	
	void Update ()
	{
		// Reset timer if duration passed
		if(Time.time - time > duration)
			time = Time.time;
		// Set ration property in script which is controlling glow effect
		// must change for all materials
		foreach(Material mat in renderer.sharedMaterials)
			mat.SetFloat("_Ratio", Mathf.Sin((2 * Mathf.PI * (Time.time - time) / duration) / 2));
	}
#endregion
}
