using System;
using Fabric;
using UnityEngine;

[Serializable]
public class ClayCounterGUIController
{
	private enum State
	{
		StartSpinning = 0,
		Spinning = 1,
		Done = 2
	}

	private const float MinTimePerParticle = 0.1f;

	public UISpriteAnimationControlled _PebbleSpriteAnim;

	public Animation _PebbleObjectAnimation;

	private static string PebbleSpinStartAnim = "PebbleSpinStart";

	private static string PebbleSpinLoopAnim = "PebbleSpinLoop";

	private static string PebbleSpinEndAnim = "PebbleSpinEnd";

	private UICurrentClayCollectionLabel _thisRunCollectedClayAmountLabel;

	public UILabel _CollectedClayAmount;

	private UIClayCollectionLabel _totalCollectedClayAmountLabel;

	public UILabel _TotalClayAmount;

	public float _ClayCountedPerSecond;

	public float _MinCountTime;

	public float _MaxCountTime;

	private float _totalTimeToSpin;

	private float _timeToSpin;

	private float _timePerClayPiece;

	private float _timeToLoseNextClayPiece;

	private float _timeToClayParticle;

	private float _clayToCollect;

	private State CurrentState;

	private bool _audioPlaying;

	public static bool IsCounting { get; private set; }

	public bool FinishedCounting
	{
		get
		{
			return CurrentState == State.Done;
		}
	}

	public void Start()
	{
		_PebbleSpriteAnim._Animation._Repeat = false;
		PebbleSpinStart();
		_clayToCollect = InGameController.Instance.ClayCollectedThisRun.Amount(0);
		IsCounting = true;
	}

	public void GetReady()
	{
		_thisRunCollectedClayAmountLabel = _CollectedClayAmount.GetComponent<UICurrentClayCollectionLabel>();
		_totalCollectedClayAmountLabel = _TotalClayAmount.GetComponent<UIClayCollectionLabel>();
		_clayToCollect = InGameController.Instance.ClayCollectedThisRun.Amount(0);
		_thisRunCollectedClayAmountLabel.enabled = false;
		float num = _clayToCollect;
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			num /= CurrentHill.Instance.ClayPerHeart;
			num = Mathf.Ceil(num);
		}
		_CollectedClayAmount.text = string.Format("{0:0}", Localization.PunctuatedNumber(num));
		_totalCollectedClayAmountLabel.enabled = false;
		float num2 = SaveData.Instance.ClayCollected.Amount(0);
		_TotalClayAmount.text = string.Format("{0:0}", Localization.PunctuatedNumber(num2 - _clayToCollect));
	}

	public void Finish()
	{
		_clayToCollect = 0f;
		_PebbleSpriteAnim.GoToLastFrame();
		_PebbleObjectAnimation.Play(PebbleSpinEndAnim);
		AnimationState animationState = _PebbleObjectAnimation[PebbleSpinEndAnim];
		animationState.normalizedTime = 1f;
		JVPController.Instance.EnableCollectingClayParticles(false);
		CurrentState = State.Done;
		StopAudio();
		IsCounting = false;
		SetCountedAmounts();
	}

	public void SetCountedAmounts()
	{
		_CollectedClayAmount.text = string.Format("{0:0}", Localization.PunctuatedNumber(0f));
		_TotalClayAmount.text = string.Format("{0:0}", Localization.PunctuatedNumber(SaveData.Instance.ClayCollected.Amount(0)));
	}

	public void Update()
	{
		switch (CurrentState)
		{
		case State.StartSpinning:
			if (!_PebbleObjectAnimation.isPlaying)
			{
				PebbleSpinStartLoop();
			}
			break;
		case State.Spinning:
			UpdateSpinning();
			break;
		}
		SetClayTexts();
	}

	private void PebbleSpinStart()
	{
		_PebbleObjectAnimation.Play(PebbleSpinStartAnim);
		_PebbleSpriteAnim.Rewind();
		_clayToCollect = InGameController.Instance.ClayCollectedThisRun.Amount(0);
		_totalTimeToSpin = _clayToCollect / _ClayCountedPerSecond;
		_totalTimeToSpin = Mathf.Clamp(_totalTimeToSpin, _MinCountTime, _MaxCountTime);
		_timeToSpin = _totalTimeToSpin;
		int numFrames = _PebbleSpriteAnim.NumFrames;
		_timePerClayPiece = _timeToSpin / (float)numFrames;
		_timeToLoseNextClayPiece = _timePerClayPiece;
		CurrentState = State.StartSpinning;
	}

	private void PebbleSpinStartLoop()
	{
		_PebbleObjectAnimation.Play(PebbleSpinLoopAnim);
		CurrentState = State.Spinning;
		StartAudio();
	}

	private void UpdateSpinning()
	{
		_timeToSpin -= Time.deltaTime;
		if (_timeToSpin < 0.5f)
		{
			StopAudio();
		}
		if (_timeToSpin <= 0f)
		{
			FinishSpinning();
		}
		_timeToLoseNextClayPiece -= Time.deltaTime;
		while (_timeToLoseNextClayPiece < 0f)
		{
			LoseClayPiece();
			_timeToLoseNextClayPiece += _timePerClayPiece;
		}
		_timeToClayParticle -= Time.deltaTime;
		if (_timeToClayParticle < 0f && _timeToSpin > 0f)
		{
			JVPController.Instance.EnableCollectingClayParticles(true);
			_timeToClayParticle = Mathf.Min(0.1f, _timeToLoseNextClayPiece);
		}
	}

	private void FinishSpinning()
	{
		_PebbleObjectAnimation.Play(PebbleSpinEndAnim);
		Finish();
	}

	private void JVPFinishAnimCompleted()
	{
	}

	private void SetClayTexts()
	{
		float value = 0f;
		if (_timeToSpin > 0f && _totalTimeToSpin > 0f)
		{
			value = _timeToSpin / _totalTimeToSpin;
		}
		value = Mathf.Clamp(value, 0f, 1f);
		float num = SaveData.Instance.ClayCollected.Amount(0);
		float num2 = _clayToCollect * value;
		float val = num - num2;
		_thisRunCollectedClayAmountLabel.enabled = false;
		_CollectedClayAmount.text = string.Format("{0:0}", Localization.PunctuatedNumber(num2));
		_totalCollectedClayAmountLabel.enabled = false;
		_TotalClayAmount.text = string.Format("{0:0}", Localization.PunctuatedNumber(val));
	}

	private void LoseClayPiece()
	{
		_PebbleSpriteAnim.NextFrame();
	}

	private void StopAudio()
	{
		if (_audioPlaying)
		{
			InGameAudio.PostFabricEvent("CountClay", EventAction.StopSound);
		}
		_audioPlaying = false;
	}

	private void StartAudio()
	{
		_audioPlaying = true;
		InGameAudio.PostFabricEvent("CountClay");
	}
}
