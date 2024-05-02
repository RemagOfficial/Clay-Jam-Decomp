using Fabric;
using UnityEngine;

public class ObstacleBounceGroundMouth : ObstacleBounceBehaviour
{
	private enum State
	{
		Sucking = 0,
		Nibbling = 1,
		Flung = 2
	}

	private const float LoseClayGap = 0.25f;

	public Vector3 _NibblePos;

	public float _SuckSpeed = 10f;

	public float _NibbleTimeMin = 0.5f;

	public float _NibbleTimeMax = 1f;

	public float _FlingSpeed = 10f;

	public float _ClayToNibblePerSecond = 5f;

	private State _state;

	private float _timeNibbling;

	private float _timeToNibble;

	private float _nextLoseClayTime;

	private float _lastFlingTime;

	public override void StartBounce(Pebble pebble)
	{
		if (Time.time - 0.25f < _lastFlingTime)
		{
			_state = State.Flung;
			return;
		}
		_state = State.Sucking;
		Suck(pebble);
	}

	public override bool UpdateBounce(Pebble pebble)
	{
		switch (_state)
		{
		case State.Sucking:
			if (Suck(pebble))
			{
				StartNibbling(pebble);
			}
			return false;
		case State.Nibbling:
			if (UpdateNibbling(pebble))
			{
				Fling(pebble);
				return true;
			}
			return false;
		case State.Flung:
			return true;
		default:
			return true;
		}
	}

	private void StartNibbling(Pebble pebble)
	{
		pebble.transform.rotation = Quaternion.identity;
		_state = State.Nibbling;
		_timeNibbling = 0f;
		_timeToNibble = Random.Range(_NibbleTimeMin, _NibbleTimeMax);
		_nextLoseClayTime = 0.25f;
		pebble.LoseClayFromBounce(_ClayToNibblePerSecond * 0.25f);
		InGameAudio.PostFabricEvent("MouthMunch", EventAction.PlaySound);
	}

	private bool UpdateNibbling(Pebble pebble)
	{
		_timeNibbling += Time.fixedDeltaTime;
		if (_timeNibbling > _timeToNibble)
		{
			return true;
		}
		pebble.transform.RotateAround(Vector3.up, 360f * Time.fixedDeltaTime);
		if (_timeNibbling > _nextLoseClayTime)
		{
			_nextLoseClayTime += 0.25f;
			pebble.LoseClayFromBounce(_ClayToNibblePerSecond * 0.25f);
		}
		return false;
	}

	private bool Suck(Pebble pebble)
	{
		Vector3 vector = _obstacle.transform.position + _NibblePos;
		Vector3 vector2 = vector - pebble.transform.position;
		float num = _SuckSpeed * Time.fixedDeltaTime;
		if (vector2.magnitude < num)
		{
			pebble.transform.position = vector;
			pebble.Velocity = Vector3.zero;
			return true;
		}
		vector2.Normalize();
		pebble.Velocity = vector2 * _SuckSpeed;
		return false;
	}

	private void Fling(Pebble pebble)
	{
		pebble.Velocity = pebble.transform.forward * _FlingSpeed;
		_lastFlingTime = Time.time;
	}
}
