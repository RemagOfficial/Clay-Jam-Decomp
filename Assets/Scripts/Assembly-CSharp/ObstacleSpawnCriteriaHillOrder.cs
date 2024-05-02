using System;

[Serializable]
public class ObstacleSpawnCriteriaHillOrder : ObstacleSpawnCriteriaBase
{
	public bool _Monster;

	public int _HillOrder;

	public bool _CycleColours;

	public bool _RelativeToPebble;

	public float _MinWidthRelativeToPebble;

	public float _MaxWidthRelativeToPebble;

	public override bool Matches(ObstacleCast cast, int currentHillUpgradeLevel)
	{
		if (cast.Defintion._HillOrder != _HillOrder)
		{
			return false;
		}
		return _Monster == cast.Defintion.IsAMonster;
	}
}
