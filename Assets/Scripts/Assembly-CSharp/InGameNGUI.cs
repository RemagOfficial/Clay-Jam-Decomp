using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InGameNGUI : MonoBehaviour
{
	public GameObject _MainPanel;

	public GameObject _PausePanel;

	public GameObject _JVPPanel;

	public GameObject _IAPPanel;

	public static InGameNGUI Instance { get; private set; }

	[method: MethodImpl(32)]
	public static event Action PauseMenuShownEvent;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.Log("More than one instance of InGameNGUI", base.gameObject);
		}
		Instance = this;
		InGameController.StateChanged += OnInGameStateChanged;
		UIEvents.ButtonPressed += OnButtonPressed;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnInGameStateChanged;
		UIEvents.ButtonPressed -= OnButtonPressed;
		Instance = null;
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause && InGameController.Instance.IsRolling && !InGameController.Instance.Paused)
		{
			GoToPauseMenu();
		}
	}

	private void OnInGameStateChanged(InGameController.State newState)
	{
		if (newState == InGameController.State.WaitingToRoll)
		{
			NGUIPanelManager.Instance.ResetToPanel(_MainPanel);
		}
		if (newState == InGameController.State.JVP)
		{
			NGUIPanelManager.Instance.ResetToPanel(_JVPPanel);
		}
	}

	private void OnButtonPressed(GameButtonType buttonType)
	{
		switch (buttonType)
		{
		case GameButtonType.Pause:
			GoToPauseMenu();
			break;
		case GameButtonType.Back:
			if (NGUIPanelManager.Instance.TopPanel == _JVPPanel)
			{
				NGUIPanelManager.Instance.ResetToPanel(_MainPanel);
			}
			else if (NGUIPanelManager.Instance.TopPanel == _IAPPanel)
			{
				if (StaticIAPItems.Instance._BackButton.active)
				{
					NGUIPanelManager.Instance.ResetToPanel(_MainPanel);
				}
			}
			else if (NGUIPanelManager.Instance.TopPanel == NGUIPanelManager.Instance._ModeChoicePanel)
			{
				NGUIPanelManager.Instance.ResetToPanel(_MainPanel);
			}
			break;
		}
	}

	public void GoToPauseMenu()
	{
		InGameController.Instance.Pause();
		NGUIPanelManager.Instance.ResetToPanel(_PausePanel);
		if (InGameNGUI.PauseMenuShownEvent != null)
		{
			InGameNGUI.PauseMenuShownEvent();
		}
	}

	public bool PanelPreventsWorldInteraction(GameObject panel)
	{
		return panel == _IAPPanel || panel == _PausePanel;
	}
}
