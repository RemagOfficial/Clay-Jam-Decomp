using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LoadingScreenCamera : MonoBehaviour
{
	private void Awake()
	{
		MetaGameController.LoadingScreenCamera = base.gameObject;
		base.gameObject.camera.enabled = false;
	}
}
