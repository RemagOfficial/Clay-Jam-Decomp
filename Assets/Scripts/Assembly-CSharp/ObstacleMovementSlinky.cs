using UnityEngine;

public class ObstacleMovementSlinky : ObstacleMovement
{
	public enum MoveState
	{
		west = 0,
		stopped = 1,
		east = 2
	}

	public int _StopFrame;

	public float _MoveDistance;

	public float _MinStopTime;

	public float _MaxStopTime;

	public MoveState _DefaultMove;

	public bool _RandomDirectionChange;

	private AnimatedSprite _anim;

	private float _timeToStartMoving;

	private MoveState _currentMoveState;

	private bool _flipped;

	private int StopFrame
	{
		get
		{
			return _StopFrame - 1;
		}
	}

	private MoveState CurrentMoveState
	{
		get
		{
			return _currentMoveState;
		}
		set
		{
			_currentMoveState = value;
		}
	}

	private void Start()
	{
		SetDirection(Heading.Stationary);
		SetNewMoveState();
		_anim = _obstacle.Visuals.AnimatedSprite;
	}

	private void StopMoving()
	{
		CurrentMoveState = MoveState.stopped;
		_timeToStartMoving = Time.time + Random.Range(_MinStopTime, _MaxStopTime);
	}

	private void SetNewMoveState()
	{
		CurrentMoveState = _DefaultMove;
		if (_RandomDirectionChange)
		{
			if (Random.Range(0, 2) == 0)
			{
				CurrentMoveState = MoveState.east;
			}
			else
			{
				CurrentMoveState = MoveState.west;
			}
		}
		float num = ((CurrentMoveState != MoveState.east) ? (0f - _MoveDistance) : _MoveDistance);
		if (HasBreachedBounds(base.transform.position.x + num))
		{
			CurrentMoveState = ((CurrentMoveState != MoveState.east) ? MoveState.east : MoveState.west);
		}
	}

	public override void UpdateRegularMovement()
	{
	}

	public void LateUpdate()
	{
		Vector3 position = base.transform.position;
		switch (CurrentMoveState)
		{
		case MoveState.east:
			_anim.PlayForward();
			if (!_flipped)
			{
				if (_anim.CurrentFrame == 0)
				{
					position.x += _MoveDistance;
					_flipped = true;
				}
			}
			else if (_anim.CurrentFrame == StopFrame)
			{
				StopMoving();
			}
			break;
		case MoveState.west:
			_anim.PlayBackward();
			if (!_flipped)
			{
				if (_anim.CurrentFrame == 0)
				{
					position.x -= _MoveDistance;
					_flipped = true;
				}
			}
			else if (_anim.CurrentFrame == _anim.LastFrame - StopFrame)
			{
				StopMoving();
			}
			break;
		case MoveState.stopped:
			_anim.Stop();
			_flipped = false;
			if (Time.time > _timeToStartMoving)
			{
				SetNewMoveState();
			}
			break;
		}
		base.transform.position = position;
	}
}
