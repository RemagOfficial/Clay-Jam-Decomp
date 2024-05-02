public class ObstacleSpawnCriteriaBase
{
	public virtual bool Matches(ObstacleCast cast, int currentHillUpgradeLevel)
	{
		return true;
	}
}
