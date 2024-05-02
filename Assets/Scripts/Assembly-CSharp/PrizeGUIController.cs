using System;
using System.Runtime.CompilerServices;
using Fabric;
using UnityEngine;

[Serializable]
public class PrizeGUIController
{
	private const string _PowerPlayWonAnim = "NextPPWon";

	private const string _PowerPlayBlobWonAnim = "NextPPWonLoop";

	public UILabel _NextPrizeMetersText;

	public GameObject _ProgressBar;

	private static string ProgressAnim = "Progress";

	private AnimationState _progressAnimState;

	public UILabel _NumPowerPlaysLabel;

	public GameObject _NextPowerPlayPanel;

	public GameObject _NumPowerPlaysBlob;

	public float _TimeToCount;

	private float _metersCounted;

	private float _timeSpentCounting;

	private HillPrizeData _currentHillPrizeProgress;

	public float _TimeBetweenAudioBlips = 0.25f;

	private float _lastBarAudioPlay;

	private float TotalMetersToAdd
	{
		get
		{
			switch (CurrentGameMode.Type)
			{
			case GameModeType.Quest:
				return InGameController.Instance.MetersFlown;
			case GameModeType.MonsterLove:
				return Pebble.Instance.MaxProgress;
			default:
				return 0f;
			}
		}
	}

	[method: MethodImpl(32)]
	public static event Action AwardedPowerPlayEvent;

	public void StartCounting()
	{
		_currentHillPrizeProgress = SaveData.Instance.Prizes.DataForHill(CurrentHill.Instance.ID);
		_timeSpentCounting = 0f;
		_metersCounted = 0f;
		_progressAnimState = _ProgressBar.animation[ProgressAnim];
		_NumPowerPlaysLabel.text = Localization.PunctuatedNumber(CurrentHill.Instance.ProgressData._PowerPlaysRemaining, 99);
		_lastBarAudioPlay = 0f;
		SetProgressBarValues();
	}

	public bool UpdateCounting(bool forceFinish)
	{
		_timeSpentCounting += Time.deltaTime;
		float value = _timeSpentCounting / _TimeToCount;
		value = Mathf.Clamp01(value);
		float num = value * TotalMetersToAdd;
		if (forceFinish)
		{
			num = TotalMetersToAdd;
		}
		while (_metersCounted < num)
		{
			_currentHillPrizeProgress.AddMetersTowardsCurrentPrize();
			_metersCounted += 1f;
			if (_currentHillPrizeProgress.PrizeShouldBeAwarded())
			{
				if (PrizeGUIController.AwardedPowerPlayEvent != null)
				{
					PrizeGUIController.AwardedPowerPlayEvent();
				}
				_currentHillPrizeProgress.AwardPrize();
				_NextPowerPlayPanel.animation.Play("NextPPWon");
				_NumPowerPlaysBlob.animation.Play("NextPPWonLoop");
				InGameAudio.PostFabricEvent("Trumpet");
			}
		}
		SetProgressBarValues();
		PlayBarIncreaseAudio(_progressAnimState.normalizedTime);
		return TotalMetersToAdd <= _metersCounted;
	}

	public void SetProgressBarValues()
	{
		float num = 1f;
		if (_currentHillPrizeProgress.NextPrize == null)
		{
			_NextPrizeMetersText.text = string.Empty;
		}
		else
		{
			float num2 = _currentHillPrizeProgress.NextPrize.Beans;
			float num3 = _currentHillPrizeProgress.MetersToNextPrize();
			float val = num2 - num3;
			string format = Localization.instance.Get("PRIZE_Distance").ToUpper();
			_NextPrizeMetersText.text = string.Format(format, Localization.PunctuatedNumber(val)) + "/" + string.Format(format, Localization.PunctuatedNumber(num2));
			if (num2 > 0f)
			{
				num = num3 / num2;
			}
		}
		_progressAnimState.speed = 0f;
		_progressAnimState.normalizedTime = 1f - num;
		_progressAnimState.weight = 1f;
		_progressAnimState.enabled = true;
		_NumPowerPlaysLabel.text = Localization.PunctuatedNumber(CurrentHill.Instance.ProgressData._PowerPlaysRemaining, 99);
	}

	private void PlayBarIncreaseAudio(float barRatio)
	{
		if (!(_lastBarAudioPlay + _TimeBetweenAudioBlips > Time.time))
		{
			_lastBarAudioPlay = Time.time;
			float num = Mathf.Lerp(0.5f, 3f, barRatio);
			InGameAudio.PostFabricEvent("PrizeBar", EventAction.SetPitch, num);
			InGameAudio.PostFabricEvent("PrizeBar");
		}
	}
}
