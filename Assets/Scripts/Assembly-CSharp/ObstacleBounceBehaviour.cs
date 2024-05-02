using UnityEngine;

public abstract class ObstacleBounceBehaviour : MonoBehaviour
{
	protected ObstacleMould _obstacle;

	private void Awake()
	{
		_obstacle = GetComponent<ObstacleMould>();
		_obstacle.SetBounceBehaviour(this);
	}

	public abstract void StartBounce(Pebble pebble);

	public abstract bool UpdateBounce(Pebble pebble);

	public static void DefaultStartBounce(Pebble pebble, ObstacleMould obstacle)
	{
		Vector3 vector = BounceDir(pebble.transform.position, obstacle.transform.position);
		float num = obstacle.Definition._BounceScale;
		if (Mathf.Abs(num) < 0.01f)
		{
			num = 0.01f;
		}
		float num2 = pebble.Speed * num;
		float maxSpeed = CurrentHill.Instance.Definition._PebbleHandlingParams.MaxSpeed;
		if (num2 > maxSpeed)
		{
			num2 = maxSpeed;
		}
		Vector3 velocity = vector * num2;
		pebble.Velocity = velocity;
		CameraDirector.Instance.StartShake();
	}

	public static Vector3 BounceDir(Vector3 obj1, Vector3 obj2)
	{
		Vector3 result = obj1 - obj2;
		result.y = 0f;
		result.Normalize();
		return result;
	}

	public static bool DefaultBounceUpdate(Pebble pebble, ObstacleMould obstacle)
	{
		return true;
	}
}
