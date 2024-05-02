using UnityEngine;

public class ObstacleMovement3Directions : ObstacleMovement
{
	public float _SpeedMPS;

	public Heading _StartDirection;

	public float _TimeInOneDirectionMin;

	public float _TimeInOneDirectionMax;

	private float _nextDirectionChangeTime;

	private bool _turnedLeft;

	private bool _turnedRight;

	private Vector3 VectorDirection { get; set; }

	public void Start()
	{
		Reset();
	}

	public override void UpdateRegularMovement()
	{
		UpdateDirection();
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
			ChangeDirection();
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
		bool flag;
		if (_turnedRight)
		{
			flag = true;
			_turnedRight = false;
		}
		else if (_turnedLeft)
		{
			flag = false;
			_turnedLeft = false;
		}
		else
		{
			flag = (_turnedLeft = Random.Range(0, 2) == 1);
			_turnedRight = !flag;
		}
		int num = ((!flag) ? 1 : (-1));
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

	private Vector3 VectorFromCompassPoint(Heading point)
	{
		float angle = 45 * (int)Direction;
		return Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
	}

	private void SetNewDirectionChangeTime()
	{
		_nextDirectionChangeTime = Time.time + Random.Range(_TimeInOneDirectionMin, _TimeInOneDirectionMax);
	}

	public override void OnBounce()
	{
		Reset();
	}

	private void Reset()
	{
		SetDirection(_StartDirection);
		_turnedLeft = false;
		_turnedRight = false;
	}
}
