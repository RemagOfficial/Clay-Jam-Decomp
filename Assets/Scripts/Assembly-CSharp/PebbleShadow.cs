using System;
using UnityEngine;

public class PebbleShadow : MonoBehaviour
{
	public float _SunAngle;

	private float _sunAngleTan;

	private Vector3 _offset;

	private Pebble _pebble;

	private void Awake()
	{
		_offset = base.transform.position;
		_sunAngleTan = Mathf.Tan(_SunAngle * ((float)Math.PI / 180f));
	}

	private void Start()
	{
		_pebble = Pebble.Instance;
	}

	private void Update()
	{
		if (_pebble.Launched)
		{
			base.gameObject.SetActiveRecursively(false);
			return;
		}
		float heightOffGround = _pebble.HeightOffGround;
		Vector3 position = _pebble.transform.position;
		float num = heightOffGround * _sunAngleTan;
		position.x += num;
		position.z += num;
		position.y = 0.05f;
		base.transform.rotation = Quaternion.identity;
		position += _offset;
		base.transform.position = position;
		base.transform.localScale = _pebble.transform.localScale;
	}
}
