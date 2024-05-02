using Fabric;
using UnityEngine;

public class PebbleFlame : MonoBehaviour
{
	public bool _FlickEffect;

	private Vector3 _offset;

	private Pebble _pebble;

	private void Awake()
	{
		_offset = base.transform.position;
	}

	private void Start()
	{
		_pebble = Pebble.Instance;
	}

	private void OnEnable()
	{
		InGameAudio.PostFabricEvent("FlameOn", EventAction.PlaySound);
		InGameAudio.PostFabricEvent("FlameLoop", EventAction.PlaySound);
	}

	private void OnDisable()
	{
		if (!(EventManager.Instance == null))
		{
			InGameAudio.PostFabricEvent("FlameLoop", EventAction.StopSound);
			InGameAudio.PostFabricEvent("FlameFade", EventAction.PlaySound);
		}
	}

	private void Update()
	{
		if (_pebble == null)
		{
			return;
		}
		if (!_FlickEffect && _pebble.Launched)
		{
			base.gameObject.SetActiveRecursively(false);
			return;
		}
		Vector3 position = _pebble.transform.position;
		if (_FlickEffect)
		{
			position.y -= _pebble.RadiusMeters;
		}
		else
		{
			position.y = 0.05f;
		}
		base.transform.rotation = Quaternion.LookRotation(_pebble.Direction);
		position += _offset;
		base.transform.position = position;
		base.transform.localScale = _pebble.transform.localScale;
	}
}
