using Fabric;
using UnityEngine;

public class MonsterLoveGUIController : MonoBehaviour
{
	private enum State
	{
		Idle = 0,
		WaitingToShowHeartsMatch = 1
	}

	private const string HeartsUIAnimStart = "HeartUI_In";

	private const string HeartsUIAnimConvert = "HeartUI_Out";

	private const string HeartsUIAnimIdle = "HeartUI_Finished";

	private const string HeartsUIAnimTransOut = "HeartUI_SlideOff";

	private const string HeartUIAnimIdle = "HeartUI_Idle";

	private const string HeartUIAnimCollect = "HeartUI_Collected";

	private const string HeartUIAnimBreak = "HeartUI_break";

	private const string HeartUIAnimEmpty = "HeartUI_empty";

	private const string HeartUIAnimShrink = "HeartUI_shrink";

	private const string HeartUIAnimScoreCombined = "HeartUI_combined";

	private const string HeartUIAnimScoreIdle = "HeartUI_combinedIdle";

	private const string HeartUIAnimScoreOut = "HeartUI_combinedOut";

	public GameObject _HeartsCollectedCounter;

	public GameObject _Cupid;

	public AnimatedSprite _CupidAnimatedSprite;

	public GameObject _CupidConverterBlob;

	public UILabel _CupidConversionNumber;

	public GameObject[] _Hearts;

	public Animation _MatchHeartsAnimation_Particles;

	public Animation _MatchHeartsAnimation_Score;

	public UILabel _MatchedHeartsScoreLabel;

	public UILabel _ClayLabel;

	public UILabel _ConvertValueLabel;

	public GameObject _SuperCupidUpgradeMarker;

	private float _displayClay;

	private float _displayConvertValue;

	private float _clayConvertPerSecond;

	private float _timeToConvert = 2f;

	private UILabel[] _heartLabels;

	private State _currentState;

	private float _showHeartsMatchedTimer;

	private float _convertValuePerSecond;

	private float _pauseTime;

	public bool ClayConvertFinished
	{
		get
		{
			if (_convertValuePerSecond > 0f)
			{
				return _displayConvertValue >= CurrentHill.Instance.ClayPerHeart;
			}
			return _displayConvertValue <= CurrentHill.Instance.ClayPerHeart;
		}
	}

	private void Awake()
	{
		GameModeStateMonsterLove.HeartCollectedEvent += OnHeartCollected;
		GameModeStateMonsterLove.HeartBrokenEvent += OnHeartBroken;
		GameModeStateMonsterLove.HeartsMatchedEvent += OnHeartsMatched;
		GameModeStateMonsterLove.MultiplierDownEvent += OnMultiplierDown;
	}

	private void OnDestroy()
	{
		GameModeStateMonsterLove.HeartCollectedEvent -= OnHeartCollected;
		GameModeStateMonsterLove.HeartBrokenEvent -= OnHeartBroken;
		GameModeStateMonsterLove.HeartsMatchedEvent -= OnHeartsMatched;
		GameModeStateMonsterLove.MultiplierDownEvent -= OnMultiplierDown;
	}

