using System;
using System.Collections;
using System.Text;
using UnityEngine;

[Serializable]
public class ProgressFlags
{
	public SavedFlag _StoryPlayed;

	public SavedFlag _JVPLocked;

	public SavedFlag _FinishedOneLevel;

	public SavedInt _lastTipShown;

	public SavedInt _lastTipShownML;

	public SavedFlag _HasSpunJVP;

	public SavedFlag _HaveSeenGoodJVPItem;

	public SavedFlag _HavePressedGoodJVPItem;

	public SavedFlag _HasFlicked;

	public SavedFlag _HasBeenLoaded;

	public SavedFlag _HasBeenInstallTracked;

	public SavedFlag _HasPlayedStoryCompleteMovie;

	public SavedFlag _QuestsStarted;

	public SavedFlag _IGTutorialGougeShown;

	public SavedFlag _IGTutorialSquashShown;

	public SavedFlag _IGTutorialBossShown;

	public SavedFlag _IGTutorialTrapShown;

	public SavedFlag _optionMusicOn;

	public SavedFlag _optionSFXOn;

	public SavedFlag _optionTutorialsOn;

	public SavedFlag[] _powerplayPicker;

	public SavedFlag _hasUsedPowerPlayPicker;

	public void SetInitialState()
	{
		_StoryPlayed = new SavedFlag("story", false, new Version("10.0.000"));
		_StoryPlayed.ValueWhenLoadingFromOldVersion = false;
		_JVPLocked = new SavedFlag("jvpLocked", true, new Version("0.1.001"));
		_JVPLocked.ValueWhenLoadingFromOldVersion = false;
		_FinishedOneLevel = new SavedFlag("finishedOneLevel", false, new Version("13.0.000"));
		_FinishedOneLevel.ValueWhenLoadingFromOldVersion = false;
		_lastTipShown = new SavedInt("lastTipShown", 0, new Version("13.0.000"));
		_lastTipShown.ValueWhenLoadingFromOldVersion = 0;
		_lastTipShownML = new SavedInt("lastTipShownML", 0, new Version("29.7.000"));
		_lastTipShownML.ValueWhenLoadingFromOldVersion = 0;
		_IGTutorialGougeShown = new SavedFlag("igtGougeShown", false, new Version("0.1.002"));
		_IGTutorialGougeShown.ValueWhenLoadingFromOldVersion = false;
		_IGTutorialSquashShown = new SavedFlag("igtSquashShown", false, new Version("0.1.003"));
		_IGTutorialSquashShown.ValueWhenLoadingFromOldVersion = false;
		_IGTutorialTrapShown = new SavedFlag("igtTrapShown", false, new Version("0.1.004"));
		_IGTutorialTrapShown.ValueWhenLoadingFromOldVersion = false;
		_IGTutorialBossShown = new SavedFlag("igtBossShown", false, new Version("0.1.004"));
		_IGTutorialBossShown.ValueWhenLoadingFromOldVersion = false;
		_optionMusicOn = new SavedFlag("optionMusicOn", true, new Version("13.0.000"));
		_optionMusicOn.ValueWhenLoadingFromOldVersion = true;
		_optionSFXOn = new SavedFlag("optionSFXOn", true, new Version("13.0.000"));
		_optionSFXOn.ValueWhenLoadingFromOldVersion = true;
		_optionTutorialsOn = new SavedFlag("optionTutorialsOn", true, new Version("25.0.000"));
		_optionTutorialsOn.ValueWhenLoadingFromOldVersion = true;
		_HasSpunJVP = new SavedFlag("spunJVP", false, new Version("21.0.000"));
		_HasSpunJVP.ValueWhenLoadingFromOldVersion = false;
		_HaveSeenGoodJVPItem = new SavedFlag("goodHLJVP", false, new Version("22.0.000"));
		_HaveSeenGoodJVPItem.ValueWhenLoadingFromOldVersion = false;
		_HavePressedGoodJVPItem = new SavedFlag("pressedHLJVP", false, new Version("22.0.000"));
		_HavePressedGoodJVPItem.ValueWhenLoadingFromOldVersion = false;
		_HasFlicked = new SavedFlag("flicked", false, new Version("22.0.000"));
		_HasFlicked.ValueWhenLoadingFromOldVersion = false;
		_HasBeenLoaded = new SavedFlag("loaded", false, new Version("26.0.000"));
		_HasBeenLoaded.ValueWhenLoadingFromOldVersion = false;
		_HasBeenInstallTracked = new SavedFlag("installed", false, new Version("27.0.000"));
		_HasBeenInstallTracked.ValueWhenLoadingFromOldVersion = false;
		_HasPlayedStoryCompleteMovie = new SavedFlag("playedStoryComplete", false, new Version("29.1.000"));
		_HasPlayedStoryCompleteMovie.ValueWhenLoadingFromOldVersion = false;
		_QuestsStarted = new SavedFlag("questsStarted", false, new Version("29.1.000"));
		_QuestsStarted.ValueWhenLoadingFromOldVersion = true;
		_powerplayPicker = new SavedFlag[4];
		for (int i = 0; i < _powerplayPicker.Length; i++)
		{
			_powerplayPicker[i] = new SavedFlag("PPicker" + i, true, new Version("29.3.000"));
			_powerplayPicker[i].ValueWhenLoadingFromOldVersion = true;
		}
		_hasUsedPowerPlayPicker = new SavedFlag("UsedPPicker", false, new Version("29.3.000"));
		CurrentHill.Instance.ID = 1;
		if (BuildDetails.Instance._DemoMode)
		{
			_optionTutorialsOn.Set = false;
			_StoryPlayed.Set = true;
		}
	}

