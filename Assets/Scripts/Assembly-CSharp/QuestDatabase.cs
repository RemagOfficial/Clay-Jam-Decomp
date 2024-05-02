using UnityEngine;

public class QuestDatabase : MonoBehaviour
{
	private HillQuests[] _allHillQuests;

	public static QuestDatabase Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of QuestDatabase", base.gameObject);
		}
		Instance = this;
		_allHillQuests = GetComponentsInChildren<HillQuests>();
	}

	public Quest GetQuest(int hillID, int questIndex)
	{
		HillQuests hillQuests = GetHillQuests(hillID);
		if (hillQuests == null)
		{
			return null;
		}
		return hillQuests.GetQuest(questIndex);
	}

	public int QuestCount(int hillID)
	{
		HillQuests hillQuests = GetHillQuests(hillID);
		if (hillQuests == null)
		{
			return 0;
		}
		return hillQuests.QuestCount();
	}

	private HillQuests GetHillQuests(int hillID)
	{
		int i;
		for (i = 0; i < _allHillQuests.Length && _allHillQuests[i]._HillID != hillID; i++)
		{
		}
		if (i == _allHillQuests.Length)
		{
			Debug.LogError(string.Format("No Quests for hill {0}", hillID));
			return null;
		}
		return _allHillQuests[i];
	}

	public float GetSkipCost(HillData hillData)
	{
		HillQuests hillQuests = GetHillQuests(hillData._ID);
		int num = hillQuests.QuestCount();
		int beastDefeatedCount = hillData._BeastDefeatedCount;
		int currentQuestIndex = hillData._CurrentQuestIndex;
		int questNumber = beastDefeatedCount * num + currentQuestIndex;
		return hillQuests.SkipCost(questNumber);
	}
}
