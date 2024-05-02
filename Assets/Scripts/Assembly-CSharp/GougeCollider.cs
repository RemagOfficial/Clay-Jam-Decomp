using System.Collections.Generic;
using UnityEngine;

public class GougeCollider : MonoBehaviour
{
	private const int SamplesPerCollider = 5;

	private GougeSpline _spline;

	private int _lastSampleIndexUsed;

	private Vector3 _lastSamplePos;

	private Vector3 _splineStartLastRendered;

	private Vector3 _splineEndLastRendered;

	public GougeSpline Spline
	{
		get
		{
			return _spline;
		}
		set
		{
			_spline = value;
			_spline.SplineSampleListReduced += OnSplineSampleListReduced;
		}
	}

	private List<GougeSectionCollider> Colliders { get; set; }

	private GougeSectionCollider CurrentCollider { get; set; }

	private GougeSectionCollider EndCollider { get; set; }

	private void Awake()
	{
		Colliders = new List<GougeSectionCollider>();
		_lastSampleIndexUsed = 0;
		_lastSamplePos = base.transform.position;
	}

	private void OnDesttoy()
	{
		_spline.SplineSampleListReduced -= OnSplineSampleListReduced;
	}

	public void Update()
	{
		if (!Spline.SamplesReady || (Spline.StartPos == _splineStartLastRendered && Spline.EndPos == _splineEndLastRendered))
		{
			return;
		}
		_splineStartLastRendered = Spline.StartPos;
		_splineEndLastRendered = Spline.EndPos;
		int num = _lastSampleIndexUsed;
		Vector3 lastSamplePos = _lastSamplePos;
		int num2 = 5;
		while (num < Spline.NumSamples - 1)
		{
			num++;
			num2--;
			GougeSplineSample nextSampleData = Spline.GetNextSampleData(num);
			if (num2 == 0)
			{
				Vector3 vector = nextSampleData.Position + base.transform.position;
				GougeSectionCollider gougeSectionCollider = NewCollider(lastSamplePos, vector, _lastSampleIndexUsed, num);
				LinkLastCollider(gougeSectionCollider);
				Colliders.Add(gougeSectionCollider);
				_lastSampleIndexUsed = num;
				_lastSamplePos = vector;
				num2 = 5;
			}
		}
		AddLastCollider();
	}

	private void AddLastCollider()
	{
		Vector3 startPoint = Vector3.zero;
		GougeSplineSample nextSampleData;
		if (_lastSampleIndexUsed > 0)
		{
			nextSampleData = Spline.GetNextSampleData(_lastSampleIndexUsed);
			startPoint = nextSampleData.Position;
		}
		startPoint += base.transform.position;
		int endSample = Spline.NumSamples - 1;
		nextSampleData = Spline.EndSample;
		Vector3 position = nextSampleData.Position;
		position += base.transform.position;
		if (EndCollider == null)
		{
			EndCollider = NewCollider(startPoint, position, _lastSampleIndexUsed, endSample);
		}
		else
		{
			EndCollider.SetEndPoints(startPoint, position, _lastSampleIndexUsed, endSample);
		}
		LinkLastCollider(EndCollider);
	}

	private GougeSectionCollider NewCollider(Vector3 startPoint, Vector3 endPoint, int startSample, int endSample)
	{
		GameObject gameObject = new GameObject("GougeSection");
		gameObject.transform.parent = base.gameObject.transform;
		GougeSectionCollider gougeSectionCollider = gameObject.AddComponent<GougeSectionCollider>();
		gougeSectionCollider.Initialise();
		gougeSectionCollider.Spline = Spline;
		gougeSectionCollider.SetEndPoints(startPoint, endPoint, startSample, endSample);
		return gougeSectionCollider;
	}

	private void RemoveCollidersUpTo(int sampleIndexToRemoveUpTo, Vector3 movedBy)
	{
		int num = 0;
		foreach (GougeSectionCollider collider in Colliders)
		{
			if (collider.EndSample < sampleIndexToRemoveUpTo + 1)
			{
				Object.Destroy(collider.gameObject);
				num++;
				continue;
			}
			int num2 = Mathf.Max(0, collider.StartSample - sampleIndexToRemoveUpTo);
			collider.StartSample -= num2;
			collider.EndSample -= sampleIndexToRemoveUpTo;
			collider.gameObject.transform.position -= movedBy;
		}
		if (num != 0)
		{
			Colliders.RemoveRange(0, num);
		}
	}

	private void OnSplineSampleListReduced(int amountReducedBy, Vector3 movedBy)
	{
		RemoveCollidersUpTo(amountReducedBy, movedBy);
		_lastSampleIndexUsed -= amountReducedBy;
		if (_lastSampleIndexUsed < 0)
		{
			_lastSampleIndexUsed = 0;
		}
	}

	private void LinkLastCollider(GougeSectionCollider lastCollider)
	{
		GougeSectionCollider gougeSectionCollider = null;
		if (Colliders.Count > 1)
		{
			gougeSectionCollider = Colliders[Colliders.Count - 1];
			gougeSectionCollider.Next = lastCollider;
		}
		lastCollider.Previous = gougeSectionCollider;
		lastCollider.Next = null;
	}

	public void Die()
	{
		foreach (GougeSectionCollider collider in Colliders)
		{
			Object.Destroy(collider.gameObject);
		}
		Colliders.Clear();
	}

	public GougeSectionCollider GetFirstCollider()
	{
		return EndCollider;
	}
}
