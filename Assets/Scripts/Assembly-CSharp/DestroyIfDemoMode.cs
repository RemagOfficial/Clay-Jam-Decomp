using UnityEngine;

public class DestroyIfDemoMode : MonoBehaviour
{
	private void Awake()
	{
		BuildDetails instance = BuildDetails.Instance;
		if (instance != null && instance._DemoMode)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
