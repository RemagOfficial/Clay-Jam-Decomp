public struct HeightMapArea
{
	private int _x;

	private int _y;

	private int _width;

	private int _length;

	public static HeightMapArea Empty = new HeightMapArea(-1, -1, -1, -1);

	public int X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	public int Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	public int Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
		}
	}

	public int Length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public int MaxX
	{
		get
		{
			return X + Width;
		}
	}

	public int MaxY
	{
		get
		{
			return Y + Length;
		}
	}

	public HeightMapArea(int x, int y, int w, int l)
	{
		_x = x;
		_y = y;
		_width = w;
		_length = l;
	}

	public void Extend(int x, int y)
	{
		if (_x == -1)
		{
			X = x;
			Y = y;
			Width = 1;
			Length = 1;
			return;
		}
		if (x < _x)
		{
			_width += _x - x;
			_x = x;
		}
		else if (x > _x + _width)
		{
			_width = x - _x;
		}
		if (y < _y)
		{
			_length += _y - y;
			_y = y;
		}
		else if (y > _y + _length)
		{
			_length = y - _y;
		}
	}
}
