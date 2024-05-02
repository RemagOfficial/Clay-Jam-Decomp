using UnityEngine;

public class HillInputCursor : MonoBehaviour
{
	public ClayJamInput.CursorDisplayType _Type;

	public Renderer[] _Renderers;

	private void Update()
	{
		if (ClayJamInput.CurrentCursorType != _Type)
		{
			TurnOn(false);
			return;
		}
		base.transform.position = HillInput.Instance.CurrentTouchGroundPos;
		TurnOn(true);
	}

	private void TurnOn(bool on)
	{
		Renderer[] renderers = _Renderers;
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = on;
		}
	}
}
