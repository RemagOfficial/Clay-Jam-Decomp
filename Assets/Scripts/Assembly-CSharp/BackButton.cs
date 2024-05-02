using UnityEngine;

public class BackButton : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			UIEvents.SendEvent(UIEventType.ButtonPressed, null, GameButtonType.Back);
		}
	}
}
