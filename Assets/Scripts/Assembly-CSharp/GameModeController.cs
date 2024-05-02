using UnityEngine;

public class GameModeController : ManagedComponent
{
	public static GameModeController Instance { get; private set; }

	public GameModeState GameMode { get; private set; }

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.Log("More than one GameMode Instance");
		}
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public override void ResetForRun()
	{
		if (GameMode != null)
		{
			GameMode.enabled = false;
		}
		switch (CurrentGameMode.Type)
		{
		case GameModeType.Quest:
			GameMode = GetComponent<GameModeStateQuest>();
			break;
		case GameModeType.MonsterLove:
			GameMode = GetComponent<GameModeStateMonsterLove>();
			break;
		default:
			GameMode = null;
			break;
		}
		if (GameMode != null)
		{
			GameMode.enabled = true;
			GameMode.OnResetForRun();
		}
	}
}