	public bool StoryHasBeenPlayed()
	{
		return _StoryPlayed.Set;
	}

	public void MarkStoryPlayed()
	{
		_StoryPlayed.Set = true;
	}

	public bool UnlockJVP()
	{
		if (!_JVPLocked.Set)
		{
			Debug.LogError("Unlocking JVP when its already unlocked");
			return false;
		}
		_JVPLocked.Set = false;
		return true;
	}

	public void SetIGTutorialGougeShown()
	{
		_IGTutorialGougeShown.Set = true;
	}

	public void SetIGTutorialSquashShown()
	{
		_IGTutorialSquashShown.Set = true;
	}

	public void SetIGTutorialTrapShown()
	{
		_IGTutorialTrapShown.Set = true;
	}

	public void SetIGTutorialBossShown()
	{
		_IGTutorialBossShown.Set = true;
	}

	public void SetMusicOn(bool b)
	{
		_optionMusicOn.Set = b;
	}

	public void SetSFXOn(bool b)
	{
		_optionSFXOn.Set = b;
		NGUITools.soundVolume = ((!b) ? 0f : 1f);
	}

	public void SetTutorialsOn(bool b)
	{
		_optionTutorialsOn.Set = b;
		if (!b)
		{
			_QuestsStarted.Set = true;
		}
	}

	public Hashtable GetHash()
	{
		Hashtable hashtable = new Hashtable();
		_StoryPlayed.AddToHashTable(hashtable);
		_JVPLocked.AddToHashTable(hashtable);
		_FinishedOneLevel.AddToHashTable(hashtable);
		_IGTutorialGougeShown.AddToHashTable(hashtable);
		_IGTutorialSquashShown.AddToHashTable(hashtable);
		_IGTutorialBossShown.AddToHashTable(hashtable);
		_IGTutorialTrapShown.AddToHashTable(hashtable);
		hashtable.Add("CurrentHill", CurrentHill.Instance.ID.ToString());
		_optionMusicOn.AddToHashTable(hashtable);
		_optionSFXOn.AddToHashTable(hashtable);
		_optionTutorialsOn.AddToHashTable(hashtable);
		_lastTipShown.AddToHashTable(hashtable);
		_lastTipShownML.AddToHashTable(hashtable);
		_HasSpunJVP.AddToHashTable(hashtable);
		_HaveSeenGoodJVPItem.AddToHashTable(hashtable);
		_HavePressedGoodJVPItem.AddToHashTable(hashtable);
		_HasFlicked.AddToHashTable(hashtable);
		_HasBeenLoaded.AddToHashTable(hashtable);
		_HasBeenInstallTracked.AddToHashTable(hashtable);
		_HasPlayedStoryCompleteMovie.AddToHashTable(hashtable);
		_QuestsStarted.AddToHashTable(hashtable);
		for (int i = 0; i < _powerplayPicker.Length; i++)
		{
			_powerplayPicker[i].AddToHashTable(hashtable);
		}
		_hasUsedPowerPlayPicker.AddToHashTable(hashtable);
		return hashtable;
	}

