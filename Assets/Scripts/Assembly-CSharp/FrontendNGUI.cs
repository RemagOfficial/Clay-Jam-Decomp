using UnityEngine;

public class FrontendNGUI : ManagedComponent
{
	public GameObject _StartPanel;

	public GameObject _JVPPanel;

	public GameObject _PreGamePanel;

	public GameObject _StoryPanel;

	public GameObject _MenuPanel;

	public GameObject _CreditsPanel;

	public GameObject _MapScreenPanel;

	public GameObject _IAPPanel;

	public static FrontendNGUI Instance { get; private set; }

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of FrontendNGUI", base.gameObject);
		}
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	protected override void OnRunStarted()
	{
		NGUIPanelManager.Instance.ResetToPanel(_StartPanel);
	}

	public bool PanelPreventsWorldInteraction(GameObject panel)
	{
		return panel == _IAPPanel || panel == _MenuPanel || panel == _CreditsPanel;
	}
}
