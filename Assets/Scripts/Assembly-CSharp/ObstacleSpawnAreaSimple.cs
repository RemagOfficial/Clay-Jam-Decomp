using UnityEngine;

public class ObstacleSpawnAreaSimple : ObstacleSpawnArea
{
	public CastId _Cast;

	private bool _spawned;

	public override void ResetForRun()
	{
		_spawned = false;
		UpdateSpawning();
	}

	protected override void UpdateSpawning()
	{
		if (!_spawned && WithinPebbleRange())
		{
			Trigger();
		}
	}

	private void Trigger()
	{
		ObstacleCast castFromID = HillObstacles.Instance.GetCastFromID(_Cast);
		if (castFromID != null)
		{
			ObstacleSpawner.Spawn(base.transform.position, castFromID, true);
		}
		_spawned = true;
	}

	private bool WithinPebbleRange()
	{
		return base.transform.position.z < base.CurrentHillDistanceToSpawn;
	}

	public override void TriggerRange(float start, float end)
	{
		if (base.transform.position.z >= start && base.transform.position.z < end)
		{
			Trigger();
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position, 1f);
	}
}
