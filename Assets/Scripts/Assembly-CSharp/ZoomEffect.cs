using UnityEngine;

public class ZoomEffect : MonoBehaviour
{
	public string[] _AnimationNames;

	private Renderer[] _renderers;

	private void Awake()
	{
		ZoomLevel.NewZoomLevelEvent += OnNewZoomLevel;
		_renderers = GetComponentsInChildren<Renderer>();
	}

	private void Update()
	{
		base.transform.position = Pebble.Position;
		if (!base.animation.isPlaying)
		{
			EnableRendering(false);
		}
	}

	public void OnDestroy()
	{
		ZoomLevel.NewZoomLevelEvent -= OnNewZoomLevel;
	}

	public void StartRun()
	{
		EnableRendering(false);
	}

	private void OnNewZoomLevel(int level, bool zoomOut)
	{
		EnableRendering(true);
		int num = level;
		if (zoomOut)
		{
			num--;
		}
		DoEffect(level);
	}

	private void DoEffect(int level)
	{
	}

	private void EnableRendering(bool enabled)
	{
		Renderer[] renderers = _renderers;
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = enabled;
		}
	}
}
