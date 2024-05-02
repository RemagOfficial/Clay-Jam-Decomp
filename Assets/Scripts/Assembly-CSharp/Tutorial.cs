using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
	public GameObject _PopupObject;

	public GameObject _SecondPopupObject;

	public List<TutorialTigger.Type> _ShowTriggers;

	public List<TutorialTigger.Type> _HideTriggers;

	public TutorialCriteria _Criteria;

	public TutorialSideEffects _SideEffects;

	private TutorialPopup _popup;

	private TutorialPopup _popup2;

	private bool _registeredTrigger;

	public float hideDelay;

	public TutorialData SavedData { get; private set; }

	public bool IsShowing
	{
		get
		{
			return (bool)_PopupObject && _PopupObject.active;
		}
	}

	private void Awake()
	{
		_popup = base.gameObject.AddComponent<TutorialPopup>();
		_popup.SetPanelObject(_PopupObject);
		if (_SecondPopupObject != null)
		{
			_popup2 = base.gameObject.AddComponent<TutorialPopup>();
			_popup2.SetPanelObject(_SecondPopupObject);
		}
		_Criteria.Tutorial = this;
	}

	private void OnDisable()
	{
		StopListeningForTrigger();
	}

	public void Reset()
	{
		SavedData = SaveData.Instance.Tutorials.GetTutorialData(base.gameObject.name);
		if (_Criteria.WillEverShowAgain())
		{
			StartListeningForTrigger();
		}
		else
		{
			StopListeningForTrigger();
		}
	}

	private void OnShow()
	{
		if (_Criteria.ShouldShowNow())
		{
			_SideEffects.OnShow();
			_popup.Show();
			if ((bool)_popup2)
			{
				_popup2.Show();
			}
			StopListeningForTrigger();
			StartListeningForHide();
		}
	}

	private void OnHide()
	{
		_SideEffects.OnHide();
		_popup.Hide(hideDelay);
		if ((bool)_popup2)
		{
			_popup2.Hide(hideDelay);
		}
		StopListeningForHide();
		Reset();
		SavedData.NumPlays++;
	}

	private void OnComplete()
	{
		StopListeningForTrigger();
	}

	private void StartListeningForTrigger()
	{
		if (_registeredTrigger)
		{
			return;
		}
		foreach (TutorialTigger.Type showTrigger in _ShowTriggers)
		{
			TutorialTigger.RegisterHandler(showTrigger, OnShow);
		}
		_registeredTrigger = true;
	}

	private void StopListeningForTrigger()
	{
		if (!_registeredTrigger)
		{
			return;
		}
		foreach (TutorialTigger.Type showTrigger in _ShowTriggers)
		{
			TutorialTigger.UnregisterHandler(showTrigger, OnShow);
		}
		_registeredTrigger = false;
	}

	private void StartListeningForHide()
	{
		foreach (TutorialTigger.Type hideTrigger in _HideTriggers)
		{
			TutorialTigger.RegisterHandler(hideTrigger, OnHide);
		}
	}

	private void StopListeningForHide()
	{
		foreach (TutorialTigger.Type hideTrigger in _HideTriggers)
		{
			TutorialTigger.UnregisterHandler(hideTrigger, OnHide);
		}
	}
}
