using System.Collections.Generic;
using UnityEngine;

public class FlyingLandscape : ManagedComponent
{
	private const int NumSectionsLandscape = 5;

	private const int NumSectionsPortrait = 3;

	public const float SectionLength = 63.99f;

	public const float HalfSectionLength = 31.995f;

	public Object _SideMountainsPrefab;

	public GameObject _Sky;

	public Object _MountainLayerPrefab;

	public Object _PrizeStickPrefab;

	public float _PrizeStickParallax;

	public List<GameObject> _mountainLayersPrefabs;

	private float prevPebbleZ;

	private List<GameObject> _MountainLayers;

	public static FlyingLandscape Instance { get; private set; }

	private int NumSections
	{
		get
		{
			return (Screen.height <= Screen.width) ? 5 : 3;
		}
	}

	public bool GivingPrize { get; set; }

	public GameObject PrizeStickObject { get; private set; }

	protected override void OnAwake()
	{
		InGameController.StateChanged += OnStateChange;
		if (Instance != null)
		{
			Debug.LogError("More than one Flyinglandscape", base.gameObject);
		}
		Instance = this;
	}

	protected override bool DoInitialise()
	{
		LoadMountains();
		InitialiseMountains();
		LoadStick();
		TurnOff();
		GivingPrize = false;
		return true;
	}

	private void OnDestroy()
	{
		Instance = null;
		InGameController.StateChanged -= OnStateChange;
	}

	private void OnStateChange(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.Flying:
			TurnOn();
			break;
		case InGameController.State.ResettingForRun:
			TurnOff();
			break;
		}
	}

	private void Update()
	{
		Vector3 position = Pebble.Position;
		Vector3 position2 = _Sky.transform.position;
		position2.z = position.z;
		_Sky.transform.position = position2;
		float num = (position.z - prevPebbleZ) * _PrizeStickParallax;
		prevPebbleZ = position.z;
		Vector3 position3 = PrizeStickObject.transform.position;
		position3.z += num;
		PrizeStickObject.transform.position = position3;
	}

	private void TurnOn()
	{
		base.gameObject.SetActiveRecursively(true);
		base.enabled = true;
		Vector3 position = new Vector3(Pebble.Position.x, 0f, CurrentHill.Instance.Length);
		base.transform.position = position;
		ResetStick();
		for (int i = 0; i < _MountainLayers.Count; i++)
		{
			_MountainLayers[i].GetComponent<FlyingLandscapeLayer>().Reset();
			_MountainLayers[i].SetActiveRecursively(true);
		}
	}

	private void TurnOff()
	{
		base.gameObject.SetActiveRecursively(false);
		base.enabled = false;
		for (int i = 0; i < _MountainLayers.Count; i++)
		{
			_MountainLayers[i].SetActiveRecursively(false);
		}
	}

	private void LoadStick()
	{
		PrizeStickObject = Object.Instantiate(_PrizeStickPrefab) as GameObject;
		PrizeStickObject.transform.parent = base.gameObject.transform;
	}

	private void LoadMountains()
	{
		_MountainLayers = new List<GameObject>(_mountainLayersPrefabs.Count);
		for (int i = 0; i < _mountainLayersPrefabs.Count; i++)
		{
			GameObject item = LoadMountainLayer(_mountainLayersPrefabs[i], (float)i / (float)_mountainLayersPrefabs.Count);
			_MountainLayers.Add(item);
		}
	}

	private GameObject LoadMountainLayer(Object layerPrefab, float factor)
	{
		GameObject gameObject = Object.Instantiate(_MountainLayerPrefab) as GameObject;
		gameObject.transform.parent = base.transform;
		FlyingLandscapeLayer component = gameObject.GetComponent<FlyingLandscapeLayer>();
		component.parallaxFactor = factor;
		component.Init(layerPrefab, NumSections);
		return gameObject;
	}

	private GameObject LoadMountain()
	{
		GameObject gameObject = Object.Instantiate(_SideMountainsPrefab) as GameObject;
		gameObject.transform.parent = base.transform;
		return gameObject;
	}

	private void InitialiseMountains()
	{
		HSVFastMaterialInitialiser.InitialiseChildRenderers(base.gameObject);
	}

	private void ResetStick()
	{
		float bestScore = CurrentHill.Instance.ProgressData._BestScore;
		if (bestScore != 0f)
		{
			float num = bestScore;
			Vector3 position = Pebble.Position;
			prevPebbleZ = position.z;
			Vector3 position2 = PrizeStickObject.transform.position;
			position2.y = 0f;
			position2.x = 0f;
			position2.z = position.z + num * (1f - _PrizeStickParallax);
			PrizeStickObject.transform.position = position2;
		}
		else
		{
			PrizeStickObject.SetActiveRecursively(false);
		}
	}
}
