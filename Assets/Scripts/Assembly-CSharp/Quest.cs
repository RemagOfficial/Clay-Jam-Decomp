using UnityEngine;

public abstract class Quest : MonoBehaviour
{
	public QuestRequirementInt _IconIndex;

	public virtual bool HasProgressCounter
	{
		get
		{
			return false;
		}
	}

	public abstract bool CompletedByCurrentRun(GameStats currentRunStats, int questIteration);

	public abstract string Description(int questIteration);

	public virtual int Progress(GameStats currentRunStats, int questIteration)
	{
		return -1;
	}

	public virtual int TargetCount(int questIteration)
	{
		return -1;
	}

	public virtual string CounterText(GameStats currentRunStats, int questIteration)
	{
		return string.Empty;
	}

	public virtual bool CannotComplete(GameStats gameStats, int p)
	{
		return false;
	}

	public virtual bool SplatFingerAffectsStats(ObstacleMould obstacle)
	{
		return true;
	}
}
