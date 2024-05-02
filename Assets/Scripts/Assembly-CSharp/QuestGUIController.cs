using System;
using UnityEngine;

[Serializable]
public class QuestGUIController
{
	private enum State
	{
		Off = 0,
		Intro_ShowingBullyBeastDeath = 1,
		Intro_WaitingForContinue = 2,
		Intro_AnimateSkipQuest = 3,
		Intro_AnimateNewQuest = 4,
		Complete_TransIn = 5,
		Complete_AnimateOldQuest = 6,
		Complete_AnimateNewQuest = 7,
		Complete_WaitingForTap = 8,
		Congrats_WaitingForTap = 9
	}

	public Animation _WholeScreenAnim;

	public Animation _QuestPanelAnim;

	public GameObject _QuestPanelObject;

	public GameObject _InfoObject;

	public GameObject _CongratsBoxObject;

	public GameObject _ClayTotalGroup;

	public GameObject _SkipButtonGroup;

	public GameObject _MonsterLoveBox;

	public GameObject _CantAffordIcon;

	public string _AllOutAnimName;

	public string _AllInAnimName;

	public string _QuestCompleteAnimName;

	public string _QuestNextAnimName;

	public string _SkipOut = "SkipOut";

	public string _SkipIn = "SkipIn";

	public LocalisableText _QuestText;

	public LocalisableText _InfoText;

	public LocalisableText _CongratsText;

	public LocalisableText _SkipText;

	public UILabel _SkipCostText;

	public LocalisableText _MonsterLoveScoreText;

	public GameObject _IconParent;

	private UISpriteAnimationControlled _questIcons;

	public PowerUpPickerGUIController _PowerUpPickerController;

	public QuestCounterGUIController _CounterController;

	public UISpriteAnimationOnEvent _SkipButtonAnim;

	public Collider _SkipButtonCollider;

	private GameObject QuestPanelParentObject
	{
		get
		{
			return _WholeScreenAnim.gameObject;
		}
	}

	private State CurrentState { get; set; }

	public bool Finished
	{
		get
		{
			return CurrentState == State.Off;
		}
	}

	private bool IsSkippingAllowed
	{
		get
		{
			return !CurrentHill.Instance.ProgressData.OnLastQuest && !BuildDetails.Instance._DemoMode;
		}
	}

	public void Update()
	{
		switch (CurrentState)
		{
		case State.Intro_ShowingBullyBeastDeath:
			UpdateShowingBullyBeastDeath();
			break;
		case State.Intro_WaitingForContinue:
			UpdateWaitingForContinue();
			break;
		case State.Intro_AnimateSkipQuest:
			UpdateAnimateSkipQuest();
			break;
		case State.Intro_AnimateNewQuest:
			UpdateAnimateNewQuestAfterSkip();
			break;
		case State.Complete_TransIn:
			UpdateCompleteTransIn();
			break;
		case State.Complete_AnimateOldQuest:
			UpdateAnimateOldQuest();
			break;
		case State.Complete_AnimateNewQuest:
			UpdateAnimateNewQuest();
			break;
		case State.Complete_WaitingForTap:
			UpdateCompleteWaitingForTap();
			break;
		case State.Congrats_WaitingForTap:
			UpdateCongratsWaitingForTap();
			break;
		}
	}

	public void ShowIntro()
	{
		switch (CurrentGameMode.Type)
		{
		case GameModeType.Quest:
			ShowIntro_Quests();
			break;
		case GameModeType.MonsterLove:
			ShowIntro_MonsterLove();
			break;
		}
	}

	private void ShowIntro_MonsterLove()
	{
		InitialiseIcons();
		QuestPanelParentObject.active = true;
		TurneQuestPanelOn(false);
		TurnInfoBoxOn(false);
		TurnCongratsBoxOn(false);
		TurnOnCounter(false);
		TurnOnSkipElements(false);
		TurnOnClayTotal(true);
		TurnLoveBoxOn(true);
		_PowerUpPickerController.TurnOn();
		_WholeScreenAnim.Play(_AllInAnimName);
		CurrentState = State.Intro_WaitingForContinue;
	}

