using UnityEngine;

public class ObstacleParticles : MonoBehaviour
{
	private static string BlingParticleResourcePath = "Obstacles/Particles/Bling";

	private static string SplatParticleResourcePath = "Obstacles/Prefabs/GroundSplat";

	private static string PowerupParticleResourcePath = "Obstacles/Particles/PowerPlayParticle";

	private static string X2ParticleResourcePath = "Obstacles/Particles/X2particle";

	private static string X4ParticleResourcePath = "Obstacles/Particles/X4particle";

	private static Object BlingPrefab;

	private static Object SplatPrefab;

	private ParticleEmitter splatEmitter;

	private static Object PowerupPrefab;

	private static Object X2Prefab;

	private static Object X4Prefab;

	private ObstacleSplat _splat;

	public HSVColour Colour { get; set; }

	private Transform CharacterTransform { get; set; }

	public void Initialise()
	{
		LoadPrefabs();
		GetCharacterTransform();
		InitialseSplat();
	}

	public void DoPowerupSplat()
	{
		Vector3 position = CharacterTransform.position;
		position.y = 0.1f;
		Object.Instantiate(PowerupPrefab, position, base.transform.rotation);
	}

	public void DoSplat(float normalisedSize)
	{
		Vector3 position = CharacterTransform.position;
		position.y = 0.1f;
		_splat.Go(normalisedSize, Colour, position, base.transform.rotation);
	}

	public void DoX2()
	{
		Object.Instantiate(X2Prefab, CharacterTransform.position, CharacterTransform.rotation);
	}

	public void DoX4()
	{
		Object.Instantiate(X4Prefab, CharacterTransform.position, CharacterTransform.rotation);
	}

	public void DoBling()
	{
		GameObject gameObject = Object.Instantiate(BlingPrefab, CharacterTransform.position, CharacterTransform.rotation) as GameObject;
		gameObject.transform.parent = CharacterTransform;
	}

	public void MarkSquashable()
	{
		DoBling();
	}

	private void LoadPrefabs()
	{
		if (BlingPrefab == null)
		{
			BlingPrefab = Resources.Load(BlingParticleResourcePath);
			PowerupPrefab = Resources.Load(PowerupParticleResourcePath);
			SplatPrefab = Resources.Load(SplatParticleResourcePath);
			X2Prefab = Resources.Load(X2ParticleResourcePath);
			X4Prefab = Resources.Load(X4ParticleResourcePath);
		}
	}

	private void GetCharacterTransform()
	{
		ObstacleVisuals componentInChildren = GetComponentInChildren<ObstacleVisuals>();
		CharacterTransform = componentInChildren.CharacterTransform;
	}

	private void InitialseSplat()
	{
		GameObject gameObject = Object.Instantiate(SplatPrefab) as GameObject;
		gameObject.transform.parent = InGameComponentManager.Instance.transform;
		_splat = gameObject.GetComponent<ObstacleSplat>();
	}
}
