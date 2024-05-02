using UnityEngine;

public class EndScreen : MonoBehaviour
{
	private ClayCollection ClayCollected { get; set; }

	private void Start()
	{
		InGameController.StateChanged += OnStateChanged;
		base.enabled = false;
	}

	private void OnStateChanged(InGameController.State state)
	{
		if (state == InGameController.State.Landed)
		{
			base.enabled = true;
			ClayCollected = InGameController.Instance.ClayCollectedThisRun;
			PlayerPrefs.SetInt("HighScore", InGameController.Instance.MetersFlown);
		}
		else
		{
			base.enabled = false;
		}
	}
}
