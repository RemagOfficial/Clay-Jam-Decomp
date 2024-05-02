public class GameModeStateQuest : GameModeState
{
	public override void OnObstacleSquash(ObstacleMould obstacle)
	{
		Pebble.Instance.GetClayFromObstacle(obstacle);
	}

	public override bool CanSquashObstacle(Pebble pebble, ObstacleMould obstacle)
	{
		if (!obstacle.Definition._Squashable)
		{
			return false;
		}
		return pebble.Size >= obstacle.Size;
	}

	public override float GetCurrentMaxSpeed(Pebble pebble)
	{
		return pebble.MaxSpeedForCurrentRadius;
	}
}
