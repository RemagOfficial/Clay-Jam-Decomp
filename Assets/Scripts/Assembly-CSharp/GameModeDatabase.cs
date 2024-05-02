using UnityEngine;

public class GameModeDatabase : MonoBehaviour
{
	public MonsterLoveDatabase _MonsterLove;

	public static GameModeDatabase Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.Log("More than one instance of GameModeDatabase");
		}
		Instance = this;
	}
}
