using UnityEngine;

public class DestroyForNonIAP : MonoBehaviour
{
	private void Awake()
	{
		BuildDetails instance = BuildDetails.Instance;
		if (instance != null && !instance._HasIAP)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