	private void SetHeartColours()
	{
		for (int i = 0; i < 3; i++)
		{
			Renderer[] componentsInChildren = _Hearts[i].GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				if (renderer.material.HasProperty("_HueShift"))
				{
					CurrentHill.Instance.Definition._RascalColours[i].UseOnHSVMaterial(renderer.material);
				}
			}
		}
	}

	private void GetHeartLabels()
	{
		if (_heartLabels == null)
		{
			_heartLabels = new UILabel[3];
		}
		for (int i = 0; i < 3; i++)
		{
			_heartLabels[i] = _Hearts[i].GetComponentInChildren<UILabel>();
		}
	}

	private void ClearHearts()
	{
		for (int i = 0; i < 3; i++)
		{
			_Hearts[i].animation.Play("HeartUI_empty");
		}
	}

	private string FloatAsPercentage(float val)
	{
		return string.Format("{0:0}%", val * 100f);
	}

	private void SetConversionNumber()
	{
		float cupidBonus = SaveData.Instance.Purchases.GetCupidBonus();
		_CupidConversionNumber.text = string.Format("X{0}", cupidBonus);
		SetUpgradeMarker();
	}

	private void SetUpgradeMarker()
	{
		float cupidBonus = SaveData.Instance.Purchases.GetCupidBonus();
		_SuperCupidUpgradeMarker.SetActiveRecursively(cupidBonus > 1f);
	}

	public void InGameEnter()
	{
		_HeartsCollectedCounter.transform.parent.gameObject.SetActiveRecursively(true);
		_HeartsCollectedCounter.animation.Play("HeartUI_In");
		SetHeartColours();
		GetHeartLabels();
		ClearHearts();
		TurnOffCupid();
		SetConversionNumber();
	}

	public void InGameExit()
	{
		if (_currentState == State.WaitingToShowHeartsMatch)
		{
			ShowHeartsMatch();
		}
	}

	public void InGameUpdate()
	{
		State currentState = _currentState;
		if (currentState != 0 && currentState == State.WaitingToShowHeartsMatch)
		{
			UpdateWaitingToShowHeartsMatch();
		}
	}

	private void UpdateWaitingToShowHeartsMatch()
	{
		if (_showHeartsMatchedTimer < Time.time)
		{
			ShowHeartsMatch();
			_currentState = State.Idle;
		}
	}

	private void ShowHeartsMatch()
	{
		for (int i = 0; i < 3; i++)
		{
			_Hearts[i].animation.Play("HeartUI_shrink");
		}
		_MatchedHeartsScoreLabel.text = string.Format("X{0}", GameModeStateMonsterLove.Instance._heartMultiplier);
		_MatchHeartsAnimation_Particles.Play();
		_MatchHeartsAnimation_Score.Play("HeartUI_combined");
		_MatchHeartsAnimation_Score.PlayQueued("HeartUI_combinedIdle");
	}

	private void OnHeartCollected(int colourIndex, int value)
	{
		_Hearts[colourIndex].animation.Play("HeartUI_Collected");
		_Hearts[colourIndex].animation["HeartUI_Idle"].wrapMode = WrapMode.Loop;
		_Hearts[colourIndex].animation.PlayQueued("HeartUI_Idle");
		_heartLabels[colourIndex].text = value.ToString();
	}

	private void OnHeartBroken(int colourIndex)
	{
		_Hearts[colourIndex].animation.Play("HeartUI_break");
		_Hearts[colourIndex].animation.PlayQueued("HeartUI_empty");
	}

	private void OnMultiplierDown()
	{
		if (_MatchHeartsAnimation_Score.IsPlaying("HeartUI_combinedIdle"))
		{
			_MatchHeartsAnimation_Score.Play("HeartUI_combinedOut");
		}
	}

	private void OnHeartsMatched(int value)
	{
		if (_currentState == State.WaitingToShowHeartsMatch)
		{
			ShowHeartsMatch();
		}
		_showHeartsMatchedTimer = Time.time + 0.5f;
		_currentState = State.WaitingToShowHeartsMatch;
	}

	public void ClayConvertStart()
	{
		OnMultiplierDown();
		_Cupid.SetActiveRecursively(true);
		if (BuildDetails.Instance._HasIAP)
		{
			_CupidConverterBlob.SetActiveRecursively(true);
		}
		SetUpgradeMarker();
		_HeartsCollectedCounter.animation.Play("HeartUI_Out");
		_HeartsCollectedCounter.animation.PlayQueued("HeartUI_Finished");
		_CupidAnimatedSprite.Play("HeartUI_Out");
		float num = InGameController.Instance.ClayCollectedThisRun.Amount(0);
		_displayClay = num / CurrentHill.Instance.ClayPerHeart;
		_clayConvertPerSecond = (num - _displayClay) / _timeToConvert;
		_displayConvertValue = 1f;
		_convertValuePerSecond = (CurrentHill.Instance.ClayPerHeart - _displayConvertValue) / _timeToConvert;
		_pauseTime = Time.time + 1f;
		UpdateTexts();
		InGameAudio.PostFabricEvent("ClayConvert", EventAction.PlaySound);
	}

	public void ClayConvertUpdate()
	{
		if (_HeartsCollectedCounter.animation.IsPlaying("HeartUI_Out"))
		{
			_pauseTime = Time.time + 1f;
			return;
		}
		_CupidAnimatedSprite.Play("HeartUI_Finished");
		if (Time.time > _pauseTime)
		{
			_displayClay += _clayConvertPerSecond * Time.deltaTime;
			if (_clayConvertPerSecond > 0f)
			{
				_displayClay = Mathf.Min(_displayClay, InGameController.Instance.ClayCollectedThisRun.Amount(0));
			}
			else
			{
				_displayClay = Mathf.Max(_displayClay, InGameController.Instance.ClayCollectedThisRun.Amount(0));
			}
			_displayConvertValue += _convertValuePerSecond * Time.deltaTime;
			if (_convertValuePerSecond > 0f)
			{
				_displayConvertValue = Mathf.Min(_displayConvertValue, CurrentHill.Instance.ClayPerHeart);
			}
			else
			{
				_displayConvertValue = Mathf.Max(_displayConvertValue, CurrentHill.Instance.ClayPerHeart);
			}
		}
		UpdateTexts();
	}

	private void ForceFinish()
	{
		_displayClay = InGameController.Instance.ClayCollectedThisRun.Amount(0);
		_displayConvertValue = CurrentHill.Instance.ClayPerHeart;
		_CupidAnimatedSprite.Play("HeartUI_Finished");
		UpdateTexts();
	}

	private void UpdateTexts()
	{
		_ClayLabel.text = string.Format("{0:0}", Mathf.CeilToInt(_displayClay));
		_ConvertValueLabel.text = FloatAsPercentage(_displayConvertValue);
	}

	public void ClayConvertExit()
	{
		ForceFinish();
		TransOutCupid();
	}

	public void TransOutCupid()
	{
		if (_HeartsCollectedCounter.animation.IsPlaying("HeartUI_Out"))
		{
			_HeartsCollectedCounter.animation["HeartUI_Out"].normalizedTime = 1f;
			_HeartsCollectedCounter.animation.Sample();
		}
		_HeartsCollectedCounter.animation.Play("HeartUI_SlideOff");
	}

	public void TurnOffCupid()
	{
		_Cupid.SetActiveRecursively(false);
		_CupidConverterBlob.SetActiveRecursively(false);
	}
}