	private void ShowIntro_Quests()
	{
		InitialiseIcons();
		QuestPanelParentObject.active = true;
		TurneQuestPanelOn(true);
		TurnInfoBoxOn(true);
		TurnCongratsBoxOn(false);
		TurnOnCounter(false);
		TurnOnSkipElements(IsSkippingAllowed);
		TurnOnClayTotal(IsSkippingAllowed);
		TurnLoveBoxOn(false);
		_PowerUpPickerController.TurnOn();
		_WholeScreenAnim.Play(_AllInAnimName);
		CurrentState = State.Intro_WaitingForContinue;
	}

	private void UpdateWaitingForContinue()
	{
		_PowerUpPickerController.UpdateBuyButtonAffordability();
	}

	private void UpdateAnimateSkipQuest()
	{
		if (!_QuestPanelAnim.isPlaying)
		{
			ShowNewQuestAfterSkip();
		}
	}

	private void ShowNewQuestAfterSkip()
	{
		CurrentQuest.Instance.MoveOn(true);
		SaveData.Instance.Save();
		if (IsSkippingAllowed)
		{
			TurnOnSkipElements(true);
			_SkipButtonGroup.animation.Play(_SkipIn);
		}
		else
		{
			TurnOnSkipElements(false);
		}
		_InfoText.text = GetInfoBoxText();
		FillInQuestDetails();
		_QuestPanelAnim.Play(_QuestNextAnimName);
		CurrentState = State.Intro_AnimateNewQuest;
	}

	private void UpdateAnimateNewQuestAfterSkip()
	{
		if (!_QuestPanelAnim.isPlaying)
		{
			CurrentState = State.Intro_WaitingForContinue;
		}
	}

	public void ShowComplete()
	{
		TurnOnCounter(false);
		_PowerUpPickerController.TurnOff();
		if (!CurrentQuest.Instance.QuestComplete)
		{
			CurrentState = State.Off;
			if (CurrentQuest.Instance.HasQuest)
			{
				CurrentQuest.Instance.MarkFailed();
			}
		}
		else if (CurrentHill.Instance.ProgressData.OnLastQuest)
		{
			ShowDeathScene();
		}
		else
		{
			ShowQuestComplete();
		}
	}

	private void ShowDeathScene()
	{
		CurrentState = State.Intro_ShowingBullyBeastDeath;
	}

	private void UpdateShowingBullyBeastDeath()
	{
		if (BossMonster.Instance.FinishedShowingDeathSequence())
		{
			ShowQuestComplete();
		}
	}

	private void ShowQuestComplete()
	{
		QuestPanelParentObject.active = true;
		TurneQuestPanelOn(true);
		TurnInfoBoxOn(false);
		TurnCongratsBoxOn(false);
		TurnOnSkipElements(false);
		_WholeScreenAnim.Play(_AllInAnimName);
		CurrentState = State.Complete_TransIn;
		InGameAudio.PostFabricEvent("QuestTrumpet");
	}

	private void UpdateCompleteWaitingForTap()
	{
		if (ClayJamInput.AnythingPressed)
		{
			HideComplete();
		}
	}

	private void UpdateCongratsWaitingForTap()
	{
		if (ClayJamInput.AnythingPressed)
		{
			HideCongrats();
		}
	}

	private void UpdateCompleteTransIn()
	{
		if (!_WholeScreenAnim.isPlaying)
		{
			AnimateOldQuestOff();
		}
	}

	private void UpdateAnimateOldQuest()
	{
		if (!_QuestPanelAnim.isPlaying)
		{
			if (BuildDetails.Instance._DemoMode)
			{
				HideComplete();
			}
			else
			{
				MoveOnToNextQuest();
			}
		}
	}

	private void UpdateAnimateNewQuest()
	{
		if (!_QuestPanelAnim.isPlaying)
		{
			CurrentState = State.Complete_WaitingForTap;
		}
	}

	private void MoveOnToNextQuest()
	{
		if (CurrentQuest.Instance.MoveOn(false))
		{
			ShowCongrats();
		}
		else
		{
			ShowNextQuest();
		}
	}

