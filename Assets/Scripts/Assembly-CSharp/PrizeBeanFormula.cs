using System;
using UnityEngine;

[Serializable]
public class PrizeBeanFormula
{
	public int[] _FirstFewPrizeBeans;

	public float _NextPrizeMultiplier;

	public int _MaxBeans;

	public int BeansForPrize(int prizeIndex)
	{
		int num = _FirstFewPrizeBeans.Length;
		if (prizeIndex < num)
		{
			return _FirstFewPrizeBeans[prizeIndex];
		}
		float num2 = 0f;
		if (num > 0)
		{
			num2 = _FirstFewPrizeBeans[num - 1];
		}
		int num3 = prizeIndex - num + 1;
		float f = num2 * Mathf.Pow(_NextPrizeMultiplier, num3);
		int num4 = Mathf.FloorToInt(f);
		int num5;
		if (num4 < 100)
		{
			num5 = 5;
		}
		else if (num4 < 1000)
		{
			num5 = 10;
		}
		else
		{
			num5 = 100;
			int num6 = 10000;
			while (num4 > num6)
			{
				num5 *= 10;
				num6 *= 10;
			}
		}
		int num7 = num4 % num5;
		int num8 = num4;
		if (num7 > 0)
		{
			num8 += num5 - num7;
		}
		return Mathf.Min(num8, _MaxBeans);
	}
}
