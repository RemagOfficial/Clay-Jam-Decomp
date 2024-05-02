using UnityEngine;

public abstract class ObstacleSpawnArea : MonoBehaviour
{
	protected float _lookAhead = 3f;

	protected float CurrentHillDistanceToSpawn
	{
		get
		{
			return CameraDirector.ScreenTop + _lookAhead;
		}
	}

	private void FixedUpdate()
	{
		if (InGameController.Instance.CurrentState == InGameController.State.RollingTop)
		{
			UpdateSpawning();
		}
	}

	public virtual void ResetForRun()
	{
	}

	public virtual void Initialise()
	{
	}

	protected virtual bool Trigger(ObstacleSpawner spawner)
	{
		return true;
	}

	protected abstract void UpdateSpawning();

	public abstract void TriggerRange(float start, float end);
}
