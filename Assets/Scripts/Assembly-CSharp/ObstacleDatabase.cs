using UnityEngine;

public class ObstacleDatabase : MonoBehaviour
{
	public static ObstacleDatabase Instance { get; private set; }

	public ObstacleDefinition[] ObstacleDefinitions { get; private set; }

	private void Awake()
	{
		if ((bool)Instance)
		{
			Debug.LogError("ObstacleDatabase created twice", base.gameObject);
		}
		Instance = this;
		LoadDefinitions();
	}

	private void LoadDefinitions()
	{
		Object[] array = Resources.LoadAll("Obstacles/Database");
		ObstacleDefinitions = new ObstacleDefinition[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = array[i] as GameObject;
			ObstacleDefinitions[i] = gameObject.GetComponent<ObstacleDefinition>();
		}
	}

	public ObstacleDefinition GetDefitnion(string name)
	{
		for (int i = 0; i < ObstacleDefinitions.Length; i++)
		{
			if (ObstacleDefinitions[i].name == name)
			{
				return ObstacleDefinitions[i];
			}
		}
		return null;
	}

	public ObstacleDefinition GetDefitnionByIndex(int index)
	{
		if (index < 0 || index >= ObstacleDefinitions.Length)
		{
			return null;
		}
		return ObstacleDefinitions[index];
	}
}
