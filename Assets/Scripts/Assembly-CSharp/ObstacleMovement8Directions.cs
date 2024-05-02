using UnityEngine;

public class ObstacleMovement8Directions : ObstacleMovement
{
	public float _SpeedMPS;

	public float _TimeInOneDirectionMin;

	public float _TimeInOneDirectionMax;

	public bool _CanChangeDirection;

	public Heading _StartDirection;

	public bool _RandomStartDirection = true;

	public float _MaximumRadius = -1f;

	private float _nextDirectionChangeTime;

	private Vector3 _originalPosition;

	private float _maxRadiusSq;

	private Vector3 VectorDirection { get; set; }

	public void Start()
	{
		if (_RandomStartDirection)
		{
			int direction = Random.Range(0, 8);
			SetDirection((Heading)direction);
		}
		else
		{
			SetDirection(_StartDirection);
		}
		VectorDirection = VectorFromCompassPoint(Direction);
		_maxRadiusSq = _MaximumRadius * _MaximumRadius;
	}

	public override void UpdateRegularMovement()
	{
		if (IsOutOfRange())
		{
			TurnAround();
		}
		else if (_CanChangeDirection)
		{
			UpdateDirection();
		}
		Vector3 position = base.transform.position;
		Vector3 vector = VectorDirection * _SpeedMPS * Time.fixedDeltaTime;
		Vector3 position2 = position + vector;
		if (HasBreachedBounds(position2.x))
		{
			if (position2.x + _obstacle.CollisionRadiusMetres > base._hillHalfWidth)
			{
				position2.x = base._hillHalfWidth - _obstacle.CollisionRadiusMetres;
			}
			else
			{
				position2.x = 0f - base._hillHalfWidth + _obstacle.CollisionRadiusMetres;
			}
			TurnAround();
		}
		base.transform.position = position2;
	}

	private void UpdateDirection()
	{
		if (!(Time.time < _nextDirectionChangeTime))
		{
			ChangeDirection();
		}
	}

	private void ChangeDirection()
	{
		int num = ((Random.Range(0, 2) != 1) ? 1 : (-1));
		int num2 = (int)(Direction + num);
		if (num2 < 0)
		{
			num2 = 7;
		}
		if (num2 > 7)
		{
			num2 = 0;
		}
		SetDirection((Heading)num2);
		VectorDirection = VectorFromCompassPoint(Direction);
		SetNewDirectionChangeTime();
	}

	private void TurnAround()
	{
		int i;
		for (i = (int)(Direction + 4); i < 0; i += 8)
		{
		}
		while (i > 7)
		{
			i -= 8;
		}
		SetDirection((Heading)i);
		VectorDirection = VectorFromCompassPoint(Direction);
	}

	private Vector3 VectorFromCompassPoint(Heading point)
	{
		float angle = 45 * (int)Direction;
		return Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
	}

	private void SetNewDirectionChangeTime()
	{
		_nextDirectionChangeTime = Time.time + Random.Range(_TimeInOneDirectionMin, _TimeInOneDirectionMax);
	}

	private bool IsOutOfRange()
	{
		float sqrMagnitude = (base.transform.position - _originalPosition).sqrMagnitude;
		if (_MaximumRadius > 0f && sqrMagnitude > _maxRadiusSq)
		{
			return true;
		}
		return false;
	}

	public override void SetStartPosition(Vector3 pos)
	{
		_originalPosition = pos;
	}
}
