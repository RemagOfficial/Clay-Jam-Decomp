using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Hill Setup/Spawn Area")]
public class ObstacleSpawnAreaRandom : ObstacleSpawnArea
{
	private const float GapFromEdge = 1f;

	public List<ObstacleSpawner> _Spawners;

	public float _Length;

	public float _LeftEdge;

	public float _RightEdge;

	public bool _PlaceRelativeToPebble;

	public float _StartDistance;

	public float _EndDistance;

	public int _NumberOfSections;

	public int _DistBetweenSpawns = -1;

	private float _currentTriggerPoint;

	private float _widthPerSection;

	private int DistBetweenSpawns
	{
		get
		{
			if (_DistBetweenSpawns <= 0)
			{
				if (_Length >= 1f)
				{
					return Mathf.FloorToInt(_Length);
				}
				return 1;
			}
			return _DistBetweenSpawns;
		}
	}

	private void Start()
	{
		if (_LeftEdge > _RightEdge)
		{
			float leftEdge = _LeftEdge;
			_LeftEdge = _RightEdge;
			_RightEdge = leftEdge;
		}
		float hillHalfWidth = CurrentHill.Instance.Definition._HillHalfWidth;
		if (hillHalfWidth > 0f)
		{
			ConstrainSpawnAreaToWidth(hillHalfWidth - 1f);
		}
		_widthPerSection = (_RightEdge - _LeftEdge) / (float)_NumberOfSections;
	}

	public override void Initialise()
	{
		foreach (ObstacleSpawner spawner in _Spawners)
		{
			spawner.Initialise();
		}
	}

	public override void ResetForRun()
	{
		SetTriggerPoint(_StartDistance);
		UpdateSpawning();
	}

	protected override void UpdateSpawning()
	{
		if (!WithinPebbleRange())
		{
			return;
		}
		for (int i = 0; i < _NumberOfSections; i++)
		{
			Vector3 triggerPoint = new Vector3(0f, 0f, _currentTriggerPoint);
			if (_PlaceRelativeToPebble)
			{
				triggerPoint.x = Pebble.Position.x;
			}
			Vector3 vector = NextFreeSpawnPosition(i);
			triggerPoint = KeepWithinHillBounds(triggerPoint);
			Vector3 spawnPoint = triggerPoint + vector;
			if (spawnPoint.x > CurrentHill.Instance.Definition._HillHalfWidth)
			{
				Debug.LogWarning("ObstacleSpawnAreaRandom.UpdateSpawning(): Spawned out of bounds! triggerPoint.x: " + triggerPoint.x + " spawnPoint.x: " + spawnPoint.x);
			}
			foreach (ObstacleSpawner spawner in _Spawners)
			{
				spawner.SpawnAll(spawnPoint);
			}
		}
		SetTriggerPoint(_currentTriggerPoint + (float)DistBetweenSpawns);
	}

	private void SetTriggerPoint(float val)
	{
		if (CurrentHill.Instance.ProgressIsBeyondHorizon(val))
		{
			_currentTriggerPoint = float.MaxValue;
		}
		else
		{
			_currentTriggerPoint = val;
		}
	}

	private Vector3 NextFreeSpawnPosition(int section)
	{
		float x = Random.Range(_LeftEdge + (float)section * _widthPerSection, _LeftEdge + (float)(section + 1) * _widthPerSection);
		float y = 0f;
		float z = Random.Range(0f, _Length);
		return new Vector3(x, y, z);
	}

	private bool WithinPebbleRange()
	{
		return _currentTriggerPoint <= _EndDistance && _currentTriggerPoint < base.CurrentHillDistanceToSpawn;
	}

	private Vector3 KeepWithinHillBounds(Vector3 triggerPoint)
	{
		float num = CurrentHill.Instance.Definition._HillHalfWidth - 1f;
		if (num > 0f)
		{
			if (triggerPoint.x + _LeftEdge < 0f - num)
			{
				triggerPoint.x += 0f - num - (triggerPoint.x + _LeftEdge);
			}
			else if (triggerPoint.x + _RightEdge > num)
			{
				triggerPoint.x -= triggerPoint.x + _RightEdge - num;
			}
		}
		return triggerPoint;
	}

	private void ConstrainSpawnAreaToWidth(float halfWidth)
	{
		if (_LeftEdge < 0f - halfWidth)
		{
			_RightEdge += 0f - halfWidth - _LeftEdge;
		}
		else if (_RightEdge > halfWidth)
		{
			_LeftEdge -= _RightEdge - halfWidth;
		}
		if (_RightEdge > halfWidth)
		{
			_RightEdge = halfWidth;
		}
		if (_LeftEdge < 0f - halfWidth)
		{
			_LeftEdge = 0f - halfWidth;
		}
	}

	public override void TriggerRange(float start, float end)
	{
		for (_currentTriggerPoint = _StartDistance; _currentTriggerPoint <= _EndDistance; _currentTriggerPoint += DistBetweenSpawns)
		{
			for (int i = 0; i < _NumberOfSections; i++)
			{
				Vector3 vector = new Vector3(0f, 0f, _currentTriggerPoint);
				Vector3 vector2 = NextFreeSpawnPosition(i);
				Vector3 spawnPoint = vector + vector2;
				foreach (ObstacleSpawner spawner in _Spawners)
				{
					spawner.SpawnAllForced(spawnPoint);
				}
			}
		}
	}
}
