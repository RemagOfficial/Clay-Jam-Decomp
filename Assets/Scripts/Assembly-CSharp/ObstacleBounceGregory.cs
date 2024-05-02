using UnityEngine;

public class ObstacleBounceGregory : ObstacleBounceBehaviour
{
	public float _MaxAngleDeviation = 180f;

	public override void StartBounce(Pebble pebble)
	{
		Vector3 position = _obstacle.transform.position;
		position.z += 0.5f;
		position.y = pebble.transform.position.y;
		float num = _MaxAngleDeviation * 0.5f;
		float angle = Random.Range(0f - num, num);
		pebble.transform.RotateAround(position, Vector3.up, angle);
		ObstacleBounceBehaviour.DefaultStartBounce(pebble, _obstacle);
	}

	public override bool UpdateBounce(Pebble pebble)
	{
		return true;
	}
}
