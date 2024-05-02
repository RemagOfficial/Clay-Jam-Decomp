using System;

[Serializable]
public class ColourStat
{
	public int _ColourIndex;

	public int _Count;

	public ColourStat(int index)
	{
		Reset(index);
	}

	public void Add(int count)
	{
		_Count += count;
	}

	public void Reset(int index)
	{
		_ColourIndex = index;
		_Count = 0;
	}

	public void SetCount(int count)
	{
		_Count = count;
	}
}
