using UnityEngine;

public class SpeedoGUI : MonoBehaviour
{
	public Vector3 MinSpeedRotation;

	public Vector3 MaxSpeedRotation;

	private float _maxSpeed;

	private float _minSpeed;

	private void Start()
	{
		_minSpeed = 0f;
		_maxSpeed = CurrentHill.Instance.Definition._PebbleHandlingParams.MaxSpeed;
	}

	private void Update()
	{
		float downhillSpeed = Pebble.Instance.DownhillSpeed;
		downhillSpeed = Mathf.Max(downhillSpeed, 0f);
		float t = (downhillSpeed - _minSpeed) / (_maxSpeed - _minSpeed);
		Vector3 euler = Vector3.Lerp(MinSpeedRotation, MaxSpeedRotation, t);
		base.transform.rotation = Quaternion.Euler(euler);
	}
}
