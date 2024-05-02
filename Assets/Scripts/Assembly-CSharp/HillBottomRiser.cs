using UnityEngine;

public class HillBottomRiser : MonoBehaviour
{
	public float _StartHeight;

	public float _EndHeight;

	private float _totalHeightToRise;

	private float _progressToStopRising;

	private float _totalDistanceToRiseOver;

	private float DistToStopRising
	{
		get
		{
			return _progressToStopRising - Pebble.ProgressSafe;
		}
	}

	public void ResetForRun()
	{
		_progressToStopRising = HillBottomController.BossMonsterTransform.position.z;
		_totalHeightToRise = _EndHeight - _StartHeight;
		_totalDistanceToRiseOver = _progressToStopRising - CurrentHill.Instance.ShowHorizonProgress;
		SetHeight(_StartHeight);
	}

	private void Update()
	{
		if (InGameController.Instance.CurrentState == InGameController.State.RollingApproach)
		{
			float num = 1f - DistToStopRising / _totalDistanceToRiseOver;
			float height = _StartHeight + _totalHeightToRise * num;
			SetHeight(height);
		}
	}

	private void SetHeight(float height)
	{
		Vector3 position = base.transform.position;
		position.y = height;
		base.transform.position = position;
	}
}
