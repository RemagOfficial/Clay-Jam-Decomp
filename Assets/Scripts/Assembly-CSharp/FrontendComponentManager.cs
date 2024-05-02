using UnityEngine;

public class FrontendComponentManager : ComponentManager
{
	public static FrontendComponentManager Instance { get; private set; }

	protected override void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one FrontendComponentManager instance", base.gameObject);
		}
		Instance = this;
		base.Awake();
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}
