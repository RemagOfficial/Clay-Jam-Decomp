using System;
using UnityEngine;

[Serializable]
public class GougeSplineSample
{
	public float SplineDist;

	public Vector3 Position { get; set; }

	public Quaternion Rotation { get; set; }

	public Vector3 Scale { get; set; }

	public bool Flatten { get; set; }
}
