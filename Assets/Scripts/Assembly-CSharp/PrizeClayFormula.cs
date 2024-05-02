using System;
using UnityEngine;

[Serializable]
public class PrizeClayFormula
{
	public float _FirstClayAmount;

	public float _NextPrizeMultiplier;

	public int _MaxClay;

	public float ClayForPrize(int proceduralPrizeNumer)
	{
		float f = _FirstClayAmount * Mathf.Pow(_NextPrizeMultiplier, proceduralPrizeNumer);
		int num = Mathf.FloorToInt(f);
		int num2 = 10;
		int num3 = 1000;
		while (num > num3)
		{
			num2 *= 10;
			num3 *= 10;
		}
		int num4 = num % num2;
		int num5 = num;
		if (num4 > 0)
		{
			num5 += num2 - num4;
		}
		float a = num5;
		return Mathf.Min(a, _MaxClay);
	}
}
