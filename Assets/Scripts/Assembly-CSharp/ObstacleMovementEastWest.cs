using UnityEngine;

public class ObstacleMovementEastWest : ObstacleMovement
{
	public float _SpeedMPS;

	public float _TimeInOneDirectionMin;

	public float _TimeInOneDirectionMax;

	private float _nextDirectionChangeTime;

	private Vector3 VectorDirection { get; set; }

	public void Start()
	{
		ChooseNewDirection();
	}

	public override void UpdateRegularMovement()
	{
		UpdateDirection();
		Vector3 position = base.transform.position;
		Vector3 vector = VectorDirection * _SpeedMPS * Time.fixedDeltaTime;
		Vector3 position2 = position + vector;
		if (HasBreachedBounds(position2.x))
		{
			position2 = position - vector;
		}
		base.transform.position = position2;
	}

	private void UpdateDirection()
	{
		if (!(Time.time < _nextDirectionChangeTime))
		{
			ChooseNewDirection();
		}
	}

	private void ChooseNewDirection()
	{
		if (Random.Range(0, 2) == 0)
		{
			VectorDirection = Vector3.left;
			SetDirection(Heading.West);
		}
		else
		{
			VectorDirection = Vector3.right;
			SetDirection(Heading.East);
		}
		SetNewDirectionChangeTime();
	}

	private void SetNewDirectionChangeTime()
	{
		_nextDirectionChangeTime = Time.time + Random.Range(_TimeInOneDirectionMin, _TimeInOneDirectionMax);
	}
}
