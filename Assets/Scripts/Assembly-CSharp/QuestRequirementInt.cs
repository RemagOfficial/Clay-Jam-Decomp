using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestRequirementInt
{
	public List<int> _FirstValues;

	public int _MaxValue;

	public int _Incrament;

	public int ValueRequired(int questIteration)
	{
		int num = 0;
		if (_FirstValues.Count > questIteration)
		{
			return _FirstValues[questIteration];
		}
		int num2 = 0;
		if (_FirstValues.Count > 0)
		{
			num2 = _FirstValues[_FirstValues.Count - 1];
		}
		int num3 = questIteration - (_FirstValues.Count - 1);
		num = num2 + num3 * _Incrament;
		return Mathf.Min(num, _MaxValue);
	}
}
