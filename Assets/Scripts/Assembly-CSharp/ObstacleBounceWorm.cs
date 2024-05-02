using System.Collections.Generic;
using UnityEngine;

public class ObstacleBounceWorm : ObstacleBounceBehaviour
{
	private const float SpinRevs = 4f;

	public float _BounceTime = 1f;

	public float _BounceDistance = 2f;

	public float _BounceHeight = 5f;

	public List<int> _BigBounceFrames;

	private Vector3 _bounceDir;

	private Vector3 _bounceCross;

	private float _timeInState;

	private float _speed;

	private bool _useDefaultBounce;

	public override void StartBounce(Pebble pebble)
	{
		SpriteAnimationData animData = _obstacle.Visuals.AnimatedSprite.GetAnimData("Default");
		int currentFrame = animData.CurrentFrame;
		_useDefaultBounce = !_BigBounceFrames.Contains(currentFrame);
		if (_useDefaultBounce)
		{
			ObstacleBounceBehaviour.DefaultStartBounce(pebble, _obstacle);
		}
		else
		{
			StartBigBounce();
		}
	}

	private void StartBigBounce()
	{
		_bounceDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
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
		CameraDirector.Instance.StartShake();
	}

	public override bool UpdateBounce(Pebble pebble)
	{
		if (_useDefaultBounce)
		{
			return ObstacleBounceBehaviour.DefaultBounceUpdate(pebble, _obstacle);
		}
		return UpdateBigBounce(pebble);
	}

	private bool UpdateBigBounce(Pebble pebble)
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
		pebble.SpinSpeedOverride = 4f;
		_timeInState += Time.fixedDeltaTime;
		return _timeInState >= _BounceTime;
	}
}
