using UnityEngine;

public class ObstacleBouncePoopie : ObstacleBounceBehaviour
{
	public float SuckSpeed = 10f;

	public float PoopSpeed = 10f;

	private float _lastPoopTime;

	public override void StartBounce(Pebble pebble)
	{
		if (!(Time.time - 2f < _lastPoopTime))
		{
			Suck(pebble);
		}
	}

	public override bool UpdateBounce(Pebble pebble)
	{
		if (Suck(pebble))
		{
			Poop(pebble);
			return true;
		}
		return false;
	}

	private bool Suck(Pebble pebble)
	{
		Vector3 vector = _obstacle.transform.position - pebble.transform.position;
		float num = SuckSpeed * Time.fixedDeltaTime;
		if (vector.magnitude < num)
		{
			pebble.transform.position = _obstacle.transform.position;
			pebble.Velocity = Vector3.zero;
			return true;
		}
		vector.Normalize();
		pebble.Velocity = vector * SuckSpeed;
		return false;
	}

	private void Poop(Pebble pebble)
	{
		pebble.Velocity = Vector3.forward * PoopSpeed;
		_lastPoopTime = Time.time;
	}
}
