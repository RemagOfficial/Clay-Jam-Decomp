using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class GougeSpline
{
	private const float MinLengthBetweenPoints = 0.35f;

	private const float DistBetweenSamples = 0.25f;

	public const int MaxSamplesPerMesh = 40;

	public List<Vector2> _points;

	public List<Vector2> _tangents;

	public List<float> _lengths;

	public Vector2 _startPos;

	public float _totalLength;

	public HermiteSpline2D _spline;

	private bool _renderingStopped;

	private float _maxTouchSpeed;

	private float _inputStartTime;

	private List<GougeSplineSample> Samples { get; set; }

	public GougeSplineSample EndSample { get; private set; }

	public int NumSamples
	{
		get
		{
			return Samples.Count;
		}
	}

	public bool SamplesReady
	{
		get
		{
			return Samples.Count >= 1;
		}
	}

	private int SamplesSinceRenderBreak { get; set; }

	public static int NextSplineOrder { get; set; }

	public int Order { get; set; }

	private bool NewInput { get; set; }

	public bool UsingFakeFirstPoint { get; private set; }

	public Vector3 StartPos
	{
		get
		{
			return Make3D(_startPos);
		}
	}

	public Vector3 EndPos
	{
		get
		{
			if (_points.Count > 0)
			{
				return Make3D(_startPos + _points[_points.Count - 1]);
			}
			return StartPos;
		}
	}

	public bool IsEmpty
	{
		get
		{
			return _points.Count == 0;
		}
	}

	public float Length
	{
		get
		{
			return _totalLength;
		}
	}

	[method: MethodImpl(32)]
	public event Action<int, Vector3> SplineSampleListReduced;

	[method: MethodImpl(32)]
	public event Action<float> SplineLengthReduced;

	public GougeSpline(Vector3 startPos, bool newInput)
	{
		Order = NextSplineOrder++;
		NewInput = newInput;
		AllocateLists();
		InitialiseSamples();
		Reset();
		AddFirstPoint(startPos);
		_renderingStopped = false;
	}

	private static Vector3 Make3D(Vector2 v2)
	{
		return new Vector3(v2.x, 0f, v2.y);
	}

	private void AllocateLists()
	{
		_points = new List<Vector2>();
		_tangents = new List<Vector2>();
		_lengths = new List<float>();
		_spline = new HermiteSpline2D();
		Samples = new List<GougeSplineSample>();
	}

	private void Reset()
	{
		_startPos = Vector2.zero;
		_totalLength = 0f;
		_maxTouchSpeed = 0f;
	}

	private void AddFirstPoint(Vector3 startPos)
	{
		_startPos = new Vector2(startPos.x, startPos.z);
		AddNewPointData(Vector2.zero, new Vector2(0f, 0.25f), 0f);
		AddFakeFirstPoint();
	}

	private void AddFakeFirstPoint()
	{
		AddNewPointData(new Vector2(0f, 0.25f), new Vector2(0f, 0.25f), 0.25f);
		UsingFakeFirstPoint = true;
	}

	private void CorrectFakeFirstPoint(Vector2 point, Vector2 tangent, float length)
	{
		_points.RemoveAt(1);
		_tangents.RemoveAt(1);
		_totalLength = 0f;
		_lengths.RemoveAt(1);
		Samples.Clear();
		AddNewPointData(point, tangent, length);
		UsingFakeFirstPoint = false;
	}

	public bool AddPoint(Vector3 point)
	{
		Vector2 vector = new Vector2(point.x, point.z);
		vector -= _startPos;
		int num = _points.Count - 1;
		Vector2 vector2 = ((!UsingFakeFirstPoint) ? _points[num] : Vector2.zero);
		float magnitude = (vector2 - vector).magnitude;
		if (magnitude <= 0.35f)
		{
			return false;
		}
		Vector2 vector3 = vector - vector2;
		if (UsingFakeFirstPoint)
		{
			_tangents[0] = vector3;
			CorrectFakeFirstPoint(vector, vector3, magnitude);
			return false;
		}
		Vector2 vector4 = (vector - _points[num - 1]) * 0.5f;
		vector4.Normalize();
		Vector2 vector5 = vector3;
		vector5.Normalize();
		float f = Vector2.Dot(vector5, vector4);
		float num2 = Mathf.Acos(f) * 57.29578f;
		if (num2 > 15f)
		{
			float num3 = (float)Math.PI / 12f;
			if (Vector3.Cross(vector5, vector4).z > 0f)
			{
				num3 *= -1f;
			}
			float x = Mathf.Cos(num3) * vector4.x - Mathf.Sin(num3) * vector4.y;
			float y = Mathf.Sin(num3) * vector4.x + Mathf.Cos(num3) * vector4.y;
			Vector2 vector6 = new Vector2(x, y);
			vector6 *= magnitude;
			Vector2 vector7 = vector2 + vector6;
			float magnitude2 = (vector7 - vector).magnitude;
			vector = vector7;
			vector3 = vector6;
			if (magnitude2 > 0.25f)
			{
				_renderingStopped = true;
			}
		}
		if (_renderingStopped)
		{
			vector = new Vector2(point.x, point.z);
			vector -= _startPos;
			vector3 = vector - _points[_points.Count - 1];
			magnitude = vector3.magnitude;
		}
		AddNewPointData(vector, vector3, magnitude);
		bool flag = _renderingStopped || NumSamples >= 40;
		if (flag)
		{
			EndSample.Flatten = true;
		}
		return flag;
	}

	public void DataAtDist(float distance, out Vector3 position, out Vector3 tangent)
	{
		if (_points.Count == 0)
		{
			position = Vector3.zero;
			tangent = Vector3.forward;
			return;
		}
		if (distance == 0f)
		{
			position = Make3D(_points[0]);
			tangent = Make3D(_tangents[0]);
			return;
		}
		if (distance >= _totalLength)
		{
			EndPointData(out position, out tangent);
			return;
		}
		int i;
		for (i = 0; i < _points.Count - 1 && _lengths[i] < distance; i++)
		{
		}
		if (_lengths[i] < distance)
		{
			Debug.LogError("Spline lengths have gone wrong");
			EndPointData(out position, out tangent);
			return;
		}
		int num = i - 1;
		float t = (distance - _lengths[num]) / (_lengths[i] - _lengths[num]);
		SetSpline(num);
		Vector2 v = _spline.PointAt(t);
		Vector2 v2 = _spline.TangentAt(t);
		position = Make3D(v);
		tangent = Make3D(v2);
	}

	private void SetSpline(int firstPointIndex)
	{
		Vector2 p = _points[firstPointIndex];
		Vector2 p2 = _points[firstPointIndex + 1];
		Vector2 r = _tangents[firstPointIndex];
		Vector2 r2 = _tangents[firstPointIndex + 1];
		_spline.Set(p, p2, r, r2);
	}

	public void EndPointData(out Vector3 position, out Vector3 tangent)
	{
		Vector2 v = _points[_points.Count - 1];
		Vector2 v2 = _tangents[_tangents.Count - 1];
		position = Make3D(v);
		tangent = Make3D(v2);
	}

	public void DebugRender()
	{
		DebugRenderSpline();
		DebugRenderSamples();
	}

	private void DebugRenderSamples()
	{
		float num = 0.1f;
		foreach (GougeSplineSample sample in Samples)
		{
			Vector3 vector = sample.Rotation * (Vector3.left * 0.25f);
			vector += sample.Position;
			vector.y += 0.5f;
			Vector3 vector2 = sample.Rotation * (Vector3.right * 0.25f);
			vector2 += sample.Position;
			vector2.y += 0.5f;
			Vector3 position = sample.Position;
			position.y += 0.5f;
			Vector3 vector3 = sample.Rotation * (new Vector3(0f, 0f, sample.Scale.z) * 0.25f);
			vector3 += sample.Position;
			vector3.y += 0.5f;
			Debug.DrawLine(vector + StartPos, vector2 + StartPos, new Color(0f, 0f, num));
			Debug.DrawLine(position + StartPos, vector3 + StartPos, new Color(0f, num, 0f));
			num += 0.1f;
			if (num > 1f)
			{
				num = 0.1f;
			}
		}
	}

	private void DebugRenderSpline()
	{
		for (int i = 0; i < _points.Count; i++)
		{
			Debug.DrawLine(Make3D(_points[i]) + StartPos, Make3D(_points[i]) + StartPos + Vector3.up, Color.red);
		}
		for (float num = 0f; num < _totalLength; num += 0.1f)
		{
			Vector3 position;
			Vector3 tangent;
			DataAtDist(num, out position, out tangent);
			position += Make3D(_startPos);
			Debug.DrawLine(position, position + Vector3.up);
		}
	}

	public bool AllPointsBelow(float worldZ)
	{
		float num = worldZ - _startPos.y;
		for (int i = 1; i < _points.Count; i++)
		{
			if (_points[i].y >= num)
			{
				return false;
			}
		}
		return true;
	}

	public float GetClosestSplineDistToPoint_WithinSamples_Downhill(Vector3 point, int startSample, int endSample, bool allowUphill)
	{
		float splineDist = Samples[startSample].SplineDist;
		splineDist -= 0.125f;
		float splineDist2 = Samples[endSample].SplineDist;
		splineDist2 += 0.125f;
		return GetClosestSplineDistToPoint_Downhill(point, splineDist, splineDist2, allowUphill);
	}

	public float GetClosestSplineDistToPoint_Downhill(Vector3 point, float startDist, float endDist, bool allowUphill)
	{
		if (endDist <= startDist)
		{
			Debug.LogError("Bad data for spline dist finction");
		}
		startDist = Mathf.Max(startDist, 0f);
		endDist = Mathf.Min(endDist, _totalLength);
		Vector3 vector = point - StartPos;
		float num = (endDist - startDist) / 10f;
		float num2 = startDist;
		float num3 = num2;
		float num4 = float.MaxValue;
		bool flag = false;
		while (num2 < endDist)
		{
			Vector3 position;
			Vector3 tangent;
			DataAtDist(num2, out position, out tangent);
			num2 += num;
			if (allowUphill || position.z > vector.z)
			{
				float sqrMagnitude = (position - vector).sqrMagnitude;
				if (sqrMagnitude < num4)
				{
					num4 = sqrMagnitude;
					num3 = num2;
					flag = true;
				}
			}
		}
		return (!flag) ? (-1f) : num3;
	}

	private void AddNewPointData(Vector2 point, Vector2 tangent, float length)
	{
		_points.Add(point);
		_tangents.Add(tangent);
		_totalLength += length;
		_lengths.Add(_totalLength);
		CalculateNewSampleData();
	}

	private Quaternion MakeSampleRotationFromTangent(Vector3 tangent)
	{
		float num = Mathf.Atan2(tangent.x, tangent.z);
		return Quaternion.AngleAxis(num * 57.29578f, Vector3.up);
	}

	public void CalculateNewSampleData()
	{
		float fingerPrintScale = CameraDirector.Instance.FingerPrintScale;
		if (Samples.Count == 0)
		{
			GougeSplineSample gougeSplineSample = new GougeSplineSample();
			gougeSplineSample.SplineDist = 0f;
			gougeSplineSample.Position = Vector3.zero;
			gougeSplineSample.Rotation = MakeSampleRotationFromTangent(Make3D(_tangents[0]));
			gougeSplineSample.Scale = new Vector3(fingerPrintScale, 1f, fingerPrintScale);
			gougeSplineSample.Flatten = !NewInput;
			Samples.Add(gougeSplineSample);
		}
		float num = Samples[Samples.Count - 1].SplineDist;
		if (Samples.Count > 1)
		{
			num += 0.125f;
		}
		float length = Length;
		Vector3 position;
		Vector3 tangent;
		for (float num2 = num + 0.25f; num2 <= length; num2 += 0.25f)
		{
			if (Samples.Count >= 40)
			{
				break;
			}
			float num3 = num2 - 0.125f;
			DataAtDist(num3, out position, out tangent);
			Vector3 scale = new Vector3(fingerPrintScale, 1f, 0.25f);
			GougeSplineSample gougeSplineSample2 = new GougeSplineSample();
			gougeSplineSample2.SplineDist = num3;
			gougeSplineSample2.Position = position;
			gougeSplineSample2.Rotation = MakeSampleRotationFromTangent(tangent);
			gougeSplineSample2.Scale = scale;
			gougeSplineSample2.Flatten = _renderingStopped;
			Samples.Add(gougeSplineSample2);
			SamplesSinceRenderBreak++;
		}
		EndPointData(out position, out tangent);
		EndSample.SplineDist = Length;
		EndSample.Position = position;
		EndSample.Rotation = MakeSampleRotationFromTangent(tangent);
		EndSample.Scale = new Vector3(fingerPrintScale, 1f, fingerPrintScale);
		EndSample.Flatten = false;
	}

	public GougeSplineSample GetNextSampleData(int splineStep)
	{
		return Samples[splineStep];
	}

	private void CropSamples(float removedLength, Vector3 moveToNewStartPos)
	{
		if (Samples.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (GougeSplineSample sample in Samples)
		{
			if (sample.SplineDist < removedLength)
			{
				num++;
				continue;
			}
			break;
		}
		if (num > 0)
		{
			Samples.RemoveRange(0, num);
		}
		foreach (GougeSplineSample sample2 in Samples)
		{
			sample2.SplineDist -= removedLength;
			sample2.Position -= moveToNewStartPos;
		}
		EndSample.SplineDist -= removedLength;
		EndSample.Position -= moveToNewStartPos;
		if (this.SplineSampleListReduced != null)
		{
			this.SplineSampleListReduced(num, moveToNewStartPos);
		}
	}

	private void InitialiseSamples()
	{
		EndSample = new GougeSplineSample();
		SamplesSinceRenderBreak = 0;
	}

	public void SetInputStartTime(float time)
	{
		_inputStartTime = time;
	}

	public void SetMaxTouchSpeed(float speed)
	{
		_maxTouchSpeed = speed;
	}

	public float GetCurrentSpeedBoost(bool ignoreDecayTime = false)
	{
		if (ignoreDecayTime)
		{
			return _maxTouchSpeed;
		}
		float num = Time.time - _inputStartTime;
		float gougeSpeedBoostDecayTime = CurrentHill.Instance.Definition._PebbleHandlingParams._GougeSpeedBoostDecayTime;
		if (num >= gougeSpeedBoostDecayTime)
		{
			return 0f;
		}
		float num2 = 1f - num / gougeSpeedBoostDecayTime;
		return num2 * _maxTouchSpeed;
	}
}
