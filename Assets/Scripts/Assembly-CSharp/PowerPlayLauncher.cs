using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PowerPlayLauncher : MonoBehaviour
{
	private const string PowerPlayLaunchAnim = "Launch";

	private Vector3 _spawnPoint;

	[method: MethodImpl(32)]
	public static event Action PowerPlayLaunched;

	public void Awake()
	{
		base.animation.playAutomatically = true;
	}

	public void SetColoursToMatchHil()
	{
		base.animation.playAutomatically = false;
		base.gameObject.SetActiveRecursively(true);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material material = renderer.material;
			CurrentHill.Instance.Definition._PowerPlayColour.UseOnHSVMaterial(material);
		}
		base.gameObject.SetActiveRecursively(false);
		base.animation.playAutomatically = true;
	}

	public void Launch()
	{
		Vector3 vector = Pebble.Instance.Velocity * 2f;
		_spawnPoint = Pebble.Instance.transform.position + vector;
		_spawnPoint.z = Mathf.Max(Pebble.Instance.transform.position.z + 3f, _spawnPoint.z);
		float min = 2f - CurrentHill.Instance.Definition._HillHalfWidth;
		float max = CurrentHill.Instance.Definition._HillHalfWidth - 2f;
		_spawnPoint.x = Mathf.Clamp(_spawnPoint.x, min, max);
		base.transform.position = _spawnPoint;
		base.gameObject.SetActiveRecursively(true);
		if (PowerPlayLauncher.PowerPlayLaunched != null)
		{
			PowerPlayLauncher.PowerPlayLaunched();
		}
		Pebble.Instance._PowerPlayActive = true;
		CurrentHill.Instance.ProgressData.PowerPlayUsed();
	}

	public bool UpdateAfterLaunch()
	{
		if (!base.animation.IsPlaying("Launch"))
		{
			SpawnPowerPlay();
			base.gameObject.SetActiveRecursively(false);
			return false;
		}
		return true;
	}

	private void SpawnPowerPlay()
	{
		ObstacleMould obstacleFromPool = HillObstacles.Instance.GetObstacleFromPool("PowerPlay");
		obstacleFromPool.Visuals.SetColour(CurrentHill.Instance.Definition._PowerPlayColour);
		obstacleFromPool.Spawn(_spawnPoint);
	}
}