	private void ShowNextQuest()
	{
		FillInQuestDetails();
		_QuestPanelAnim.Play(_QuestNextAnimName);
		CurrentState = State.Complete_AnimateNewQuest;
	}

	private void ShowCongrats()
	{
		TurnCongratsBoxOn(true);
		CurrentState = State.Congrats_WaitingForTap;
	}

	private void AnimateOldQuestOff()
	{
		_QuestPanelAnim.Play(_QuestCompleteAnimName);
		CurrentState = State.Complete_AnimateOldQuest;
	}

	private void HideIntro()
	{
		_WholeScreenAnim.Play(_AllOutAnimName);
		CurrentState = State.Off;
		TurnOnCounter(CurrentGameMode.Type == GameModeType.Quest);
		_PowerUpPickerController.EnableToggles(false);
	}

	private void HideComplete()
	{
		_WholeScreenAnim.Play(_AllOutAnimName);
		CurrentState = State.Off;
	}

	private void HideCongrats()
	{
		_WholeScreenAnim.Play(_AllOutAnimName);
		CurrentState = State.Off;
		if (CurrentHill.Instance.ProgressData._BeastDefeatedCount == 1 && SaveData.Instance.Hills.AllDefeated)
		{
			UIEvents.SendEvent(UIEventType.ReturnToFrontend, null);
		}
	}

	private void TurneQuestPanelOn(bool on)
	{
		if (on)
		{
			FillInQuestDetails();
		}
		_QuestPanelObject.SetActiveRecursively(on);
	}

	private void TurnInfoBoxOn(bool on)
	{
		if (on)
		{
			_InfoText.text = GetInfoBoxText();
		}
		_InfoObject.SetActiveRecursively(on);
	}

	private void TurnLoveBoxOn(bool on)
	{
		_MonsterLoveBox.SetActiveRecursively(on);
		if (on)
		{
			_MonsterLoveScoreText.text = GetMonsterLoveScoreText();
		}
	}

	private void TurnCongratsBoxOn(bool on)
	{
		if (on)
		{
			_CongratsText.text = GetCongratsText();
		}
		_CongratsBoxObject.SetActiveRecursively(on);
	}

	private void TurnOnSkipElements(bool on)
	{
		_SkipButtonGroup.SetActiveRecursively(on);
		if (on)
		{
			_SkipText.text = GetSkipText();
			_SkipCostText.text = GetSkipCostText();
			if (BuildDetails.Instance._HasIAP)
			{
				_SkipButtonCollider.enabled = true;
			}
			else if (CanAffordSkip())
			{
				_SkipButtonCollider.enabled = true;
				_CantAffordIcon.SetActiveRecursively(false);
			}
			else
			{
				_SkipButtonAnim.PlayOnReleaseAnim();
				_SkipButtonCollider.enabled = false;
			}
		}
	}

	private void TurnOnClayTotal(bool on)
	{
		_ClayTotalGroup.SetActiveRecursively(on);
	}

	private string GetInfoBoxText()
	{
		string key = string.Format("BOSS_Name_{0}", CurrentHill.Instance.ID);
		string arg = Localization.instance.Get(key);
		string format = Localization.instance.Get("QUESTS_Tracker1");
		string arg2 = string.Format(format, arg);
		string format2 = Localization.instance.Get("QUESTS_Tracker2");
		string arg3 = string.Format(format2, CurrentHill.Instance.ProgressData._CurrentQuestIndex, QuestDatabase.Instance.QuestCount(CurrentHill.Instance.ID));
		return string.Format("{0}\\n{1}", arg2, arg3);
	}

	private string GetCongratsText()
	{
		if (CurrentHill.Instance.ProgressData._BeastDefeatedCount == 1)
		{
			return GetCongratsTextFirstTime();
		}
		return GetCongratsTextSubsequentTimes();
	}

	private string GetCongratsTextFirstTime()
	{
		string arg = Localization.instance.Get("QUESTS_Won1");
		string key = string.Format("BOSS_Name_{0}", CurrentHill.Instance.ID);
		string arg2 = Localization.instance.Get(key);
		string name = CurrentHill.Instance.Definition._Name;
		string arg3 = Localization.instance.Get(name);
		string format = Localization.instance.Get("QUESTS_Won2");
		string arg4 = string.Format(format, arg2, arg3);
		return string.Format("{0}\\n\\n{1}", arg, arg4);
	}

