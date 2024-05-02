using System;
using UnityEngine;

[Serializable]
public class ObstacleCast : IComparable<ObstacleCast>
{
	public CastId _Id;

	private ObstacleDefinition _defintion;

	public string Name
	{
		get
		{
			return _Id.MouldName;
		}
		set
		{
			_Id.MouldName = value;
		}
	}

	public int _ColourIndex
	{
		get
		{
			return _Id.ColourIndex;
		}
		set
		{
			_Id.ColourIndex = value;
		}
	}

	public ObstacleDefinition Defintion
	{
		get
		{
			if (!_defintion)
			{
				_defintion = ObstacleDatabase.Instance.GetDefitnion(Name);
				if (!_defintion)
				{
					Debug.LogError(string.Format("Mould {0} does not exist", _Id.MouldName));
					return null;
				}
			}
			return _defintion;
		}
	}

	public ObstacleCast()
	{
		_Id = new CastId();
	}

	public int CompareTo(ObstacleCast other)
	{
		if (Defintion._Size < other.Defintion._Size)
		{
			return -1;
		}
		if (Defintion._Size > other.Defintion._Size)
		{
			return 1;
		}
		return 0;
	}

	public override string ToString()
	{
		return string.Format("{0}:size {1}", _Id.MouldName, Defintion._Size);
	}
}
