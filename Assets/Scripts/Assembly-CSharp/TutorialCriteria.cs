using System;
using System.Collections.Generic;

[Serializable]
public class TutorialCriteria
{
	public int _MaxPlays = -1;

	public int _GapBetweenPlays;

	public bool _IgnoreGapFirstPlay;

	public bool _SomethingNewInShop;

	public bool _InGameOnly;

	public bool _FrontendOnly;

	public bool _HasNeverSpunJVP;

	public bool _HasNeverPressedPlay;

	public bool _HasNeverSeenGoodItem;

	public bool _HasNeverPressedGoodItem;

	public bool _HasNeverFlicked;

	public bool _PowePlayAvailable;

	public bool _CantAffordAnything;

	public bool _CanUnlockNextHill;

	public bool _CurrentHillLocked;

	public Tutorial _TutorialHasBeenPlayed;

	public Tutorial _TutorialNotPlayed;

	public List<Tutorial> _TutorialsNotAllowedToPlay;

	public bool _InQuestMode;

	public bool _InMonsterLove;

	public bool _BuildHasIAP;

	private int _gapCount;

	public Tutorial Tutorial { get; set; }

	public bool WillEverShowAgain()
	{
		return true && CheckHasPressedPlay() && CheckMaxPlays() && CheckHasNeverSpunJVP() && CheckHasNeverSeenGoodItem() && CheckHasNeverPressedGoodItem() && CheckHasNeverFlicked() && CheckTutorialNotPlayed();
	}

	public bool ShouldShowNow()
	{
		bool flag = true && SaveData.Instance.Progress._optionTutorialsOn.Set && CheckHasPressedPlay() && CheckMaxPlays() && CheckSomethingNewInShop() && CheckGameState() && CheckHasNeverSpunJVP() && CheckHasNeverSeenGoodItem() && CheckHasNeverPressedGoodItem() && CheckHasNeverFlicked() && CheckPowerPlaysAvailable() && CheckTutorialHasBeenPlayed() && CheckTutorialNotPlayed() && CheckTutorialNotAllowedToPlayed() && CheckCantAffordAnything() && CheckCanUnlockNextHill() && CheckCurrentHillLocked() && CheckIsQuestMode() && CheckIsMonsterLove() && CheckBuildHasIAP();
		if (flag && _GapBetweenPlays > 0 && (!_IgnoreGapFirstPlay || Tutorial.SavedData.NumPlays > 0))
		{
			_gapCount++;
			if (_gapCount <= _GapBetweenPlays)
			{
				return false;
			}
			_gapCount = 0;
		}
		return flag;
	}

	private bool CheckHasPressedPlay()
	{
		if (!_HasNeverPressedPlay)
		{
			return true;
		}
		return !SaveData.Instance.Progress._FinishedOneLevel.Set;
	}

	private bool CheckMaxPlays()
	{
		if (_MaxPlays == -1)
		{
			return true;
		}
		return Tutorial.SavedData.NumPlays < _MaxPlays;
	}

	private bool CheckSomethingNewInShop()
	{
		if (!_SomethingNewInShop)
		{
			return true;
		}
		return CurrentHill.Instance.ProgressData.HasSomethingNewInShop();
	}

	private bool CheckGameState()
	{
		MetaGameController.State currentState = MetaGameController.Instance.CurrentState;
		if (_FrontendOnly && currentState != MetaGameController.State.Frontend && currentState != MetaGameController.State.LoadingFrontend)
		{
			return false;
		}
		if (_InGameOnly && currentState != MetaGameController.State.InGame && currentState != MetaGameController.State.LoadingInGame)
		{
			return false;
		}
		return true;
	}

	private bool CheckHasNeverSpunJVP()
	{
		if (!_HasNeverSpunJVP)
		{
			return true;
		}
		return !SaveData.Instance.Progress._HasSpunJVP.Set;
	}

	private bool CheckHasNeverSeenGoodItem()
	{
		if (!_HasNeverSeenGoodItem)
		{
			return true;
		}
		return !SaveData.Instance.Progress._HaveSeenGoodJVPItem.Set;
	}

	private bool CheckHasNeverPressedGoodItem()
	{
		if (!_HasNeverPressedGoodItem)
		{
			return true;
		}
		return !SaveData.Instance.Progress._HavePressedGoodJVPItem.Set;
	}

	private bool CheckHasNeverFlicked()
	{
		if (!_HasNeverFlicked)
		{
			return true;
		}
		return !SaveData.Instance.Progress._HasFlicked.Set;
	}

	private bool CheckTutorialHasBeenPlayed()
	{
		if (_TutorialHasBeenPlayed == null)
		{
			return true;
		}
		TutorialData tutorialData = SaveData.Instance.Tutorials.GetTutorialData(_TutorialHasBeenPlayed.name);
		return tutorialData == null || tutorialData.NumPlays > 0;
	}

	private bool CheckTutorialNotPlayed()
	{
		if (_TutorialNotPlayed == null)
		{
			return true;
		}
		TutorialData tutorialData = SaveData.Instance.Tutorials.GetTutorialData(_TutorialNotPlayed.name);
		return tutorialData == null || tutorialData.NumPlays <= 0;
	}

	private bool CheckTutorialNotAllowedToPlayed()
	{
		if (_TutorialsNotAllowedToPlay.Count == 0)
		{
			return true;
		}
		foreach (Tutorial item in _TutorialsNotAllowedToPlay)
		{
			if (item._Criteria.ShouldShowNow())
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckPowerPlaysAvailable()
	{
		if (!_PowePlayAvailable)
		{
			return true;
		}
		return CurrentHill.Instance.ProgressData._PowerPlaysRemaining > 0;
	}

	private bool CheckCantAffordAnything()
	{
		if (!_CantAffordAnything)
		{
			return true;
		}
		if (CurrentHill.Instance.ProgressData._State == LockState.Locked)
		{
			return false;
		}
		return JVPController.Instance.NumAffordableItems == 0;
	}

	private bool CheckCanUnlockNextHill()
	{
		if (!_CanUnlockNextHill)
		{
			return true;
		}
		return FrontendWorldController.Instance.CanUnlockNextHill();
	}

	private bool CheckCurrentHillLocked()
	{
		if (!_CurrentHillLocked)
		{
			return true;
		}
		return CurrentHill.Instance.ProgressData._State == LockState.Locked;
	}

	private bool CheckIsQuestMode()
	{
		if (!_InQuestMode)
		{
			return true;
		}
		return CurrentGameMode.Type == GameModeType.Quest;
	}

	private bool CheckIsMonsterLove()
	{
		if (!_InMonsterLove)
		{
			return true;
		}
		return CurrentGameMode.Type == GameModeType.MonsterLove;
	}

	private bool CheckBuildHasIAP()
	{
		if (!_BuildHasIAP)
		{
			return true;
		}
		return BuildDetails.Instance._HasIAP;
	}
}
