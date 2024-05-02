using System;

[Serializable]
public class TutorialSideEffects
{
	public bool _PauseGame;

	public bool _FreezeSpinner;

	public bool _StartQuests;

	public bool _AwardPowerPlay;

	public void OnShow()
	{
		if (_PauseGame)
		{
			InGameController.Instance.Pause();
		}
		if (_FreezeSpinner)
		{
			JVPController.Instance.FreezeSpinner();
		}
		if (_StartQuests)
		{
			SaveData.Instance.Progress._QuestsStarted.Set = true;
		}
	}

	public void OnHide()
	{
		if (_PauseGame)
		{
			InGameController.Instance.Unpause();
		}
		if (_FreezeSpinner)
		{
			JVPController.Instance.UnFreezeSpinner();
		}
		if (_AwardPowerPlay)
		{
			CurrentHill.Instance.ProgressData._PowerPlaysRemaining++;
		}
	}
}
