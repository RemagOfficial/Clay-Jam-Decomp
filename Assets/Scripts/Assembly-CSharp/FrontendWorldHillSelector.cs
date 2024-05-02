using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FrontendWorldHillSelector
{
	private const string CharacterSlotIconResourcePath = "BigJigger/MonsterIcons";

	public int _HillID;

	public int _hillHierarchyPrefix;

	public ObjectAsButtonButton _LeftArrow;

	public ObjectAsButtonButton _RightArrow;

	public ObjectAsButtonButton _PlayButton;

	public GameObject _BestBeanObject;

	public TextMesh _BestBeanText;

	public GameObject _PowerPlaysObject;

	public TextMesh _PowerPlaysText;

	public Animation _UpgradeAnimation;

	public Animation _BossAnimation;

	public FrontendWorldPadlock _Padlock;

	public string BossAppearAnim = "BossAppear";

	public string BossHiddenAnim = "BossHidden";

	public string BossUpgradeAnim = "AddSection{0}";

	public GameObject _Boss;

	public Material _BossMaterialNormal;

	private Material _BossMaterialNormalInst;

	public Material _BossMaterialDefeated;

	private Material _BossMaterialDefeatedInst;

	public AnimatedSprite _BossAnimatedSprite;

	public GameObject _Trophy;

	public TextMesh _TrophyText;

	private List<CharacterSlotData> _characterSlots;

	private Texture2D[] _characterSlotIcons;

	private FrontendWorldController Controller;

	public void Initialise(FrontendWorldController controller)
	{
		Controller = controller;
		_LeftArrow.Initialise(12);
		_RightArrow.Initialise(12);
		_PlayButton.Initialise(12);
		_PlayButton.ShouldPlayIdleAnim = true;
		_BossMaterialNormalInst = new Material(_BossMaterialNormal);
		_BossMaterialDefeatedInst = new Material(_BossMaterialDefeated);
		InitialiseCharacterSlotData();
	}

	public void UpdateButtons(string currentMouseOverButton)
	{
		if (!Controller.IsTransitionAnimationPlaying())
		{
			if (_LeftArrow.UpdateState(currentMouseOverButton))
			{
				Controller.SelectHillLeft();
			}
			if (_RightArrow.UpdateState(currentMouseOverButton))
			{
				Controller.SelectHillRight();
			}
			if (_PlayButton.UpdateState(currentMouseOverButton))
			{
				Controller.Play();
			}
		}
	}

	private void SetUpgradeLevel(bool useLastSeenState)
	{
		HillData hillByID = SaveData.Instance.Hills.GetHillByID(_HillID);
		int upgradeLevelAnimated = hillByID._UpgradeLevelAnimated;
		int num = ((!useLastSeenState) ? hillByID._UpgradeLevel : upgradeLevelAnimated);
		if (num != upgradeLevelAnimated)
		{
			AnimateUpgradeLevel(hillByID._UpgradeLevelAnimated, hillByID._UpgradeLevel);
			hillByID._UpgradeLevelAnimated = hillByID._UpgradeLevel;
			SaveData.Instance.MarkAsNeedToSave(false);
			return;
		}
		SnapToUpgradeLevel(num);
		bool flag = hillByID._State == LockState.Locked;
		if (!flag && useLastSeenState)
		{
			flag = upgradeLevelAnimated == -1;
		}
		if (flag)
		{
			_Padlock.SetLocked();
		}
		else
		{
			_Padlock.SetUnlocked();
		}
	}

	private void SnapToUpgradeLevel(int upgradeLevel)
	{
		int num = upgradeLevel + 1;
		if (num == 0)
		{
			num = 1;
		}
		string animation = string.Format("Section{0}", num);
		_UpgradeAnimation.Play(animation);
	}

	private void AnimateUpgradeLevel(int fromUpgradeLevel, int toUpgradeLevel)
	{
		if (fromUpgradeLevel == -1)
		{
			_Padlock.Unlock();
		}
		if (toUpgradeLevel != 0)
		{
			string animation = string.Format("AddSection{0}", toUpgradeLevel + 1);
			_UpgradeAnimation.Play(animation);
			InGameAudio.PostFabricEvent("HillGrow");
		}
	}

	public void DisplayBoss(bool forceHide)
	{
		HillData progressData = CurrentHill.Instance.ProgressData;
		if (progressData._BeastDefeatedCount != 0)
		{
			_Boss.renderer.material = _BossMaterialDefeatedInst;
			_BossAnimatedSprite.Material = _BossMaterialDefeatedInst;
			_Trophy.SetActiveRecursively(true);
			_TrophyText.text = Localization.PunctuatedNumber(progressData._BeastDefeatedCount, 99);
		}
		else
		{
			_Trophy.SetActiveRecursively(false);
			_Boss.renderer.material = _BossMaterialNormalInst;
			_BossAnimatedSprite.Material = _BossMaterialNormalInst;
		}
		if (forceHide || progressData._State == LockState.Locked)
		{
			HideBoss();
		}
		else if (progressData._UpgradeLevel == 0)
		{
			_BossAnimation.Play(BossAppearAnim);
		}
		else if ((bool)_BossAnimation[string.Format(BossUpgradeAnim, progressData._UpgradeLevel + 1)])
		{
			_BossAnimation.Play(string.Format(BossUpgradeAnim, progressData._UpgradeLevel + 1));
		}
	}

	public void HideBoss()
	{
		_BossAnimation.Play(BossHiddenAnim);
	}

	private void SetBestBean()
	{
		float bestScore = SaveData.Instance.Hills.GetHillByID(_HillID)._BestScore;
		if (bestScore == 0f)
		{
			_BestBeanObject.SetActiveRecursively(false);
			return;
		}
		_BestBeanObject.SetActiveRecursively(true);
		_BestBeanText.text = string.Format("{0}", bestScore);
	}

	private void SetPowerPlays()
	{
		int powerPlaysRemaining = SaveData.Instance.Hills.GetHillByID(_HillID)._PowerPlaysRemaining;
		if (powerPlaysRemaining == 0)
		{
			TurnOffPowerPlays();
			return;
		}
		_PowerPlaysObject.SetActiveRecursively(true);
		_PowerPlaysText.text = string.Format("{0}", Localization.PunctuatedNumber(powerPlaysRemaining, 99));
	}

	private void TurnOffPowerPlays()
	{
		_PowerPlaysObject.SetActiveRecursively(false);
	}

	public void SetPlayButtonState()
	{
		if (SaveData.Instance.Hills.GetHillByID(_HillID)._State == LockState.Locked)
		{
			_PlayButton._GameObject.SetActiveRecursively(false);
		}
		else
		{
			_PlayButton._GameObject.SetActiveRecursively(true);
		}
	}

	public void Refresh(bool useLastSeenState = false)
	{
		SetBestBean();
		SetUpgradeLevel(useLastSeenState);
		SetPowerPlays();
		SetCharacterSlots(false);
		DisplayBoss(useLastSeenState);
	}

	public void SetupForStory()
	{
		TurnOffPowerPlays();
		SetCharacterSlots(true);
		_Padlock.SetUnlocked();
		_Trophy.SetActiveRecursively(false);
		if (StoryController.Instance.PlayingCompleteStory)
		{
			_Boss.renderer.material = _BossMaterialDefeatedInst;
			_BossAnimatedSprite.Material = _BossMaterialDefeatedInst;
		}
		else
		{
			_Boss.renderer.material = _BossMaterialNormalInst;
			_BossAnimatedSprite.Material = _BossMaterialNormalInst;
		}
	}

	private void InitialiseCharacterSlotData()
	{
		LoadCharacterSlotIcons();
		HillDefinition definitionFromID = HillDatabase.Instance.GetDefinitionFromID(_HillID);
		int count = definitionFromID._UpgradeLevels.Count;
		_characterSlots = new List<CharacterSlotData>(count * 3);
		for (int i = 0; i < count; i++)
		{
			string creatureUnlock = definitionFromID._UpgradeLevels[i]._CreatureUnlock;
			for (int j = 0; j < 3; j++)
			{
				CastId castID = new CastId(creatureUnlock, j);
				CastData castData = SaveData.Instance.Casts.GetCast(castID);
				if (castData == null)
				{
					Debug.LogError(string.Format("Couldn't find cast data for {0}, colour {1}", creatureUnlock, j));
					continue;
				}
				string text = string.Format("Hill{0}Slot{1}", _hillHierarchyPrefix, i * 3 + j);
				GameObject gameObject = GameObject.Find(text);
				if (gameObject == null || gameObject.renderer == null)
				{
					Debug.LogWarning(string.Format("Game object {0} {1} not found", text, (!(gameObject == null)) ? "with a renderer" : string.Empty));
					continue;
				}
				Texture2D texture2D = Array.Find(_characterSlotIcons, (Texture2D t) => t.name == castData.IconName);
				if (texture2D == null)
				{
					Debug.LogError(string.Format("No icon called {0} found for hill slot {1}", castData.IconName, text));
				}
				else
				{
					_characterSlots.Add(new CharacterSlotData(gameObject.renderer, castData, texture2D));
				}
			}
		}
	}

	private void LoadCharacterSlotIcons()
	{
		UnityEngine.Object[] array = Resources.LoadAll("BigJigger/MonsterIcons");
		_characterSlotIcons = new Texture2D[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_characterSlotIcons[i] = array[i] as Texture2D;
		}
	}

	private void SetCharacterSlots(bool showAll)
	{
		foreach (CharacterSlotData characterSlot in _characterSlots)
		{
			if (showAll)
			{
				characterSlot.TurnOn();
			}
			else
			{
				characterSlot.TurnOnIfPurchasedOnHill(_HillID);
			}
		}
	}
}
