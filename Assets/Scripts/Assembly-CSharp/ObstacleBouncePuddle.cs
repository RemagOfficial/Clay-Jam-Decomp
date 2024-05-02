using UnityEngine;

public class ObstacleBouncePuddle : ObstacleBounceBehaviour
{
	public float _SpeedMultipler = 0.5f;

	private float _originalSpeed;

	private Vector3 _originalDir;

	public override void StartBounce(Pebble pebble)
	{
		_originalSpeed = pebble.Speed;
		_originalDir = pebble.Direction;
	}

	public override bool UpdateBounce(Pebble pebble)
	{
		if (_obstacle.TouchesPebble)
		{
			pebble.Velocity = _originalSpeed * _SpeedMultipler * _originalDir;
			return false;
		}
		return true;
	}
}
