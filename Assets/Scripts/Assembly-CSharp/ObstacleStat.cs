using System;

[Serializable]
public class ObstacleStat
{
	public string _ObstacleName;

	public int _Count;

	public ObstacleStat(string name)
	{
		Reset(name);
	}

	public void Add(int count)
	{
		_Count += count;
	}

	public void Reset(string name)
	{
		_ObstacleName = name;
		_Count = 0;
	}

	public void SetCount(int count)
	{
		_Count = count;
	}
}
