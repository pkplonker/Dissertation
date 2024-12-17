using UnityEngine;

namespace RealTimeUpdateRuntime.ClassWrappers
{
	[System.Serializable]
	public class Vector3Wrapper
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public Vector3Wrapper(Vector3 v3)
		{
			X = v3.x;
			Y = v3.y;
			Z = v3.z;
		}
	}
}