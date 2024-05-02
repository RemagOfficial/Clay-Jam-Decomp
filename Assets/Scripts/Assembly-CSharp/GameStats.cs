using System;
using System.Collections.Generic;

[Serializable]
public class GameStats
{
	private List<ObstacleStat> _squashedTotal;

	private List<ObstacleStat> _squashedInARow;

	private ObstacleStat _currentSquashRow;

	private List<ObstacleStat> _bouncedTotal;

	private List<ObstacleStat> _bouncedInARow;

	private ObstacleStat _currentBounceRow;

	private List<ColourStat> _coloursTotal;

	private List<ColourStat> _coloursInARow;

	private ColourStat _currentColourRow;

	public int MetersFlown { get; private set; }

	public int PowerUpsCollected { get; private set; }

	public int SquashedWithShrink { get; private set; }

	public int SquashedWithFlame { get; private set; }

	public void Initialise()
	{
		int capacity = ObstacleDatabase.Instance.ObstacleDefinitions.Length;
		_squashedTotal = new List<ObstacleStat>(capacity);
		_squashedInARow = new List<ObstacleStat>(capacity);
		_currentSquashRow = new ObstacleStat(string.Empty);
		_bouncedTotal = new List<ObstacleStat>(capacity);
		_bouncedInARow = new List<ObstacleStat>(capacity);
		_currentBounceRow = new ObstacleStat(string.Empty);
		_coloursTotal = new List<ColourStat>(3);
		_coloursInARow = new List<ColourStat>(3);
		_currentColourRow = new ColourStat(-1);
		Clear();
	}

	public void Clear()
	{
		_squashedTotal.Clear();
		_squashedInARow.Clear();
		_currentSquashRow.Reset(string.Empty);
		_bouncedTotal.Clear();
		_bouncedInARow.Clear();
		_currentBounceRow.Reset(string.Empty);
		_coloursTotal.Clear();
		_coloursInARow.Clear();
		_currentColourRow.Reset(-1);
		MetersFlown = 0;
		PowerUpsCollected = 0;
		SquashedWithShrink = 0;
		SquashedWithFlame = 0;
	}

	public void ObstacleSquashed(string name, int colourIndex, bool withFlame, bool withShrink)
	{
		AddToSquashedTotal(name);
		AddToSquashedInARow(name);
		if (colourIndex != -1)
		{
			AddToColoursTotal(colourIndex);
			AddToColoursInARow(colourIndex);
		}
		else
		{
			_currentColourRow.Reset(-1);
		}
		if (withFlame)
		{
			SquashedWithFlame++;
		}
		if (withShrink)
		{
			SquashedWithShrink++;
		}
		_currentBounceRow.Reset(string.Empty);
	}

	public void ObstacleBounced(string name)
	{
		AddToBouncedTotal(name);
		AddToBouncedInARow(name);
	}

	public void OnPoweUpCollected()
	{
		PowerUpsCollected++;
	}

	public void SetMetersFlown(int distance)
	{
		MetersFlown += distance;
	}

	public int TotalSquashed()
	{
		return ListTotal(_squashedTotal);
	}

	public int TotalBounced()
	{
		return ListTotal(_bouncedTotal);
	}

	public int TotalSquashedByName(string obstacleName)
	{
		return ObstacleStatCount(_squashedTotal, obstacleName);
	}

	public int TotalBouncedByName(string obstacleName)
	{
		return ObstacleStatCount(_bouncedTotal, obstacleName);
	}

	public int LongestRowSquashed(string obstacleName)
	{
		return ObstacleStatCount(_squashedInARow, obstacleName);
	}

	public int LongestRowBounced(string obstacleName)
	{
		return ObstacleStatCount(_bouncedInARow, obstacleName);
	}

	public int TotalColour(int colourIndex)
	{
		return ColourStatCount(_coloursTotal, colourIndex);
	}

	public int LongestColourRow(int colourIndex)
	{
		return ColourStatCount(_coloursInARow, colourIndex);
	}