	public void Decode(Hashtable hashtable, Version version, bool adding)
	{
		_StoryPlayed.SetFromSaveData(hashtable, version, adding);
		_JVPLocked.SetFromSaveData(hashtable, version, adding);
		_FinishedOneLevel.SetFromSaveData(hashtable, version, adding);
		_IGTutorialGougeShown.SetFromSaveData(hashtable, version, adding);
		_IGTutorialSquashShown.SetFromSaveData(hashtable, version, adding);
		_IGTutorialBossShown.SetFromSaveData(hashtable, version, adding);
		_IGTutorialTrapShown.SetFromSaveData(hashtable, version, adding);
		string s = (string)hashtable["CurrentHill"];
		CurrentHill.Instance.ID = int.Parse(s);
		_optionMusicOn.SetFromSaveData(hashtable, version, adding);
		_optionSFXOn.SetFromSaveData(hashtable, version, adding);
		_optionTutorialsOn.SetFromSaveData(hashtable, version, adding);
		_lastTipShown.SetFromSaveData(hashtable, version);
		_lastTipShownML.SetFromSaveData(hashtable, version);
		_HasSpunJVP.SetFromSaveData(hashtable, version, adding);
		_HaveSeenGoodJVPItem.SetFromSaveData(hashtable, version, adding);
		_HavePressedGoodJVPItem.SetFromSaveData(hashtable, version, adding);
		_HasFlicked.SetFromSaveData(hashtable, version, adding);
		_HasBeenLoaded.SetFromSaveData(hashtable, version, adding);
		_HasBeenInstallTracked.SetFromSaveData(hashtable, version, adding);
		_HasPlayedStoryCompleteMovie.SetFromSaveData(hashtable, version, adding);
		_QuestsStarted.SetFromSaveData(hashtable, version, adding);
		for (int i = 0; i < _powerplayPicker.Length; i++)
		{
			_powerplayPicker[i].SetFromSaveData(hashtable, version, adding);
		}
		_hasUsedPowerPlayPicker.SetFromSaveData(hashtable, version, adding);
		NGUITools.soundVolume = ((!_optionSFXOn.Set) ? 0f : 1f);
	}

	public void ResetTutorialRelatedFlags()
	{
		_lastTipShown.Value = 0;
		_lastTipShownML.Value = 0;
		_HasSpunJVP.Set = false;
		_HaveSeenGoodJVPItem.Set = false;
		_HavePressedGoodJVPItem.Set = false;
		_HasFlicked.Set = false;
	}

	public string GetPowerPlayPickerString()
	{
		if (_powerplayPicker.Length < 4 || !_hasUsedPowerPlayPicker.Set)
		{
			return "NotUsed";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(string.Format("fl{0}", (!_powerplayPicker[0].Set) ? "N" : "Y"));
		stringBuilder.Append(string.Format("bo{0}", (!_powerplayPicker[1].Set) ? "N" : "Y"));
		stringBuilder.Append(string.Format("sh{0}", (!_powerplayPicker[2].Set) ? "N" : "Y"));
		stringBuilder.Append(string.Format("sp{0}", (!_powerplayPicker[3].Set) ? "N" : "Y"));
		return stringBuilder.ToString();
	}
}
