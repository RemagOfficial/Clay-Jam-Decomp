using UnityEngine;

[RequireComponent(typeof(ObstacleMould))]
public abstract class ObstacleMovement : MonoBehaviour
{
	public enum Heading
	{
		North = 0,
		NorthEast = 1,
		East = 2,
		SouthEast = 3,
		South = 4,
		SouthWest = 5,
		West = 6,
		NorthWest = 7,
		Stationary = 8,
		NumHeadings = 9
	}

	protected ObstacleMould _obstacle;

	protected Heading Direction;

	protected float _hillHalfWidth
	{
		get
		{
			return CurrentHill.Instance.Definition._HillHalfWidth;
		}
	}

	private void Awake()
	{
		_obstacle = GetComponent<ObstacleMould>();
	}

	public static string AnimationName(Heading direction)
	{
		switch (direction)
		{
		case Heading.North:
			return "North";
		case Heading.NorthEast:
			return "NorthEast";
		case Heading.East:
			return "East";
		case Heading.SouthEast:
			return "SouthEast";
		case Heading.South:
			return "South";
		case Heading.SouthWest:
			return "SouthWest";
		case Heading.West:
			return "West";
		case Heading.NorthWest:
			return "NorthWest";
		case Heading.Stationary:
			return "Default";
		default:
			return "Default";
		}
	}

	public void SetDirection(Heading direction)
	{
		Direction = direction;
		_obstacle.SetMovementAnimation(AnimationName(Direction));
	}

	public abstract void UpdateRegularMovement();

	public virtual void OnBounce()
	{
	}

	public virtual void SetStartPosition(Vector3 pos)
	{
	}

	protected bool HasBreachedBounds(float xPos)
	{
		if (_hillHalfWidth <= 0f)
		{
			return false;
		}
		return xPos - _obstacle.CollisionRadiusMetres < 0f - _hillHalfWidth || xPos + _obstacle.CollisionRadiusMetres > _hillHalfWidth;
	}
}
