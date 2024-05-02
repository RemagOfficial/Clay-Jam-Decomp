using UnityEngine;

public class AudioListenerController : MonoBehaviour
{
	private void Awake()
	{
		InGameController.StateChanged += OnInGameStateChanged;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnInGameStateChanged;
	}

	private void Update()
	{
		Vector3 position = Pebble.Instance.transform.position;
		position.y = 0f;
		base.transform.position = position;
	}

	private void OnInGameStateChanged(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.NotStarted:
			base.enabled = false;
			break;
		case InGameController.State.RollingTop:
			base.enabled = true;
			break;
		}
	}
}
