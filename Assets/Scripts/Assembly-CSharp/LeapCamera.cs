using UnityEngine;

public class LeapCamera : MonoBehaviour
{
	private static LeapCamera _instance;

	public static Camera Camera
	{
		get
		{
			return _instance.gameObject.camera;
		}
	}

	private void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("More than one leap camera", base.gameObject);
		}
		_instance = this;
		if (Camera == null)
		{
			Debug.LogError("No cmaera component on LeapCamera object");
		}
	}
}
