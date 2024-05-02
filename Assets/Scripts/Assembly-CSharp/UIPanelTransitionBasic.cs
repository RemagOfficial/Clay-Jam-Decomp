using UnityEngine;

public class UIPanelTransitionBasic : MonoBehaviour
{
	public UIEventType _EventOnTransitionOn;

	public GameObject _Panel;

	public virtual void TransitionOn()
	{
		if (_EventOnTransitionOn != 0)
		{
			UIEvents.SendEvent(_EventOnTransitionOn, _Panel);
		}
	}

	public virtual void TransitionOff()
	{
		UIEvents.SendEvent(UIEventType.PanelTransitionOffFinished, _Panel);
	}
}
