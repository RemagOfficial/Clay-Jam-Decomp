using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NGUIPanelManager : MonoBehaviour
{
	public enum TransitionType
	{
		activateRecursively = 0,
		activateParentPanelOnly = 1
	}

	private enum State
	{
		Idle = 0,
		WaitForTransitionInAnimToFinish = 1,
		WaitForTransitionOutAnimToFinish = 2
	}

	private const string TransitionInSubString = "TransIn";

	private const string TransitionOutSubString = "TransOut";

	public GameObject _LoadingPanel;

	public GameObject _ModeChoicePanel;

	private Stack<GameObject> PanelStack = new Stack<GameObject>();

	private bool _transitionNextPanelOn;

	private State _state;

	private List<Animation> _currentTransitionAnimation;

	private List<AnimationState> _currentTransitionAnimStates;

	public static NGUIPanelManager Instance { get; private set; }

	public GameObject TopPanel
	{
		get
		{
			if (PanelStack.Count > 0)
			{
				return PanelStack.Peek();
			}
			return null;
		}
	}

	private UIPanelPopup PopupPanel { get; set; }

	public bool CurrentPanelPreventsWorldInteraction
	{
		get
		{
			if (TopPanel == _ModeChoicePanel)
			{
				return true;
			}
			if (FrontendNGUI.Instance != null)
			{
				return FrontendNGUI.Instance.PanelPreventsWorldInteraction(TopPanel);
			}
			if (InGameNGUI.Instance != null)
			{
				return InGameNGUI.Instance.PanelPreventsWorldInteraction(TopPanel);
			}
			return false;
		}
	}

	[method: MethodImpl(32)]
	public static event Action<GameObject> PanelActivated;

	[method: MethodImpl(32)]
	public static event Action<GameObject> PanelClosed;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("NGUIPanelManager created twice. Here.", base.gameObject);
			Debug.LogError("and here", Instance.gameObject);
		}
		Instance = this;
		UIEvents.PushPanel += PushPanelOnStack;
		UIEvents.PopPanel += PopPanelFromStack;
		UIEvents.ResetToPanel += ResetToPanel;
		UIEvents.PanelTransitionOffFinished += PanelTransitionOffFinished;
		_state = State.Idle;
		_currentTransitionAnimation = new List<Animation>(4);
		_currentTransitionAnimStates = new List<AnimationState>(4);
	}

	private void OnDestroy()
	{
		UIEvents.PushPanel -= PushPanelOnStack;
		UIEvents.PopPanel -= PopPanelFromStack;
		UIEvents.ResetToPanel -= ResetToPanel;
		UIEvents.PanelTransitionOffFinished -= PanelTransitionOffFinished;
	}

	public void PushPanelOnStack(GameObject targetPanel)
	{
		if (!targetPanel)
		{
			Debug.LogError("ERROR! You're passing in a null panel, you silly sausage!");
		}
		if (PanelStack.Count > 0)
		{
			TurnPanelInteractionsOn(PanelStack.Peek(), false);
		}
		PanelStack.Push(targetPanel);
		_transitionNextPanelOn = true;
		TurnTopPanelOn();
	}

	public void PopPanelFromStack(GameObject targetPanel)
	{
		if (PanelStack.Count < 1)
		{
			Debug.LogError("ERROR! Any second now, you're going to be trying to look at an object in an empty stack. You won't like it.");
		}
		if (targetPanel != PanelStack.Peek())
		{
			Debug.LogError("ERROR! The object passed in is not the current object. It really should be, you know?");
		}
		if ((bool)PopupPanel && targetPanel == PopupPanel.gameObject)
		{
			OnPopupPanelClosed();
		}
		if (NGUIPanelManager.PanelClosed != null)
		{
			NGUIPanelManager.PanelClosed(targetPanel);
		}
		StartTurningTopPanelOff();
	}

	public void ResetToPanel(GameObject panel)
	{
		_transitionNextPanelOn = true;
		GameObject gameObject = null;
		if (PanelStack.Count > 0)
		{
			gameObject = PanelStack.Peek();
		}
		while (PanelStack.Count > 0)
		{
			GameObject gameObject2 = PanelStack.Peek();
			if (gameObject2 != gameObject)
			{
				DeactivatePanel(gameObject2);
			}
			PanelStack.Pop();
		}
		if (gameObject != null)
		{
			PanelStack.Push(panel);
			PanelStack.Push(gameObject);
			PopPanelFromStack(gameObject);
		}
		else
		{
			PushPanelOnStack(panel);
		}
	}

	public void ShowPopup(UIPanelPopup panel, NGUIPopUpData data)
	{
		PopupPanel = panel;
		if ((bool)PopupPanel && (bool)PopupPanel.gameObject)
		{
			PopupPanel.SetData(data);
			PushPanelOnStack(PopupPanel.gameObject);
		}
	}

	public void HidePopup()
	{
		if (PopupPanel != null)
		{
			PopPanelFromStack(PopupPanel.gameObject);
		}
	}

	private void PanelTransitionOffFinished(GameObject targetPanel)
	{
		if (targetPanel != PanelStack.Peek())
		{
			Debug.LogError("ERROR! The object passed in is not the current object. It really should be, you know?");
		}
		TurnTopPanelOff();
	}

	private void TurnPanelInteractionsOn(GameObject panel, bool on)
	{
		if ((bool)panel)
		{
			TransitionType transitionType = TransitionType.activateRecursively;
			NGUIPanelController component = panel.GetComponent<NGUIPanelController>();
			if (component != null)
			{
				transitionType = component._TransitionType;
			}
			if (transitionType != TransitionType.activateParentPanelOnly)
			{
				SetPanelInteractions(panel, on);
			}
			if (on && NGUIPanelManager.PanelActivated != null)
			{
				NGUIPanelManager.PanelActivated(panel);
			}
		}
	}

	private static void SetPanelInteractions(GameObject panel, bool enabled)
	{
		Collider[] componentsInChildren = panel.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			collider.enabled = enabled;
		}
	}

	private void TurnTopPanelOn()
	{
		ActivateTopPanel();
		if (_transitionNextPanelOn)
		{
			TransitionTopPanelOn();
			_transitionNextPanelOn = false;
		}
	}

	private void TransitionTopPanelOn()
	{
		GameObject gameObject = PanelStack.Peek();
		UIPanelTransitionBasic[] componentsInChildren = gameObject.GetComponentsInChildren<UIPanelTransitionBasic>();
		UIPanelTransitionBasic[] array = componentsInChildren;
		foreach (UIPanelTransitionBasic uIPanelTransitionBasic in array)
		{
			uIPanelTransitionBasic.TransitionOn();
		}
		bool flag = true;
		NGUIPanelController component = gameObject.GetComponent<NGUIPanelController>();
		if (component != null)
		{
			flag = !component._SurpressAutoAnimations;
		}
		if (flag)
		{
			Animation[] componentsInChildren2 = gameObject.GetComponentsInChildren<Animation>();
			Animation[] array2 = componentsInChildren2;
			foreach (Animation animation in array2)
			{
				foreach (AnimationState item in animation)
				{
					if (item.name.Contains("TransIn"))
					{
						item.time = 0f;
						item.speed = 1f;
						animation.Play(item.name);
						_currentTransitionAnimation.Add(animation);
						_currentTransitionAnimStates.Add(item);
					}
				}
			}
		}
		_state = State.WaitForTransitionInAnimToFinish;
	}

	private void StartTurningTopPanelOff()
	{
		GameObject gameObject = PanelStack.Peek();
		UIPanelTransitionBasic[] componentsInChildren = gameObject.GetComponentsInChildren<UIPanelTransitionBasic>();
		UIPanelTransitionBasic[] array = componentsInChildren;
		foreach (UIPanelTransitionBasic uIPanelTransitionBasic in array)
		{
			uIPanelTransitionBasic.TransitionOff();
		}
		bool flag = true;
		NGUIPanelController component = gameObject.GetComponent<NGUIPanelController>();
		if (component != null)
		{
			flag = !component._SurpressAutoAnimations;
		}
		if (flag)
		{
			Animation[] componentsInChildren2 = gameObject.GetComponentsInChildren<Animation>();
			Animation[] array2 = componentsInChildren2;
			foreach (Animation animation in array2)
			{
				foreach (AnimationState item in animation)
				{
					if (item.name.Contains("TransOut"))
					{
						item.time = 0f;
						item.speed = 1f;
						animation.Play(item.name);
						_currentTransitionAnimation.Add(animation);
						_currentTransitionAnimStates.Add(item);
					}
				}
			}
		}
		TurnPanelInteractionsOn(gameObject, false);
		_state = State.WaitForTransitionOutAnimToFinish;
	}

	private void Update()
	{
		if (_state == State.Idle)
		{
			return;
		}
		if (_currentTransitionAnimStates.Count > 0)
		{
			bool flag = true;
			for (int i = 0; i < _currentTransitionAnimStates.Count; i++)
			{
				if ((bool)_currentTransitionAnimation[i] && _currentTransitionAnimation[i].IsPlaying(_currentTransitionAnimStates[i].name))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
		}
		if (_state == State.WaitForTransitionOutAnimToFinish)
		{
			GameObject gameObject = PanelStack.Peek();
			UIPanelTransitionBasic[] componentsInChildren = gameObject.GetComponentsInChildren<UIPanelTransitionBasic>();
			if (componentsInChildren.Length == 0)
			{
				TurnTopPanelOff();
				_currentTransitionAnimation.Clear();
				_currentTransitionAnimStates.Clear();
				_state = State.Idle;
			}
		}
		else if (_state == State.WaitForTransitionInAnimToFinish)
		{
			_currentTransitionAnimation.Clear();
			_currentTransitionAnimStates.Clear();
			_state = State.Idle;
		}
	}

	private void TurnTopPanelOff()
	{
		DeactivateTopPanel();
		PanelStack.Pop();
		if (PanelStack.Count > 0)
		{
			TurnTopPanelOn();
		}
	}

	private void DeactivateTopPanel()
	{
		DeactivatePanel(PanelStack.Peek());
	}

	private void ActivateTopPanel()
	{
		ActivatePanel(PanelStack.Peek());
	}

	private void ActivatePanel(GameObject panel)
	{
		TransitionType transitionType = TransitionType.activateRecursively;
		NGUIPanelController component = panel.GetComponent<NGUIPanelController>();
		if (component != null)
		{
			transitionType = component._TransitionType;
		}
		if (transitionType == TransitionType.activateParentPanelOnly)
		{
			panel.active = true;
			return;
		}
		panel.SetActiveRecursively(true);
		TurnPanelInteractionsOn(panel, true);
	}

	private void DeactivatePanel(GameObject panel)
	{
		panel.SetActiveRecursively(false);
	}

	private void OnPopupPanelClosed()
	{
		if (PopupPanel.OnClosedCallback != null)
		{
			PopupPanel.OnClosedCallback();
		}
		PopupPanel = null;
	}
}
