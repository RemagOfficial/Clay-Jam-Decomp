using UnityEngine;

public class HillBottomController : ManagedComponent
{
	private const string BossMonsterPositionName = "BossMonsterPosition";

	public GameObject _Landscape;

	public GameObject _Lights;

	public static Transform BossMonsterTransform;

	private GameObject _bossMonsterObject;

	private HillBottomRiser[] _bottomRisers;

	protected override void OnAwake()
	{
		FindBossMonsterPosition();
		GetBottomRiserRefs();
		InGameController.StateChanged += OnStateChanged;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnStateChanged;
	}

	private void Update()
	{
		if (InGameController.Instance.CurrentState != InGameController.State.RollingFinalMoments)
		{
			SetPosition();
		}
	}

	private void PositionBossMonster()
	{
		_bossMonsterObject = BossMonster.Instance.gameObject;
		_bossMonsterObject.transform.parent = BossMonsterTransform;
		_bossMonsterObject.transform.position = BossMonsterTransform.position;
		_bossMonsterObject.transform.rotation = BossMonsterTransform.rotation;
	}

	private bool FindBossMonsterPosition()
	{
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.gameObject.name == "BossMonsterPosition")
			{
				BossMonsterTransform = transform;
				return true;
			}
		}
		Debug.LogError(string.Format("HillBottomRiser {0} needs a {1} node", base.name, "BossMonsterPosition"));
		return false;
	}

	private void OnStateChanged(InGameController.State newState)
	{
		if (newState == InGameController.State.Flying)
		{
			TurnLandscapeOff();
			TurnLightingOff();
		}
	}

	public override void ResetForRun()
	{
		ResetPosition();
	}

	private void ResetPosition()
	{
		Vector3 position = new Vector3(0f, 0f, CurrentHill.Instance.Length);
		base.transform.position = position;
		HillBottomRiser[] bottomRisers = _bottomRisers;
		foreach (HillBottomRiser hillBottomRiser in bottomRisers)
		{
			hillBottomRiser.ResetForRun();
		}
		PositionBossMonster();
	}

	protected override void OnRunStarted()
	{
		if (CurrentGameMode.HasBottom)
		{
			TurnLandscapeOn();
		}
		else
		{
			TurnLandscapeOff();
		}
		TurnLightingOn();
	}

	private void GetBottomRiserRefs()
	{
		_bottomRisers = base.gameObject.GetComponentsInChildren<HillBottomRiser>();
	}

	private void TurnLandscapeOn()
	{
		SetPosition();
		_Landscape.SetActiveRecursively(true);
	}

	private void TurnLandscapeOff()
	{
		_Landscape.SetActiveRecursively(false);
	}

	private void TurnLightingOn()
	{
		_Lights.SetActiveRecursively(true);
	}

	private void TurnLightingOff()
	{
		_Lights.SetActiveRecursively(false);
	}

	private void SetPosition()
	{
		Vector3 position = base.transform.position;
		position.x = Pebble.Position.x;
		base.transform.position = position;
	}
}
