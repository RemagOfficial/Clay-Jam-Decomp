using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CurrentQuest : MonoBehaviour
{
	private Quest _quest;

	public static CurrentQuest Instance { get; private set; }

	public bool HasQuest
	{
		get
		{
			return _quest != null;
		}
	}

	public string Description
	{
		get
		{
			int beastDefeatedCount = CurrentHill.Instance.ProgressData._BeastDefeatedCount;
			return (!(_quest == null)) ? _quest.Description(beastDefeatedCount) : "NO QUEST!";
		}
	}

	public int IconIndex
	{
		get
		{
			int beastDefeatedCount = CurrentHill.Instance.ProgressData._BeastDefeatedCount;
			return (!(_quest == null)) ? _quest._IconIndex.ValueRequired(beastDefeatedCount) : 0;
		}
	}

	public bool QuestComplete
	{
		get
		{
			if (_quest == null)
			{
				return false;
			}
			return _quest.CompletedByCurrentRun(CurrentRunStats.Instance.Stats, CurrentHill.Instance.ProgressData._BeastDefeatedCount);
		}
	}

	public bool CannotComplete
	{
		get
		{
			if (_quest == null)
			{
				return true;
			}
			return _quest.CannotComplete(CurrentRunStats.Instance.Stats, CurrentHill.Instance.ProgressData._BeastDefeatedCount);
		}
	}

	public bool AllQuestsComplete
	{
		get
		{
			return QuestComplete && CurrentHill.Instance.ProgressData.OnLastQuest;
		}
	}

	public bool HasQuestWithProgressCounter
	{
		get
		{
			return (bool)_quest && _quest.HasProgressCounter;
		}
	}

	public int Progress
	{
		get
		{
			if (_quest == null)
			{
				return -1;
			}
			return _quest.Progress(CurrentRunStats.Instance.Stats, CurrentHill.Instance.ProgressData._BeastDefeatedCount);
		}
	}

	public int Target
	{
		get
		{
			if (_quest == null)
			{
				return -1;
			}
			return _quest.TargetCount(CurrentHill.Instance.ProgressData._BeastDefeatedCount);
		}
	}

	public string CounterText
	{
		get
		{
			if (_quest == null)
			{
				return string.Empty;
			}
			return _quest.CounterText(CurrentRunStats.Instance.Stats, CurrentHill.Instance.ProgressData._BeastDefeatedCount);
		}
	}

	[method: MethodImpl(32)]
	public static event Action<string> QuestCompleteEvent;

	[method: MethodImpl(32)]
	public static event Action<string> QuestFailedEvent;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("more than one instance of CurrentQuest", base.gameObject);
		}
		Instance = this;
		InGameController.StateChanged += OnStateChanged;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnStateChanged;
	}

	private void OnStateChanged(InGameController.State newState)
	{
		if (newState == InGameController.State.ResettingForRun)
		{
			GetQuest();
		}
	}

	private void GetQuest()
	{
		if (!SaveData.Instance.Progress._QuestsStarted.Set && !SaveData.Instance.Progress._optionTutorialsOn.Set)
		{
			SaveData.Instance.Progress._QuestsStarted.Set = true;
		}
		if (SaveData.Instance.Progress._QuestsStarted.Set)
		{
			_quest = QuestDatabase.Instance.GetQuest(CurrentHill.Instance.ID, CurrentHill.Instance.ProgressData._CurrentQuestIndex);
		}
		else
		{
			_quest = null;
		}
	}

	public bool ShouldSplatFingerAffectStats(ObstacleMould obstacle)
	{
		if (_quest == null)
		{
			return false;
		}
		return _quest.SplatFingerAffectsStats(obstacle);
	}

	public bool MoveOn(bool skipped)
	{
		if (!skipped)
		{
			string obj = CurrentHill.Instance.ProgressData.FullQuestIDString();
			if (CurrentQuest.QuestCompleteEvent != null)
			{
				CurrentQuest.QuestCompleteEvent(obj);
			}
		}
		bool result = CurrentHill.Instance.ProgressData.OnQuestCompleted();
		GetQuest();
		return result;
	}

	public void MarkFailed()
	{
		string obj = string.Format("QuestFail_H{0}_I{1}_Q{2}", CurrentHill.Instance.ID, CurrentHill.Instance.ProgressData._BeastDefeatedCount, CurrentHill.Instance.ProgressData._CurrentQuestIndex);
		if (CurrentQuest.QuestFailedEvent != null)
		{
			CurrentQuest.QuestFailedEvent(obj);
		}
	}
}
