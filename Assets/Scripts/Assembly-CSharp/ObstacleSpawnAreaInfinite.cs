using UnityEngine;

public class ObstacleSpawnAreaInfinite : ObstacleSpawnArea
{
	public ObstacleSpawnCriteriaHillOrder _Criteria;

	public float _Start;

	public float _End = -1f;

	public float _MinSpread;

	public float _MaxSpread;

	private int _nextColourIfCycling;

	private Vector3 _nextTriggerPosition;

	public override void ResetForRun()
	{
		_nextColourIfCycling = 0;
		SetNextTriggerPositionFrom(_Start);
		UpdateSpawning();
	}

	protected override void UpdateSpawning()
	{
		if (WithinPebbleRange())
		{
			Trigger();
		}
	}

	private void Trigger()
	{
		ObstacleCast randomCastByCriteria = HillObstacles.Instance.GetRandomCastByCriteria(_Criteria);
		if (randomCastByCriteria != null)
		{
			int prefferedColour = ((!_Criteria._CycleColours) ? (-1) : _nextColourIfCycling);
			ObstacleSpawner.Spawn(_nextTriggerPosition, randomCastByCriteria, false, prefferedColour);
			_nextColourIfCycling++;
			if (_nextColourIfCycling >= 3)
			{
				_nextColourIfCycling = 0;
			}
		}
		SetNextTriggerPosition();
	}

	private bool WithinPebbleRange()
	{
		if (_End != -1f && _nextTriggerPosition.z > _End)
		{
			return false;
		}
		return _nextTriggerPosition.z < base.CurrentHillDistanceToSpawn;
	}

	public override void TriggerRange(float start, float end)
	{
		SetNextTriggerPositionFrom(start);
		while (_nextTriggerPosition.z < end)
		{
			Trigger();
		}
	}

	private void SetNextTriggerPosition()
	{
		SetNextTriggerPositionFrom(_nextTriggerPosition.z);
	}

	private void SetNextTriggerPositionFrom(float lastZPos)
	{
		float z = lastZPos + Random.Range(_MinSpread, _MaxSpread);
		float num = ((!_Criteria._Monster) ? 0.25f : 1.5f);
		float num2 = CurrentHill.Instance.Definition._HillHalfWidth - num;
		float num3;
		if (_Criteria._RelativeToPebble)
		{
			num3 = Random.Range(_Criteria._MinWidthRelativeToPebble, _Criteria._MaxWidthRelativeToPebble);
			num3 += Pebble.Instance.transform.position.x;
			num3 = Mathf.Clamp(num3, 0f - num2, num2);
		}
		else
		{
			num3 = Random.Range(0f - num2, num2);
		}
		_nextTriggerPosition = new Vector3(num3, 0f, z);
	}
}
