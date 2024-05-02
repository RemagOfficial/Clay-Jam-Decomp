using UnityEngine;

public class HillDatabase : MonoBehaviour
{
	public float _ShowHorizonDistance;

	public float _FinalMomentsDistance;

	public float _GougeCapDistance;

	public float _GougeBoostZoneDistance;

	public static HillDatabase Instance { get; private set; }

	private HillDefinition[] Definitions { get; set; }

	public static int NumHills
	{
		get
		{
			return Instance.Definitions.Length;
		}
	}

	private void Awake()
	{
		if ((bool)Instance)
		{
			Debug.LogError("HillDatabase created twice", base.gameObject);
		}
		Instance = this;
		LoadDefintions();
	}

	private void LoadDefintions()
	{
		Object[] array = Resources.LoadAll("Hills/AllHills");
		Definitions = new HillDefinition[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = array[i] as GameObject;
			Definitions[i] = gameObject.GetComponent<HillDefinition>();
			Definitions[i]._PebbleHandlingParams.CalculateConstants();
		}
	}

	public HillDefinition GetDefintionFromIndex(int index)
	{
		return Definitions[index];
	}

	public HillDefinition GetDefinitionFromID(int hillID)
	{
		HillDefinition[] definitions = Definitions;
		foreach (HillDefinition hillDefinition in definitions)
		{
			if (hillDefinition._ID == hillID)
			{
				return hillDefinition;
			}
		}
		Debug.LogError(string.Format("Hill ID {0} not found", hillID), base.gameObject);
		return null;
	}

	public int GetIndexForID(int ID)
	{
		for (int i = 0; i < Definitions.Length; i++)
		{
			if (Definitions[i]._ID == CurrentHill.Instance.ID)
			{
				return i;
			}
		}
		Debug.LogError(string.Format("Couldn't find hill with ID {0}", ID));
		return 0;
	}
}
