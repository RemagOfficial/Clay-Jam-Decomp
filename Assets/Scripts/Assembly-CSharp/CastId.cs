using System;

[Serializable]
public class CastId
{
	public string _MouldName;

	public int _ColourIndex;

	public string MouldName
	{
		get
		{
			return _MouldName;
		}
		set
		{
			_MouldName = value;
		}
	}

	public int ColourIndex
	{
		get
		{
			return _ColourIndex;
		}
		set
		{
			_ColourIndex = value;
		}
	}

	public CastId()
	{
	}

	public CastId(string mouldName, int colourIndex)
	{
		MouldName = mouldName;
		ColourIndex = colourIndex;
	}

	public void copyFrom(CastId id)
	{
		MouldName = id.MouldName;
		ColourIndex = id.ColourIndex;
	}

	public bool Compare(CastId id)
	{
		if (id.MouldName == MouldName && id.ColourIndex == ColourIndex)
		{
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		CastId castId = obj as CastId;
		if (castId == null)
		{
			return false;
		}
		return Compare(castId);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
