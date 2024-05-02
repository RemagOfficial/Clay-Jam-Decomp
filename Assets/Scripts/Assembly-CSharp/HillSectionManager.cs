using UnityEngine;

public class HillSectionManager : ManagedComponent
{
	private const int NumSectionsAlong = 12;

	private const int NumEdges = 2;

	private const float SectionLength = 5f;

	private const float SectionHalfLength = 2.5f;

	private const float EndSectionLength = 20f;

	private const float EndSectionDistToPivot = 2.5f;

	public Object _MainSectionPrefab;

	public Object _EndSectionPrefab;

	public Object _LeftEdgePrefab;

	public Object _LeftEndEdgePrefab;

	public Object _RightEdgePrefab;

	public Object _RightEndEdgePrefab;

	private GameObject[] _mainSections = new GameObject[12];

	private GameObject _endSection;

	private GameObject[,] _edges = new GameObject[12, 2];

	private GameObject[] _endEdges = new GameObject[2];

	private int _bottomRow;

	private int _topRow;

	private int _bottomEdgeRow;

	private int _topEdgeRow;

	private float _edgeHalfWidth;

	private bool _hillHasEdges;

	public static HillSectionManager Instance { get; private set; }

	private float HillHalfWidth
	{
		get
		{
			return CurrentHill.Instance.Definition._HillHalfWidth;
		}
	}

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of HillSectionManager", base.gameObject);
		}
		Instance = this;
		InGameController.StateChanged += OnStateChanged;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnStateChanged;
		Instance = null;
	}

	private bool CreateNextGridSection()
	{
		for (int i = 0; i < 12; i++)
		{
			if (!(_mainSections[i] != null))
			{
				GameObject gameObject = Object.Instantiate(_MainSectionPrefab) as GameObject;
				gameObject.transform.parent = base.transform;
				gameObject.name = string.Format("Section{0}-", i);
				_mainSections[i] = gameObject;
				return false;
			}
		}
		return true;
	}

	private bool CreateNextEndSection()
	{
		if (_endSection != null)
		{
			return true;
		}
		GameObject gameObject = Object.Instantiate(_EndSectionPrefab) as GameObject;
		gameObject.transform.parent = base.transform;
		gameObject.name = string.Format("EndSection");
		_endSection = gameObject;
		return false;
	}

	private bool CreateNextEdge()
	{
		if ((bool)_LeftEdgePrefab)
		{
			for (int i = 0; i < 12; i++)
			{
				if (!(_edges[i, 0] != null))
				{
					GameObject gameObject = Object.Instantiate(_LeftEdgePrefab) as GameObject;
					gameObject.transform.parent = base.transform;
					gameObject.name = string.Format("Edge Left-{0}", i);
					_edges[i, 0] = gameObject;
					return false;
				}
			}
		}
		if ((bool)_RightEdgePrefab)
		{
			for (int j = 0; j < 12; j++)
			{
				if (!(_edges[j, 1] != null))
				{
					GameObject gameObject2 = Object.Instantiate(_RightEdgePrefab) as GameObject;
					gameObject2.transform.parent = base.transform;
					gameObject2.name = string.Format("Edge-Right-{0}", j);
					_edges[j, 1] = gameObject2;
					return false;
				}
			}
		}
		return true;
	}

	private bool CreateNextEndEdge()
	{
		if ((bool)_LeftEndEdgePrefab)
		{
			GameObject gameObject = Object.Instantiate(_LeftEndEdgePrefab) as GameObject;
			gameObject.transform.parent = base.transform;
			gameObject.name = string.Format("EndEdge-Left");
			_endEdges[0] = gameObject;
		}
		if ((bool)_LeftEndEdgePrefab)
		{
			GameObject gameObject2 = Object.Instantiate(_RightEndEdgePrefab) as GameObject;
			gameObject2.transform.parent = base.transform;
			gameObject2.name = string.Format("EndEdge-Right");
			_endEdges[1] = gameObject2;
		}
		return true;
	}

	public void Unload()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (InGameController.Instance.Paused || Pebble.Instance.IsInAvalanche)
		{
			return;
		}
		int bottomRow = _bottomRow;
		float num = _mainSections[bottomRow].transform.position.z + 2.5f;
		float num2 = -1f;
		if (CurrentGameMode.HasBottom)
		{
			float num3 = _mainSections[_topRow].transform.position.z + 2.5f + 20f;
			num2 = num3 - CurrentHill.Instance.Length;
		}
		if (!(HillCollapser.Instance.PointOfCollapse > num) || !(num2 < 0f))
		{
			return;
		}
		_mainSections[_bottomRow].transform.Translate(0f, 0f, 60f, Space.World);
		_bottomRow = ((_bottomRow < 11) ? (_bottomRow + 1) : 0);
		_topRow = ((_topRow < 11) ? (_topRow + 1) : 0);
		if (_hillHasEdges)
		{
			for (int i = 0; i < 2; i++)
			{
				_edges[_bottomEdgeRow, i].transform.Translate(0f, 0f, 60f, Space.World);
			}
			_bottomEdgeRow = ((_bottomEdgeRow < 11) ? (_bottomEdgeRow + 1) : 0);
			_topEdgeRow = ((_topEdgeRow < 11) ? (_topEdgeRow + 1) : 0);
		}
		PositionEndSection();
	}

	protected override bool DoInitialise()
	{
		if (!CreateNextGridSection())
		{
			return false;
		}
		if (!CreateNextEndSection())
		{
			return false;
		}
		if (HillHalfWidth > 0f)
		{
			if (!CreateNextEdge())
			{
				return false;
			}
			if (!CreateNextEndEdge())
			{
				return false;
			}
			_hillHasEdges = true;
		}
		return true;
	}

	public override void ResetForRun()
	{
		ResetGrid();
	}

	private void ResetGrid()
	{
		float num = 2.5f;
		if (CurrentGameMode.HasBottom)
		{
			float num2 = (CurrentHill.Instance.Length - 20f) / 5f;
			int num3 = 0;
			if (num2 < 12f)
			{
				num3 = 12 - Mathf.FloorToInt(num2);
			}
			float num4 = num2 - Mathf.Floor(num2);
			float num5 = (float)num3 * -5f;
			num5 += num4 * 5f;
			num = num5 - 2.5f;
		}
		int num6 = 0;
		int num7 = -6;
		while (num7 < 6)
		{
			Vector3 position = new Vector3(0f, 0f, num + (float)num7 * 5f);
			_mainSections[num6].gameObject.transform.position = position;
			num7++;
			num6++;
		}
		if (_hillHasEdges)
		{
			num6 = 0;
			int num8 = -6;
			while (num8 < 6)
			{
				for (int i = 0; i < 2; i++)
				{
					_edgeHalfWidth = 0f;
					float x = ((i != 0) ? (HillHalfWidth + _edgeHalfWidth) : (0f - HillHalfWidth - _edgeHalfWidth));
					Vector3 position2 = new Vector3(x, 0f, num + (float)num8 * 5f);
					_edges[num6, i].gameObject.transform.position = position2;
				}
				num8++;
				num6++;
			}
			_bottomEdgeRow = 0;
			_topEdgeRow = 11;
		}
		_bottomRow = 0;
		_topRow = 11;
		PositionEndSection();
	}

	private void PositionEndSection()
	{
		Vector3 position = _mainSections[_topRow].transform.position;
		position.z += 5f;
		_endSection.transform.position = position;
		PositionEndEdges();
	}

	private void PositionEndEdges()
	{
		if (_hillHasEdges)
		{
			for (int i = 0; i < 2; i++)
			{
				float z = _edges[_topEdgeRow, i].transform.position.z;
				z += 5f;
				float x = ((i != 0) ? (HillHalfWidth + _edgeHalfWidth) : (0f - HillHalfWidth - _edgeHalfWidth));
				_endEdges[i].transform.position = new Vector3(x, 0f, z);
			}
		}
	}

	private void OnStateChanged(InGameController.State newState)
	{
		switch (newState)
		{
		case InGameController.State.Flying:
		case InGameController.State.ShowingResults:
			base.gameObject.SetActiveRecursively(false);
			break;
		case InGameController.State.ResettingForRun:
			base.gameObject.SetActiveRecursively(true);
			break;
		}
	}
}
