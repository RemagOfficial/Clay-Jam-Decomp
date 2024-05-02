using System;
using System.Collections.Generic;

[Serializable]
public class QuestRequirementString
{
	public List<string> _Values;

	public string ValueRequired(int questIteration)
	{
		int num = questIteration;
		if (num >= _Values.Count)
		{
			num = _Values.Count - 1;
		}
		return _Values[num];
	}
}