	private ObstacleStat GetObstacleStat(List<ObstacleStat> list, string name)
	{
		ObstacleStat obstacleStat = list.Find((ObstacleStat o) => o._ObstacleName.Equals(name));
		if (obstacleStat == null)
		{
			obstacleStat = new ObstacleStat(name);
			list.Add(obstacleStat);
		}
		return obstacleStat;
	}

	private ColourStat GetColourStat(List<ColourStat> list, int colourIndex)
	{
		ColourStat colourStat = list.Find((ColourStat c) => c._ColourIndex == colourIndex);
		if (colourStat == null)
		{
			colourStat = new ColourStat(colourIndex);
			list.Add(colourStat);
		}
		return colourStat;
	}

	private void AddToSquashedTotal(string name)
	{
		AddToTotalsList(_squashedTotal, name);
	}

	private void AddToSquashedInARow(string name)
	{
		AddToInARowList(_squashedInARow, _currentSquashRow, name);
	}

	private void AddToColoursTotal(int colourIndex)
	{
		ColourStat colourStat = GetColourStat(_coloursTotal, colourIndex);
		colourStat.Add(1);
	}

	private void AddToColoursInARow(int colourIndex)
	{
		if (_currentColourRow._ColourIndex != colourIndex)
		{
			_currentColourRow.Reset(colourIndex);
		}
		_currentColourRow.Add(1);
		int count = _currentColourRow._Count;
		ColourStat colourStat = GetColourStat(_coloursInARow, colourIndex);
		int count2 = colourStat._Count;
		if (count2 < count)
		{
			colourStat.SetCount(count);
		}
	}

	private void AddToBouncedTotal(string name)
	{
		AddToTotalsList(_bouncedTotal, name);
	}

	private void AddToBouncedInARow(string name)
	{
		AddToInARowList(_bouncedInARow, _currentBounceRow, name);
	}

	private void AddToTotalsList(List<ObstacleStat> list, string name)
	{
		ObstacleStat obstacleStat = GetObstacleStat(list, name);
		obstacleStat.Add(1);
	}

	private void AddToInARowList(List<ObstacleStat> list, ObstacleStat currentRow, string name)
	{
		if (!(currentRow._ObstacleName == name))
		{
			currentRow.Reset(name);
		}
		currentRow.Add(1);
		int count = currentRow._Count;
		ObstacleStat obstacleStat = GetObstacleStat(list, name);
		int count2 = obstacleStat._Count;
		if (count2 < count)
		{
			obstacleStat.SetCount(count);
		}
	}

	private int ListTotal(List<ObstacleStat> list)
	{
		int num = 0;
		foreach (ObstacleStat item in list)
		{
			num += item._Count;
		}
		return num;
	}

	private int ObstacleStatCount(List<ObstacleStat> list, string obstacleName)
	{
		ObstacleStat obstacleStat = list.Find((ObstacleStat o) => o._ObstacleName == obstacleName);
		if (obstacleStat != null)
		{
			return obstacleStat._Count;
		}
		return 0;
	}

	private int ColourStatCount(List<ColourStat> list, int colourIndex)
	{
		ColourStat colourStat = list.Find((ColourStat c) => c._ColourIndex == colourIndex);
		if (colourStat != null)
		{
			return colourStat._Count;
		}
		return 0;
	}

	public int CurrentRow(int colourIndex)
	{
		if (_currentColourRow._ColourIndex == colourIndex)
		{
			return _currentColourRow._Count;
		}
		return 0;
	}

	public int CurrentSquashRow(string obstacleName)
	{
		if (_currentSquashRow._ObstacleName == obstacleName)
		{
			return _currentSquashRow._Count;
		}
		return 0;
	}

	public int CurrentBounceRow(string obstacleName)
	{
		if (_currentBounceRow._ObstacleName == obstacleName)
		{
			return _currentBounceRow._Count;
		}
		return 0;
	}
}
