using System;
using UnityEngine;

[Serializable]
public class GougeSplineWalker
{
	public GougeSpline Spline { get; private set; }

	public float T { get; set; }

	public Transform ObjectToWalk { get; set; }

	public Vector3 Position { get; private set; }

	public Vector3 Tangent { get; private set; }

	public GougeSplineWalker(Transform objectToWalk)
	{
		ObjectToWalk = objectToWalk;
	}

	public bool StartNewSpline(GougeSectionCollider gougeCollider, bool allowUphill = true)
	{
		if (gougeCollider == null)
		{
			Spline = null;
			return false;
		}
		GougeSpline spline = gougeCollider.Spline;
		float num = spline.GetClosestSplineDistToPoint_WithinSamples_Downhill(ObjectToWalk.position, gougeCollider.StartSample, gougeCollider.EndSample, allowUphill);
		if (num == -1f)
		{
			num = 0f;
		}
		Spline = spline;
		T = num;
		Vector3 position;
		Vector3 tangent;
		Spline.DataAtDist(T, out position, out tangent);
		Position = position + Spline.StartPos;
		Tangent = tangent;
		return true;
	}

	public Vector3 Walk(float dist, ref bool stillOnSpline)
	{
		Vector3 tangent = Tangent;
		Vector3 zero = Vector3.zero;
		Vector3 position = ObjectToWalk.position;
		position.y = 0f;
		float num = dist * 2f;
		float num2 = T + num;
		float num3 = dist / 10f;
		Vector3 position2;
		Spline.DataAtDist(T, out position2, out tangent);
		Position = position2 + Spline.StartPos;
		zero = Position - position;
		float magnitude = zero.magnitude;
		T += num3;
		for (bool flag = true; flag && T >= 0f && T < Spline.Length; flag = ((!(dist > 0f)) ? (T > num2) : (T < num2)))
		{
			Spline.DataAtDist(T, out position2, out tangent);
			Position = Spline.StartPos + position2;
			zero = Position - position;
			magnitude = zero.magnitude;
			if (magnitude >= Mathf.Abs(dist))
			{
				break;
			}
			T += num3;
		}
		zero.Normalize();
		zero *= Mathf.Abs(dist);
		Tangent = tangent;
		if (T < 0f || T > Spline.Length)
		{
			stillOnSpline = false;
		}
		else
		{
			stillOnSpline = true;
		}
		return zero;
	}
}
