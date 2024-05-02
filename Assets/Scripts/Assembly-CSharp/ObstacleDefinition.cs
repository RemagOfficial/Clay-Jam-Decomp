using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Obstacle Definition")]
[ExecuteInEditMode]
public class ObstacleDefinition : MonoBehaviour
{
	public float _Size;

	public float _ClayValue = -1f;

	public bool _ClayColoured = true;

	public bool _Squashable = true;

	public bool _CanBePurchased;

	public bool _Purchased;

	public ObstacleType _Type;

	public LockState[] _InitialState = new LockState[5];

	public LockState[] _CurrentState = new LockState[5];

	public float _VisualScale = 1f;

	public float _CollisionRadius;

	public string _LifeTimeFabricEvent;

	public string _EmergeFabricEvent;

	public string _BounceFabricEvent;

	public List<HSVColour> _Colours;

	public List<float> _ColourCosts;

	public List<float> _ColourClayValues;

	public List<Vector3> _CollisionPoints;

	public List<int> _NonCollideableAnimTiles;

	public string _IconTextureName = string.Empty;

	public float _BounceScale = 1f;

	public bool _BounceLosesClay;

	public string _SquashAudio;

	public bool _IsPolyCutOut;

	public bool _PauseAudioOnBounce = true;

	public bool _IsVertColoured;

	public string ExtraMaterialName = string.Empty;

	public int _QuestIconIndex = -1;

	public int _HillOrder;

	[SerializeField]
	private string _StatName = string.Empty;

	public bool Splattable
	{
		get
		{
			return _Type != ObstacleType.PowerUp;
		}
	}

	public string StatName
	{
		get
		{
			if (string.IsNullOrEmpty(_StatName))
			{
				return base.gameObject.name;
			}
			return _StatName;
		}
	}

	public bool UsesHSVShader
	{
		get
		{
			return _ClayColoured || _Type == ObstacleType.Native || _Type == ObstacleType.PowerUp;
		}
	}

	public bool IsPurchased
	{
		get
		{
			return _Purchased;
		}
	}

	private float SingleClayValue
	{
		get
		{
			if (_ClayValue < 0f)
			{
				return _Size;
			}
			return _ClayValue;
		}
	}

	public bool IsAMonster
	{
		get
		{
			return _Type == ObstacleType.Creature || _Type == ObstacleType.Rascal;
		}
	}

	public int HeartValue
	{
		get
		{
			int result = 1;
			int num = _HillOrder - 1;
			if (num < GameModeDatabase.Instance._MonsterLove._HeartsPerHillOrder.Length)
			{
				result = GameModeDatabase.Instance._MonsterLove._HeartsPerHillOrder[num];
			}
			return result;
		}
	}

	public float InvulnerableTimePerHeart
	{
		get
		{
			float result = 1f;
			int num = _HillOrder - 1;
			if (num < GameModeDatabase.Instance._MonsterLove._HeartsPerHillOrder.Length)
			{
				result = GameModeDatabase.Instance._MonsterLove._InvulnerableTimePerHillOrder[num];
			}
			return result;
		}
	}

	public HSVColour GetColour(int colourIndex)
	{
		if (_Type == ObstacleType.Rascal)
		{
			if (colourIndex < 0 || colourIndex >= CurrentHill.Instance.Definition._RascalColours.Count)
			{
				Debug.LogError(string.Format("No rascal colour index {0} found for hill {1}", colourIndex, CurrentHill.Instance.ID));
				return HSVColour.NoShift;
			}
			return CurrentHill.Instance.Definition._RascalColours[colourIndex];
		}
		if (colourIndex < _Colours.Count)
		{
			return _Colours[colourIndex];
		}
		return HSVColour.NoShift;
	}

	public float CostForColour(int colourIndex)
	{
		if (colourIndex < 0 || colourIndex > _ColourCosts.Count)
		{
			Debug.LogError(string.Format("No Cost for colour {0} set on {1}", colourIndex, base.name));
			return 0f;
		}
		if (BuildDetails.Instance._HasIAP)
		{
			return _ColourCosts[colourIndex];
		}
		float num = _ColourCosts[colourIndex];
		num *= 0.5f;
		return Mathf.Floor(num);
	}

	public float ClayValueForColour(int colourIndex)
	{
		if (_ColourClayValues == null || _ColourClayValues.Count == 0)
		{
			return SingleClayValue;
		}
		if (colourIndex < 0 || colourIndex > _ColourClayValues.Count)
		{
			Debug.LogError(string.Format("No Clay value for colour {0} set on {1}", colourIndex, base.name));
			return 0f;
		}
		return _ColourClayValues[colourIndex];
	}
}
