using UnityEngine;

public class JVPComponentManager : ComponentManager
{
	public static JVPComponentManager Instance { get; private set; }

	protected override void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one JVPComponentManager instance", base.gameObject);
		}
		Instance = this;
		base.Awake();
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}
