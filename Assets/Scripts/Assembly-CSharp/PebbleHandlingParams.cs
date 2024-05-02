using System;
using UnityEngine;

[Serializable]
public class PebbleHandlingParams
{
	public float _MaxRadiusScale = 8f;

	public float _OriginalRadiusScale = 1f;

	public float _ModelRadiusMeters = 0.25f;

	public float _RadiusRatio_TouchingCurb = 0.6f;

	public float _TopMaxSpeed = 10f;

	public float _BaseMaxSpeed = 2f;

	public float _MinHitMonsterSpeed = 5f;

	public float _Acceleration = 100f;

	public float _BounceHeightSmallestSize = 3f;

	public float _BounceHeightBiggestSize = 5f;

	public float _BounceDistanceSmallestSize = 3f;

	public float _BounceDistanceBiggestSize = 8f;

	public float _BounceSpinRPS = 4f;

	public float _SpinFactorOriginalRadius = 1.5f;

	public float _SpinFactorMaxRadius = 5f;

	public float _ClayNeededForMaxRadius = 1500f;

	public float _StartSize = 1f;

	public float _PercentLostPerBounce = 5f;

	public float _FlyDistanceMax = 1000f;

	public float _FlyDistanceMin = 30f;

	public float _FlyTime = 2.5f;

	public float _FlyHeight = 12.5f;

	public float _ClayNeededForMaxFlight = 1500f;

	public float _FlyFactor_Clay = 0.8f;

	public float _FlyFactor_Speed = 0.2f;

	public float _FlyFactor_Gouge = 0.2f;

	public float _TimeToStopWhenTravelingDirectlyUpHill = 0.4f;

	public float _SpeedToDropOutOfGouge = 2f;

	public float _DistanceAllowedToTravelUpHill = 0.4f;

	public float _GougeDownHillRatioForMinBoost;

	public float _GougeDownHillRatioForMaxBoost;

	public float _GougeMinBoost = 1f;

	public float _GougeMaxBoost = 2f;

	public float _GougeMaxBoostInBottomZone = 6f;

	public float _GougeSpeedBoostDecayTime = 5f;

	public float _FlickMinBoost = 1f;

	public float _FlickMaxBoost = 6f;

	public float _FlickMaxSpeedInchesPerSec = 30f;

	public float _MinFlickPowerNormalisedForSpecialFX = 0.9f;

	public float _MinFlickPowerNormalisedForMediumFX = 0.6f;

	public float _MomentumDownhillCosineToGain = 0.5f;

	public float _MomentumDownhillCosineForMaxGain = 0.707f;

	public float _MomentumDownhillCosineToLose = 0.17f;

	public float _MomentumDownhillCosineForMaxLoss = -0.17f;

	public float _MomentumBuilupTime = 40f;

	public float _MomentumMaxBoost = 1.5f;

	public float _MomentumLossTime = 1f;

	public float _MomentumBounceLoss = 0.5f;

	public float _TimeToStopWhenOutOfLives = 1f;

	public float _MLove_ProgressForMinSpeed = 10f;

	public float _MLove_ProgressForMaxSpeed = 200f;

	public float _MLove_MinSpeed = 2f;

	public float _MLove_MaxSpeed = 10f;

	public float MaxSpeed
	{
		get
		{
			return _TopMaxSpeed * _MomentumMaxBoost;
		}
	}

	public float MinSpeed
	{
		get
		{
			return _BaseMaxSpeed;
		}
	}

	public float MaxSpeedPerRadius { get; set; }

	public float SpinfactorPerRadius { get; set; }

	public float BounceHeightPerRadius { get; set; }

	public float BounceDistancePerRadius { get; set; }

	public float OriginalVolume { get; private set; }

	public float MaxVolume { get; private set; }

	public float MaxAddedVolume { get; private set; }

	public float VolumeAddedPerClay { get; private set; }

	public float GetRadiusScaleForSize(float size)
	{
		float a = size - _StartSize;
		float num = Mathf.Min(a, _ClayNeededForMaxRadius);
		float num2 = OriginalVolume + num * VolumeAddedPerClay;
		return Mathf.Pow(num2 / 0.75f / (float)Math.PI, 1f / 3f);
	}

	public float PebbleRadiusMetersAtSize(float size)
	{
		return GetRadiusScaleForSize(size) * _ModelRadiusMeters;
	}

	public float MonsterLoveNormalisedSpeed(float progress)
	{
		float value = (progress - _MLove_ProgressForMinSpeed) / (_MLove_ProgressForMaxSpeed - _MLove_ProgressForMinSpeed);
		return Mathf.Clamp01(value);
	}

	public float MonsterLoveSpeedForProgress(float progress)
	{
		return _MLove_MinSpeed + MonsterLoveNormalisedSpeed(progress) * (_MLove_MaxSpeed - _MLove_MinSpeed);
	}

	public void CalculateConstants()
	{
		CalculateSpeedConstants();
		CalculateBounceConstants();
		CalculateVolumeConstants();
	}

	private void CalculateSpeedConstants()
	{
		MaxSpeedPerRadius = (_TopMaxSpeed - _BaseMaxSpeed) / (_MaxRadiusScale - _OriginalRadiusScale);
		SpinfactorPerRadius = (_SpinFactorMaxRadius - _SpinFactorOriginalRadius) / (_MaxRadiusScale - _OriginalRadiusScale);
	}

	private void CalculateBounceConstants()
	{
		BounceHeightPerRadius = (_BounceHeightBiggestSize - _BounceHeightSmallestSize) / (_MaxRadiusScale - _OriginalRadiusScale);
		BounceDistancePerRadius = (_BounceDistanceBiggestSize - _BounceDistanceSmallestSize) / (_MaxRadiusScale - _OriginalRadiusScale);
	}

	private void CalculateVolumeConstants()
	{
		OriginalVolume = (float)Math.PI * 3f / 4f * _OriginalRadiusScale * _OriginalRadiusScale * _OriginalRadiusScale;
		MaxVolume = (float)Math.PI * 3f / 4f * _MaxRadiusScale * _MaxRadiusScale * _MaxRadiusScale;
		MaxAddedVolume = MaxVolume - OriginalVolume;
		VolumeAddedPerClay = MaxAddedVolume / _ClayNeededForMaxRadius;
	}

	public float GetRatioOfMaxSpeed(float speed)
	{
		return (speed - MinSpeed) / (MaxSpeed - MinSpeed);
	}
}
