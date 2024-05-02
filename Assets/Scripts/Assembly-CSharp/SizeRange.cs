public class SizeRange
{
	public float Min { get; private set; }

	public float Max { get; private set; }

	public SizeRange()
	{
		Clear();
	}

	public void Clear()
	{
		Min = float.MaxValue;
		Max = float.MinValue;
	}

	public void Add(float size)
	{
		if (Min > size)
		{
			Min = size;
		}
		if (Max < size)
		{
			Max = size;
		}
	}

	public bool Contains(float size)
	{
		return Min <= size && Max >= size;
	}
}
