using UnityEngine;

public class Slope
{
	public float Degrees { get; set; }

	public float Length { get; set; }

	public Vector3 Normal { get; private set; }

	public Vector3 Gravity { get; private set; }

	public Slope(float degrees, float length)
	{
		Degrees = degrees;
		Length = length;
	}

	public void Initialise()
	{
		Vector3 up = Vector3.up;
		Quaternion quaternion = Quaternion.AngleAxis(Degrees, Vector3.left);
		Normal = quaternion * up;
		Vector3 gravity = Physics.gravity;
		quaternion = Quaternion.AngleAxis(Degrees, Vector3.right);
		Gravity = quaternion * gravity;
	}
}