	private string GetCongratsTextSubsequentTimes()
	{
		string arg = Localization.instance.Get("QUESTS_Won1");
		string key = string.Format("BOSS_Name_{0}", CurrentHill.Instance.ID);
		string arg2 = Localization.instance.Get(key);
		string format = Localization.instance.Get("QUESTS_Won4");
		string arg3 = string.Format(format, arg2);
		return string.Format("{0}\\n\\n{1}", arg, arg3);
	}

	private string GetSkipText()
	{
		return Localization.instance.Get("QUESTS_Skip");
	}

	private string GetSkipCostText()
	{
		string format = Localization.instance.Get("QUESTS_SkipCost");
		string arg = Localization.PunctuatedNumber(CostToSkip(), int.MaxValue);
		return string.Format(format, arg);
	}

	private string GetMonsterLoveScoreText()
	{
		string format = Localization.instance.Get("MONSTERLOVE_Beat");
		string arg = Localization.PunctuatedNumber(CurrentHill.Instance.ProgressData._BestScoreMonsterLove, 99999);
		return string.Format(format, arg);
	}

	private void InitialiseIcons()
	{
		if (!(_questIcons != null))
		{
			string text = string.Format("GUI/QuestIcons/QuestIconHill{0}", CurrentHill.Instance.ID);
			UnityEngine.Object @object = Resources.Load(text);
			if (@object == null)
			{
				Debug.LogError(string.Format("QuestIcons Resource not found: {0}", text));
				return;
			}
			_questIcons = AddIconsToObject(@object, _IconParent);
			_CounterController.Icon = AddIconsToObject(@object, _CounterController._IconParent);
		}
	}

	private UISpriteAnimationControlled AddIconsToObject(UnityEngine.Object iconPrefab, GameObject parent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(iconPrefab) as GameObject;
		UISpriteAnimationControlled component = gameObject.GetComponent<UISpriteAnimationControlled>();
		if (component == null)
		{
			Debug.LogError("QuestIcons prefab does not have a UISpriteAnimationControlled component", iconPrefab);
			return null;
		}
		gameObject.transform.parent = parent.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		return component;
	}

	private void FillInQuestDetails()
	{
		_questIcons.GotoFrame(CurrentQuest.Instance.IconIndex);
		_QuestText.text = CurrentQuest.Instance.Description;
	}

	public void StartInGame()
	{
		QuestPanelParentObject.active = true;
	}

	public void TurnOnCounter(bool on)
	{
		_CounterController.TurnOn(on);
	}

	public void HideCounter()
	{
		TurnOnCounter(false);
	}

	public void ShowCounter()
	{
		TurnOnCounter(true);
	}

	public void ContinuePressed()
	{
		if (CurrentState == State.Intro_WaitingForContinue)
		{
			HideIntro();
		}
	}

	public void SkipPressed()
	{
		if (CurrentState == State.Intro_WaitingForContinue)
		{
			if (CanAffordSkip())
			{
				PayForSkip();
				_SkipButtonGroup.animation.Play(_SkipOut);
				_QuestPanelAnim.Play(_QuestCompleteAnimName);
				CurrentState = State.Intro_AnimateSkipQuest;
				AnalyticsController.Instance.SkipQuest();
			}
			else if (BuildDetails.Instance._HasIAP)
			{
				StaticIAPItems._forcedUserToStore = true;
				UIEvents.SendEvent(UIEventType.ResetToPanel, InGameNGUI.Instance._IAPPanel);
			}
		}
	}

	private float CostToSkip()
	{
		return QuestDatabase.Instance.GetSkipCost(CurrentHill.Instance.ProgressData);
	}

	private bool CanAffordSkip()
	{
		return SaveData.Instance.ClayCollected.CanSubtract(0, CostToSkip());
	}

	private void PayForSkip()
	{
		SaveData.Instance.ClayCollected.Subtract(0, CostToSkip());
	}
}
