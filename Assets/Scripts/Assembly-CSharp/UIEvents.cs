using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class UIEvents
{
	[method: MethodImpl(32)]
	public static event Action<GameObject> PushPanel;

	[method: MethodImpl(32)]
	public static event Action<GameObject> PopPanel;

	[method: MethodImpl(32)]
	public static event Action<GameObject> ResetToPanel;

	[method: MethodImpl(32)]
	public static event Action<GameObject> PanelTransitionOffFinished;

	[method: MethodImpl(32)]
	public static event Action NextPressed;

	[method: MethodImpl(32)]
	public static event Action<GameButtonType> ButtonPressed;

	[method: MethodImpl(32)]
	public static event Action<GameObject> ButtonPressed_WithTarget;

	[method: MethodImpl(32)]
	public static event Action StartGame;

	[method: MethodImpl(32)]
	public static event Action<GameObject> ReturnToFrontend;

	[method: MethodImpl(32)]
	public static event Action<GameObject> AwardPrize;

	public static void SendEvent(UIEventType eventType, GameObject target)
	{
		SendEvent(eventType, target, GameButtonType.None);
	}

	public static void SendEvent(UIEventType eventType, GameObject target, GameButtonType buttonType)
	{
		switch (eventType)
		{
		case UIEventType.PushPanel:
			if (UIEvents.PushPanel != null)
			{
				UIEvents.PushPanel(target);
			}
			break;
		case UIEventType.PopPanel:
			if (UIEvents.PopPanel != null)
			{
				UIEvents.PopPanel(target);
			}
			break;
		case UIEventType.ResetToPanel:
			if (UIEvents.ResetToPanel != null)
			{
				UIEvents.ResetToPanel(target);
			}
			break;
		case UIEventType.PanelTransitionOffFinished:
			if (UIEvents.PanelTransitionOffFinished != null)
			{
				UIEvents.PanelTransitionOffFinished(target);
			}
			break;
		case UIEventType.NextPressed:
			if (UIEvents.NextPressed != null)
			{
				UIEvents.NextPressed();
			}
			break;
		case UIEventType.ButtonPressed:
			if (UIEvents.ButtonPressed != null)
			{
				UIEvents.ButtonPressed(buttonType);
			}
			break;
		case UIEventType.StartGame:
			if (UIEvents.StartGame != null)
			{
				UIEvents.StartGame();
			}
			break;
		case UIEventType.ReturnToFrontend:
			if (UIEvents.ReturnToFrontend != null)
			{
				UIEvents.ReturnToFrontend(target);
			}
			break;
		case UIEventType.AwardPrize:
			if (UIEvents.AwardPrize != null)
			{
				UIEvents.AwardPrize(target);
			}
			break;
		case UIEventType.ButtonPressedTarget:
			if (UIEvents.ButtonPressed_WithTarget != null)
			{
				UIEvents.ButtonPressed_WithTarget(target);
			}
			break;
		case UIEventType.QuitApplication:
			Application.Quit();
			break;
		case UIEventType._unused_2:
			break;
		}
	}
}
