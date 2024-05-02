using System;

[Serializable]
public class HillCollapseParams
{
	public float _DistBehindPebbleTostart;

	public float _MaxDistBehindPebble;

	public float _WarningDistanceBehindPebble;

	public float _WarningHoldTime;

	public float _StartTime;

	public float _SpeedAtStart;

	public float _SpeedAtEnd;

	public float _TimeToDeath;

	public float _TimeToConsume;

	public float _MinPebbleSpeedRatio;

	public float _MaxPebbleSpeedRatio;

	public float _ProgressPassedMaxPebbleSpeedForMaxRatio;
}
