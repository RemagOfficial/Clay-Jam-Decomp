using UnityEngine;

public class ScreenCursorTimed : MonoBehaviour
{
	public ClayJamInput.CursorDisplayType _Type;

	public Renderer[] _Renderers;

	public AnimatedSprite _Timer;

	private void Awake()
	{
		TurnOn(false);
	}

	private void Start()
	{
		if (!ClayJamInput.ShowCursorEver)
		{
			Disable();
		}
	}

	private void Update()
	{
		if (!(Camera.current == null))
		{
			if (ClayJamInput.CurrentCursorType != _Type)
			{
				TurnOn(false);
				return;
			}
			if (!ClayJamInput.ShowCursorNow)
			{
				TurnOn(false);
				return;
			}
			TurnOn(true);
			SetWorldPositionFromScreenPosition(ClayJamInput.CursorScreenPosition);
		}
	}

	private void SetWorldPositionFromScreenPosition(Vector3 screenPos)
	{
		Ray ray = LeapCamera.Camera.ScreenPointToRay(screenPos);
		float enter;
		if (new Plane(Vector3.forward, 0f).Raycast(ray, out enter))
		{
			Vector3 point = ray.GetPoint(enter);
			base.transform.position = point;
		}
	}

	private void TurnOn(bool on)
	{
		Renderer[] renderers = _Renderers;
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = on;
		}
		UpdateTimer(on && ClayJamInput.CursorActive);
	}

	private void Disable()
	{
		TurnOn(false);
		base.gameObject.SetActive(false);
	}

	private void UpdateTimer(bool active)
	{
		if (active)
		{
			_Timer.renderer.enabled = true;
			_Timer.ShowFrameAtNormalisedTime(ClayJamInput.CursorTimer);
		}
		else
		{
			_Timer.renderer.enabled = false;
			_Timer.ShowFrameAtNormalisedTime(0f);
		}
	}
}
