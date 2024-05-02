using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ZoomLevel : ManagedComponent
{
	public float[] _MaxRadiusPerZoom;

	private bool _forcedToMax;

	public static ZoomLevel Instance { get; private set; }

	public int TotalLevels
	{
		get
		{
			return _MaxRadiusPerZoom.Length + 1;
		}
	}

	public int CurrentZoomLevel { get; private set; }

	public float CurrentMaxRadius
	{
		get
		{
			if (CurrentZoomLevel >= _MaxRadiusPerZoom.Length)
			{
				return float.MaxValue;
			}
			return _MaxRadiusPerZoom[CurrentZoomLevel];
		}
	}

	public float CurrentMinRadius
	{
		get
		{
			if (CurrentZoomLevel == 0)
			{
				return float.MinValue;
			}
			return _MaxRadiusPerZoom[CurrentZoomLevel - 1];
		}
	}

	[method: MethodImpl(32)]
	public static event Action<int, bool> NewZoomLevelEvent;

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("Multiple instances of ZoomLevel", base.gameObject);
		}
		Instance = this;
	}

	private void Update()
	{
		float radiusMeters = Pebble.Instance.RadiusMeters;
		if (radiusMeters > CurrentMaxRadius)
		{
			ZoomOut();
		}
		else if (radiusMeters < CurrentMinRadius)
		{
			ZoomIn();
		}
	}

	public override void ResetForRun()
	{
		CurrentZoomLevel = 0;
		_forcedToMax = false;
	}

	private void ZoomOut()
	{
		if (CurrentZoomLevel < _MaxRadiusPerZoom.Length)
		{
			CurrentZoomLevel++;
			if (ZoomLevel.NewZoomLevelEvent != null)
			{
				ZoomLevel.NewZoomLevelEvent(CurrentZoomLevel, true);
			}
		}
	}

	private void ZoomIn()
	{
		if (!_forcedToMax && CurrentZoomLevel != 0)
		{
			CurrentZoomLevel--;
			if (ZoomLevel.NewZoomLevelEvent != null)
			{
				ZoomLevel.NewZoomLevelEvent(CurrentZoomLevel, false);
			}
		}
	}

	public void ForceMaxZoom()
	{
		_forcedToMax = true;
		CurrentZoomLevel = _MaxRadiusPerZoom.Length;
		if (ZoomLevel.NewZoomLevelEvent != null)
		{
			ZoomLevel.NewZoomLevelEvent(CurrentZoomLevel, true);
		}
	}
}
