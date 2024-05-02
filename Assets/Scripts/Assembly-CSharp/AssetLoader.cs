using UnityEngine;

public class AssetLoader
{
	private static GameObject _inGameAudio;

	public static void LoadLevelAssets(int hillId)
	{
	}

	public static void UnloadLevelAssets()
	{
		Object.Destroy(_inGameAudio);
	}

	public static void LoadFrontendAssets()
	{
	}

	public static void UnloadFrontendAssets()
	{
	}
}
