using UnityEngine;

public class ObstacleBounceRainbow : ObstacleBounceBehaviour
{
	public float _boostScale = 2f;

	private bool _throughMiddle;

	public override void StartBounce(Pebble pebble)
	{
		_throughMiddle = _obstacle.TouchColliderIndex == 0;
		if (!_throughMiddle)
		{
			ObstacleBounceBehaviour.DefaultStartBounce(pebble, _obstacle);
		}
		else
		{
			pebble.Velocity = Vector3.forward * pebble.Speed * _boostScale;
		}
	}

	public override bool UpdateBounce(Pebble pebble)
	{
		if (!_throughMiddle)
		{
			return ObstacleBounceBehaviour.DefaultBounceUpdate(pebble, _obstacle);
		}
		return true;
	}
}
