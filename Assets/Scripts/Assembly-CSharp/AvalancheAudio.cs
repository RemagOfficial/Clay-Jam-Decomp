using Fabric;
using UnityEngine;

public class AvalancheAudio : MonoBehaviour
{
	private const string EventName = "Avalanche";

	private void Awake()
	{
		HillCollapser.StateChanged += OnStateChange;
		InGameController.PausedEvent += OnPaused;
		InGameController.UnpausedEvent += OnUnpaused;
	}

	private void OnDestroy()
	{
		HillCollapser.StateChanged -= OnStateChange;
		InGameController.PausedEvent -= OnPaused;
		InGameController.UnpausedEvent -= OnUnpaused;
	}

	private void Update()
	{
		if (!(HillCollapser.Instance == null))
		{
			float x = Pebble.Instance.transform.position.x;
			float pointOfCollapse = HillCollapser.Instance.PointOfCollapse;
			Vector3 position = new Vector3(x, 0f, pointOfCollapse);
			base.transform.position = position;
		}
	}

	private void OnStateChange(HillCollapser.State newState)
	{
		switch (newState)
		{
		case HillCollapser.State.WaitingToRoll:
			StopAudio();
			break;
		case HillCollapser.State.Rolling:
			PlayAudio();
			break;
		case HillCollapser.State.Stopped:
			StopAudio();
			break;
		case HillCollapser.State.ConsumingPebble:
			break;
		}
	}

	private void OnPaused()
	{
		PauseAudio();
	}

	private void OnUnpaused()
	{
		UnpauseAudio();
	}

	private void PlayAudio()
	{
		SendAudioEvent(EventAction.PlaySound);
	}

	private void StopAudio()
	{
		SendAudioEvent(EventAction.StopSound);
	}

	private void PauseAudio()
	{
		SendAudioEvent(EventAction.PauseSound);
	}

	private void UnpauseAudio()
	{
		if (HillCollapser.Instance.CurrentState == HillCollapser.State.Rolling)
		{
			SendAudioEvent(EventAction.PlaySound);
		}
	}

	private void SendAudioEvent(EventAction eventAction)
	{
		InGameAudio.PostFabricEvent("Avalanche", eventAction, null, base.gameObject);
	}
}
