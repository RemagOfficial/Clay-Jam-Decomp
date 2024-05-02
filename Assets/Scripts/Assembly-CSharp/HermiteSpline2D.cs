using UnityEngine;

public class HermiteSpline2D
{
	public delegate float MinimiseFunc(Vector2 pos, Vector2 posOnSpline);

	private bool _initialised;

	public Vector2 GetBezierControlPoint0
	{
		get
		{
			return R0 / 3f;
		}
	}

	public Vector2 GetBezierControlPoint1
	{
		get
		{
			return R1 / 3f * -1f;
		}
	}

	private Vector2 P0 { get; set; }

	private Vector2 P1 { get; set; }

	private Vector2 R0 { get; set; }

	private Vector2 R1 { get; set; }

	public HermiteSpline2D()
	{
		_initialised = false;
	}

	public HermiteSpline2D(Vector2 p0, Vector2 p1, Vector2 r0, Vector2 r1)
	{
		Set(p0, p1, r0, r1);
	}

	public void Set(Vector2 p0, Vector2 p1, Vector2 r0, Vector2 r1)
	{
		P0 = p0;
		P1 = p1;
		R0 = r0;
		R1 = r1;
		_initialised = true;
	}

	public Vector2 PointAt(float t)
	{
		if (!_initialised)
		{
			Debug.LogError("unitialised spline");
		}
		float num = t * t;
		float num2 = num * t;
		return P0 * (2f * num2 - 3f * num + 1f) + P1 * (-2f * num2 + 3f * num) + R0 * (num2 - 2f * num + t) + R1 * (num2 - num);
	}

	public Vector2 TangentAt(float t)
	{
		if (!_initialised)
		{
			Debug.LogError("unitialised spline");
		}
		if (R0.sqrMagnitude == 0f || R1.sqrMagnitude == 0f)
		{
			Debug.LogError("Zero tangents in spline");
		}
		float num = t * t;
		return P0 * (6f * num - 6f * t) + P1 * (-6f * num + 6f * t) + R0 * (3f * num - 4f * t + 1f) + R1 * (3f * num - 2f * t);
	}

	public Vector2 Get2ndDerivateAt(float t)
	{
		if (!_initialised)
		{
			Debug.LogError("unitialised spline");
		}
		return P0 * (12f * t - 6f) + P1 * (-12f * t + 6f) + R0 * (6f * t - 4f) + R1 * (6f * t - 2f);
	}

	public float GetMinimumT(MinimiseFunc func, Vector2 pos, float start, float min, out float result, out Vector2 resultPos)
	{
		float num = start;
		resultPos = PointAt(num);
		result = func(pos, resultPos);
		float num2 = min;
		float num3 = 1f;
		float num4 = num + num2 * num3;
		Vector2 vector = PointAt(num4);
		float num5 = func(pos, vector);
		if (num5 > result)
		{
			num3 *= -1f;
		}
		else
		{
			num = num4;
			resultPos = vector;
			result = num5;
		}
		num4 = num + num2 * num3;
		vector = PointAt(num4);
		for (num5 = func(pos, vector); num5 < result; num5 = func(pos, vector))
		{
			num = num4;
			resultPos = vector;
			result = num5;
			num2 *= 2f;
			num4 = num + num2 * num3;
			vector = PointAt(num4);
		}
		while (num2 > min)
		{
			num2 *= 0.5f;
			num4 = num + num2 * num3;
			vector = PointAt(num4);
			num5 = func(pos, vector);
			if (num5 < result)
			{
				num = num4;
				resultPos = vector;
				result = num5;
				continue;
			}
			num3 *= -1f;
			num4 = num + num2 * num3;
			vector = PointAt(num4);
			num5 = func(pos, vector);
			if (num5 < result)
			{
				num = num4;
				resultPos = vector;
				result = num5;
			}
		}
		return num;
	}
}
