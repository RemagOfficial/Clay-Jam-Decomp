using UnityEngine;

[RequireComponent(typeof(Pebble))]
public class SquashMarkController : MonoBehaviour
{
	public Vector3[] _MarkRotations;

	public GameObject _SquashMarkPrefab;

	public Texture2D _GenericMarkTetxure;

	private int _nextTargetRotation;

	private int MaxMarks
	{
		get
		{
			return _MarkRotations.Length;
		}
	}

	private Pebble ParentPebble { get; set; }

	private SquashMark[] SquashMarks { get; set; }

	private void Awake()
	{
		ParentPebble = GetComponent<Pebble>();
		LoadSquashMarks();
	}

	public void StartRun()
	{
		ResetAllSquashMarks();
	}

	public void AddMark(Quaternion rotation, ObstacleMould obstacle)
	{
		for (int num = MaxMarks - 1; num > 0; num--)
		{
			SquashMarks[num].SetFrom(SquashMarks[num - 1]);
		}
		IncramentIndex(ref _nextTargetRotation);
		SquashMarks[0].Appear(rotation, obstacle, Quaternion.Euler(_MarkRotations[_nextTargetRotation]));
	}

	public void ResetAllSquashMarks()
	{
		_nextTargetRotation = 0;
		for (int i = 0; i < MaxMarks; i++)
		{
			SquashMarks[i].TurnOff();
		}
	}

	public SquashMark LoseTopLayer()
	{
		int num = -1;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < MaxMarks; i++)
		{
			if (SquashMarks[i].gameObject.active)
			{
				if (!flag)
				{
					flag = true;
					num = i;
				}
				else if (!flag2)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2)
		{
			SquashMarks[num].TurnOff();
			return SquashMarks[num];
		}
		return null;
	}

	private void LoadSquashMarks()
	{
		SquashMarks = new SquashMark[MaxMarks];
		for (int i = 0; i < MaxMarks; i++)
		{
			LoadSquashMark(i);
		}
	}

	private void LoadSquashMark(int index)
	{
		GameObject gameObject = Object.Instantiate(_SquashMarkPrefab) as GameObject;
		gameObject.transform.parent = base.transform;
		SquashMark component = gameObject.GetComponent<SquashMark>();
		SquashMarks[index] = component;
		component.SetTexture(_GenericMarkTetxure);
	}

	private void IncramentIndex(ref int index)
	{
		if (index == MaxMarks - 1)
		{
			index = 0;
		}
		else
		{
			index++;
		}
	}

	private void DecramentIndex(ref int index)
	{
		if (index == 0)
		{
			index = MaxMarks - 1;
		}
		else
		{
			index--;
		}
	}
}
