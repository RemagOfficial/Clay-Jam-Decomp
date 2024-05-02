using UnityEngine;

public abstract class GameStatTextObject : MonoBehaviour
{
	public TextMesh _DistanceTextMesh;

	private Renderer[] _renderers;

	private string _FormatString;

	protected virtual string GetFormatString()
	{
		return "{0}";
	}

	private void Awake()
	{
		InGameController.StateChanged += OnInGameStateChanged;
		_renderers = GetComponentsInChildren<Renderer>();
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnInGameStateChanged;
	}

	private void OnInGameStateChanged(InGameController.State newState)
	{
		if (newState == InGameController.State.ResettingForRun)
		{
			_FormatString = GetFormatString();
			int stat = GetStat();
			if ((float)stat <= 0f)
			{
				TurnOffRenderers();
			}
			else
			{
				TurnOnRenderers();
			}
			_DistanceTextMesh.text = string.Format(_FormatString.ToUpper(), Localization.PunctuatedNumber(stat, 99999));
		}
	}

	protected abstract int GetStat();

	private void TurnOnRenderers()
	{
		Renderer[] renderers = _renderers;
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = true;
		}
	}

	private void TurnOffRenderers()
	{
		Renderer[] renderers = _renderers;
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = false;
		}
	}
}
