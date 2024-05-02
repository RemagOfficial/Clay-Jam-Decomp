using System.Runtime.CompilerServices;
using UnityEngine;

public class SurfaceHeightMap
{
	public delegate void SurfaceHeightsChangedHandler(Rect worldArea, HeightMapArea heightMapArea);

	private const float InitialHeight = 1f;

	private const float HeightsPerMeter = 51.2f;

	private const float MaxWidthMetres = 21f;

	private const float MaxLengthMetres = 16f;

	private const int Width = 1075;

	private const int Length = 819;

	private Vector3 _pebbleOffset = new Vector3(10.5f, 0f, 3f);

	private float[,] _heights;

	private int _firstRow;

	private int _firstColumn;

	private HeightMapArea _dirtyArea = HeightMapArea.Empty;

	private Vector3 _worldOffest;

	private Vector3 _lastPebblePos;

	private Color[] _fingerPrintColors;

	private int _fingerPrintWidth;

	private Texture2D _lastFingerPrintTexture;

	[method: MethodImpl(32)]
	public static event SurfaceHeightsChangedHandler SurfaceHeightsChangedEvent;

	public SurfaceHeightMap()
	{
		_heights = new float[1075, 819];
		ResetAroundPebble(Vector3.zero);
	}

	public void SetPebblePosition(Vector3 pos)
	{
		Vector3 vector = new Vector3(pos.x, 0f, pos.z);
		if ((_lastPebblePos - vector).sqrMagnitude > 100f)
		{
			ResetAroundPebble(vector);
		}
		if ((_lastPebblePos - vector).sqrMagnitude > 1f)
		{
			MoveWithPebble(vector);
		}
	}

	private void ResetAroundPebble(Vector3 pos)
	{
		_worldOffest = _pebbleOffset * -1f;
		_lastPebblePos = pos;
		for (int i = 0; i < 1075; i++)
		{
			for (int j = 0; j < 819; j++)
			{
				_heights[i, j] = 1f;
			}
		}
	}

	private void MoveWithPebble(Vector3 pos)
	{
		int x;
		int y;
		WorldPosToMapIndex(pos - _pebbleOffset, out x, out y);
		int num = _firstColumn + x;
		if (num < 0)
		{
			num = 1075 + num;
		}
		else if (num >= 1075)
		{
			num -= 1075;
		}
		int num2 = _firstRow + y;
		if (_firstRow < 0)
		{
			num2 = 819 + _firstRow;
		}
		else if (_firstRow >= 819)
		{
			num2 -= 819;
		}
		if (x > 0)
		{
			for (int i = 0; i < x; i++)
			{
				ClearColumn(i);
			}
		}
		else if (x < 0)
		{
			for (int j = x; j < 0; j++)
			{
				ClearColumn(1075 - j);
			}
		}
		if (y > 0)
		{
			for (int k = 0; k < y; k++)
			{
				ClearRow(k);
			}
		}
		else if (y < 0)
		{
			for (int l = y; l < 0; l++)
			{
				ClearRow(819 - l);
			}
		}
		_firstColumn = num;
		_firstRow = num2;
		_worldOffest = _lastPebblePos - _pebbleOffset;
		_lastPebblePos = pos;
	}

	private void ClearRow(int y)
	{
		int num = (y + _firstRow) % 819;
		for (int i = 0; i < 1075; i++)
		{
			_heights[i, num] = 1f;
		}
	}

	private void ClearColumn(int x)
	{
		int num = (x + _firstColumn) % 1075;
		for (int i = 0; i < 819; i++)
		{
			_heights[num, i] = 1f;
		}
	}

	public float GetHeight(int x, int y)
	{
		int num = (x + _firstColumn) % 1075;
		int num2 = (y + _firstRow) % 819;
		return _heights[num, num2];
	}

	private void SetHeight(int x, int y, float value)
	{
		int num = (x + _firstColumn) % 1075;
		int num2 = (y + _firstRow) % 819;
		_heights[num, num2] = value;
		_dirtyArea.Extend(x, y);
	}

	private void NotifyChange()
	{
		Rect rect = default(Rect);
		rect.x = _worldOffest.x + (float)_dirtyArea.X / 51.2f;
		rect.y = _worldOffest.z + (float)_dirtyArea.Y / 51.2f;
		rect.width = (float)_dirtyArea.Width / 51.2f;
		rect.height = (float)_dirtyArea.Length / 51.2f;
		Rect worldArea = rect;
		SurfaceHeightMap.SurfaceHeightsChangedEvent(worldArea, _dirtyArea);
		_dirtyArea = HeightMapArea.Empty;
	}

	private void AddGougeSection(GougeSection gougeSection)
	{
		Vector3 vector = gougeSection.End - gougeSection.Start;
		for (int i = 0; i < 25; i++)
		{
			Vector3 vector2 = gougeSection.Start + vector * 0.04f * i;
			int x;
			int y;
			WorldPosToMapIndex(vector2, out x, out y);
			if (!AddFingerPrint(x, y, gougeSection.BumpTexture))
			{
				Debug.LogError(string.Concat("Gouge point (", vector2, ") is outside current heightmap area ", _worldOffest));
				return;
			}
		}
		NotifyChange();
	}

	private bool AddFingerPrint(int centerX, int centerY, Texture2D bumpTexture)
	{
		if (_lastFingerPrintTexture != bumpTexture)
		{
			_fingerPrintColors = new Color[bumpTexture.width * bumpTexture.height];
			_fingerPrintColors = bumpTexture.GetPixels();
			_fingerPrintWidth = bumpTexture.width;
			_lastFingerPrintTexture = bumpTexture;
		}
		int num = centerX - _fingerPrintWidth / 2;
		int num2 = centerY - _fingerPrintWidth / 2;
		if (num < 0 || num2 < 0 || num + _fingerPrintWidth > 1075 || num2 + _fingerPrintWidth > 819)
		{
			return false;
		}
		int num3 = num;
		int num4 = 0;
		while (num4 < _fingerPrintWidth)
		{
			int num5 = num2;
			int num6 = 0;
			while (num6 < _fingerPrintWidth)
			{
				Color color = _fingerPrintColors[num4 + num6 * _fingerPrintWidth];
				float height = GetHeight(num3, num5);
				float r = color.r;
				if (r < height)
				{
					SetHeight(num3, num5, r);
				}
				num6++;
				num5++;
			}
			num4++;
			num3++;
		}
		return true;
	}

	public void WorldPosToMapIndex(Vector3 worldPos, out int x, out int y)
	{
		float num = worldPos.x - _worldOffest.x;
		x = (int)Mathf.Floor(num * 51.2f + 0.5f);
		float num2 = worldPos.z - _worldOffest.z;
		y = (int)Mathf.Floor(num2 * 51.2f + 0.5f);
	}
}
