using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/Button Event")]
public class UIButtonEvent : MonoBehaviour
{
	public enum Trigger
	{
		OnClick = 0,
		OnPress = 1,
		OnRelease = 2
	}

	public delegate void TiggerHandler(string triggerID);

	public GameObject _Target;

	public UIEventType _EventType;

	public GameButtonType _GameButtonType;

	public Trigger _Trigger;

	private bool _started;

	private bool _highlighted;

	private string _triggerParam;

	[method: MethodImpl(32)]
	public event TiggerHandler Triggered;

	private void Start()
	{
		_started = true;
	}

	private void OnEnable()
	{
		if (_started && _highlighted)
		{
			OnClick();
		}
	}

	private void OnPress(bool isPressed)
	{
		if (base.enabled && ((isPressed && _Trigger == Trigger.OnPress) || (!isPressed && _Trigger == Trigger.OnRelease)))
		{
			Send();
		}
	}

	private void OnClick()
	{
		if (base.enabled && _Trigger == Trigger.OnClick)
		{
			Send();
		}
	}

	private void Send()
	{
		UIEvents.SendEvent(_EventType, _Target, _GameButtonType);
		if (this.Triggered != null)
		{
			this.Triggered((_triggerParam == null) ? base.name : _triggerParam);
		}
	}

	public void RegisterCallback(TiggerHandler callback)
	{
		this.Triggered = (TiggerHandler)Delegate.Combine(this.Triggered, callback);
	}

	public void SetCallbackParam(string callbackParam)
	{
		_triggerParam = callbackParam;
	}
}
