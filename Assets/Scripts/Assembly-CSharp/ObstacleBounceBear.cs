using UnityEngine;

public class ObstacleBounceBear : ObstacleBounceBehaviour
{
	private enum State
	{
		Squashing = 0,
		Squashed = 1,
		Releasing = 2,
		Released = 3
	}

	public Vector3 _SquashOffset = new Vector3(0f, 0f, 0.5f);

	public float _SquashTime = 1f;

	public float _SquashDepth = 0.7f;

	private State _state;

	private float _timeBeingSquashed;

	private Vector3 _squashPos;

	private Vector3 _originalVelocity;

	public override void StartBounce(Pebble pebble)
	{
		_state = State.Squashing;
		_squashPos = pebble.transform.position;
		_originalVelocity = pebble.Velocity;
		CameraDirector.Instance.StartShake();
	}

	public override bool UpdateBounce(Pebble pebble)
	{
		switch (_state)
		{
		case State.Squashing:
			if (UpdateSquashing(pebble))
			{
				_timeBeingSquashed = 0f;
				_state = State.Squashed;
			}
			return false;
		case State.Squashed:
			if (UpdateSquashed(pebble))
			{
				_state = State.Releasing;
			}
			return false;
		case State.Releasing:
			if (UpdateReleasing(pebble))
			{
				_state = State.Released;
				return true;
			}
			return false;
		case State.Released:
			return true;
		default:
			return true;
		}
	}

	private bool UpdateSquashing(Pebble pebble)
	{
		pebble.transform.position = _squashPos;
		pebble.SetHeight(pebble.RadiusMeters * (0f - _SquashDepth) * 2f);
		pebble.Velocity = Vector3.zero;
		return true;
	}

	private bool UpdateSquashed(Pebble pebble)
	{
		_timeBeingSquashed += Time.fixedDeltaTime;
		if (_timeBeingSquashed > _SquashTime)
		{
			return true;
		}
		return false;
	}

	private bool UpdateReleasing(Pebble pebble)
	{
		pebble.ResetScale();
		pebble.StickToHill();
		pebble.Velocity = _originalVelocity;
		return true;
	}
}
