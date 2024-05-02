using UnityEngine;

public class ObstacleBouncePuffer : ObstacleBounceBehaviour
{
	private const float SpinRevs = 2f;

	public float _BounceTime = 1f;

	public float _BounceDistance = 3f;

	public float _BounceHeight = 3f;

	private Vector3 _bounceDir;

	private Vector3 _bounceCross;

	private float _timeInState;

	private float _speed;

	public override void StartBounce(Pebble pebble)
	{
		_bounceDir = pebble.Direction;
		if (_bounceDir.sqrMagnitude == 0f)
		{
			_bounceDir = Vector3.forward;
		}
		else
		{
			_bounceDir.Normalize();
		}
		_bounceCross = Vector3.Cross(Vector3.up, _bounceDir);
		_timeInState = 0f;
		_speed = _BounceDistance / _BounceTime;
	}

	public override bool UpdateBounce(Pebble pebble)
	{
		pebble.Velocity = _bounceDir * _speed;
		float num = _timeInState / _BounceTime;
		num *= 2f;
		if (num > 1f)
		{
			num = 2f - num;
		}
		num = 1f - num;
		num *= num;
		num = 1f - num;
		float height = _BounceHeight * num;
		pebble.SetHeight(height);
		pebble.SpinAxis = _bounceCross;
		pebble.SpinSpeedOverride = 2f;
		_timeInState += Time.fixedDeltaTime;
		return _timeInState >= _BounceTime;
	}
}
