using System.Collections.Generic;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
	private const string DEMO_TEXT_LOCALISATION_KEY = "TIP_DEMO";

	public string _LocalisationKeyBase;

	public int _NumBeginnersTips;

	public int _NumTipsTotal;

	public string _LocalisationKeyBaseML;

	public int _NumBeginnersTipsML;

	public int _NumTipsTotalML;

	public List<UIGameModeLoading> _ModeObjects;

	public LocalisableText _TipText;

	public LocalisableText _LevelNameText;

	private int _currentTip;

	private int _currentTipML;

	public List<int> _IAPSpecificTips;

	public List<int> _IAPSpecificTipsML;

	private void Start()
	{
		_currentTip = SaveData.Instance.Progress._lastTipShown.Value;
		_currentTipML = SaveData.Instance.Progress._lastTipShownML.Value;
		ShowForFirstLoad();
	}

	private void OnEnable()
	{
		if (MetaGameController.Instance.LoadingScreenHasTips)
		{
			ShowForLoadingGame();
		}
		else
		{
			ShowForLoadingFrontend();
		}
	}

	private void PickNextTip()
	{
		if (CurrentGameMode.Type == GameModeType.Quest)
		{
			_currentTip++;
			if (_currentTip >= _NumTipsTotal)
			{
				_currentTip = _NumBeginnersTips;
			}
			if (!BuildDetails.Instance._HasIAP)
			{
				while (_IAPSpecificTips.Contains(_currentTip))
				{
					_currentTip++;
					if (_currentTip >= _NumTipsTotal)
					{
						_currentTip = _NumBeginnersTips;
					}
				}
			}
			if (_currentTip < 10)
			{
				_TipText.text = Localization.instance.Get(_LocalisationKeyBase + "0" + _currentTip);
			}
			else
			{
				_TipText.text = Localization.instance.Get(_LocalisationKeyBase + _currentTip);
			}
			if ((bool)SaveData.Instance)
			{
				SaveData.Instance.Progress._lastTipShown.Value = _currentTip;
				SaveData.Instance.Save();
			}
			return;
		}
		_currentTipML++;
		if (_currentTipML >= _NumTipsTotalML)
		{
			_currentTipML = _NumBeginnersTipsML;
		}
		if (!BuildDetails.Instance._HasIAP)
		{
			while (_IAPSpecificTipsML.Contains(_currentTipML))
			{
				_currentTipML++;
				if (_currentTipML >= _NumTipsTotal)
				{
					_currentTipML = _NumBeginnersTips;
				}
			}
		}
		if (_currentTipML < 10)
		{
			_TipText.text = Localization.instance.Get(_LocalisationKeyBaseML + "0" + _currentTipML);
		}
		else
		{
			_TipText.text = Localization.instance.Get(_LocalisationKeyBaseML + _currentTipML);
		}
		if ((bool)SaveData.Instance)
		{
			SaveData.Instance.Progress._lastTipShownML.Value = _currentTipML;
			SaveData.Instance.Save();
		}
	}

	private void Update()
	{
	}

	private void ShowForFirstLoad()
	{
		if (BuildDetails.Instance._DemoMode)
		{
			_TipText.text = Localization.instance.Get("TIP_DEMO");
		}
		else
		{
			HideTips();
		}
		ShowLevelText(Localization.instance.Get("Game_Title"));
	}

	private void ShowForLoadingGame()
	{
		if (BuildDetails.Instance._DemoMode)
		{
			_TipText.text = Localization.instance.Get("TIP_DEMO");
			ShowLevelText(Localization.instance.Get("Game_Title"));
		}
		else
		{
			PickNextTip();
			ShowLevelText(Localization.instance.Get(CurrentHill.Instance.Definition._Name));
		}
		foreach (UIGameModeLoading modeObject in _ModeObjects)
		{
			modeObject.SetupForGame();
		}
	}

	private void ShowForLoadingFrontend()
	{
		HideTips();
		HideLevelText();
		foreach (UIGameModeLoading modeObject in _ModeObjects)
		{
			modeObject.SetupForFrontend();
		}
	}

	private void HideTips()
	{
		_TipText.text = string.Empty;
	}

	private void HideLevelText()
	{
		_LevelNameText.text = string.Empty;
	}

	private void ShowLevelText(string text)
	{
		_LevelNameText.text = text;
	}
}
