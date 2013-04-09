using UnityEngine;
using System.Collections;

namespace utils
{
	public static class Vector3Helper
	{
		//! Signed distance.
		/*!
			\param NormalizedNormal must be normalized, otherwise distance will
			not be precise. It is normal vector of plane which divides the
			space for positive/negative.
		*/
		public static float DistanceSigned(Vector3 From, Vector3 To, Vector3 NormalizedNormal)
		{
			Vector3 actual_pos = From - To;
			return NormalizedNormal.x * actual_pos.x + NormalizedNormal.y * actual_pos.y + NormalizedNormal.z * actual_pos.z;
		}
	}
}