using UnityEngine;

public class HillComponentManager : ComponentManager
{
	public static HillComponentManager Instance { get; private set; }

	protected override void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one InGameComponent instance", base.gameObject);
		}
		Instance = this;
		base.Awake();
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}
