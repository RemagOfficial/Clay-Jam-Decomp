using UnityEngine;

public class GameModeState : MonoBehaviour
{
	public virtual int Lives
	{
		get
		{
			return 1;
		}
		protected set
		{
		}
	}

	public bool HasLivesLeft
	{
		get
		{
			return Lives > 0;
		}
	}

	public virtual void OnResetForRun()
	{
	}

	public virtual void OnObstacleSquash(ObstacleMould obstacle)
	{
	}

	public virtual void OnObstacleBounce(ObstacleMould obstacle)
	{
	}

	public virtual bool CanSquashObstacle(Pebble pebble, ObstacleMould obstacle)
	{
		return false;
	}

	public virtual float GetCurrentMaxSpeed(Pebble pebble)
	{
		return 1f;
	}

	public virtual void GiveClayBoost(Pebble pebble)
	{
		ClayData jacketClay = PowerupDatabase.Instance.GetJacketClay(pebble.Size);
		Pebble.Instance.GetClayInstantly(jacketClay);
	}
}
