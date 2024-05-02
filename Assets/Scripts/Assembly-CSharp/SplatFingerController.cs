using UnityEngine;

public class SplatFingerController : MonoBehaviour
{
	public Camera _GUICamera;

	private void Awake()
	{
		SplatInput.SplatFingerTouchDown += OnSplatFinger;
		InGameController.StateChanged += OnStateChanged;
	}

	private void OnDestroy()
	{
		SplatInput.SplatFingerTouchDown -= OnSplatFinger;
		InGameController.StateChanged -= OnStateChanged;
	}

	private void OnSplatFinger(Vector2 screenPos)
	{
		Vector3 position = _GUICamera.ScreenToWorldPoint(screenPos);
		base.transform.position = position;
		if (!base.gameObject.active)
		{
			base.gameObject.SetActiveRecursively(true);
		}
		base.animation.Play();
	}

	private void OnStateChanged(InGameController.State newState)
	{
		if (newState == InGameController.State.ResettingForRun)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}
}
