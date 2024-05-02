using UnityEngine;

public class HillQuests : MonoBehaviour
{
	public int _HillID;

	public float _MinSkipCost;

	public float _SkipCostIncrament;

	public float _MaxSkipCost;

	public Quest[] _Quests;

	public Quest GetQuest(int questIndex)
	{
		if (questIndex < 0 || questIndex >= _Quests.Length)
		{
			Debug.LogError(string.Format("No Quest number {0} for hill {1}", questIndex, _HillID));
			return null;
		}
		return _Quests[questIndex];
	}

	public int QuestCount()
	{
		return _Quests.Length;
	}

	public float SkipCost(int questNumber)
	{
		float a = _MinSkipCost + _SkipCostIncrament * (float)questNumber;
		a = Mathf.Min(a, _MaxSkipCost);
		if (!BuildDetails.Instance._HasIAP)
		{
			a *= 0.5f;
			a = Mathf.Floor(a);
		}
		return a;
	}
}
