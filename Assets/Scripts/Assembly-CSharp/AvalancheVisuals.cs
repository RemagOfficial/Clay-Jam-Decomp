using System.Collections.Generic;
using UnityEngine;

public class AvalancheVisuals : MonoBehaviour
{
	public float _RollerSpeedMax;

	public float _RollerSpeedMin;

	public float _UV_Roller_SpeedRatio;

	public List<Renderer> _RenderersToScroll;

	public Animation _RollerAnimation;

	private ParticleEmitter[] _particles;

	private AnimationState _rollerAnimState;

	public AvalancheColour _MonsterLoveFinalColour;

	public AvalancheColour _StandardColour;

	public AvalancheColour _FlashColourGood;

	public AvalancheColour _FlashColourBad;

	public List<AvalancheColour> _MonsterLoveLevelColours;

	private List<Material> _materialsToColour;

	private AvalancheColour _currentColour;

	private AvalancheColour _targetColour;

	private AvalancheColour _lastColour;

	private int _lastColourLevel;

	private int _targetColourLevel;

	private float _colourChangeTime;

	public float _ColourAnimTime;

	private void Awake()
	{
		_rollerAnimState = _RollerAnimation[_RollerAnimation.clip.name];
		_particles = GetComponentsInChildren<ParticleEmitter>();
		GetColourMaterials();
		_currentColour = new AvalancheColour();
	}

	private void Update()
	{
		if (HillCollapser.Instance == null)
		{
			return;
		}
		float normalisedSpeed = HillCollapser.Instance.NormalisedSpeed;
		float num = Mathf.Lerp(_RollerSpeedMin, _RollerSpeedMax, normalisedSpeed);
		if (HillCollapser.Instance.Stopped)
		{
			num = 0f;
		}
		if (HillCollapser.Instance.CurrentState == HillCollapser.State.WaitingToRoll)
		{
			for (int i = 0; i < _particles.Length; i++)
			{
				_particles[i].emit = false;
				_particles[i].ClearParticles();
			}
		}
		else
		{
			for (int j = 0; j < _particles.Length; j++)
			{
				_particles[j].emit = true;
			}
		}
		AnimateRoller(num);
		SetColour();
		float speed = num * _UV_Roller_SpeedRatio;
		ScrollUVs(speed);
	}

	private void AnimateRoller(float speed)
	{
		_rollerAnimState.speed = speed;
	}

	private void ScrollUVs(float speed)
	{
		foreach (Renderer item in _RenderersToScroll)
		{
			Vector2 mainTextureOffset = item.material.mainTextureOffset;
			mainTextureOffset.y += speed * Time.deltaTime;
			if (mainTextureOffset.y > 1f)
			{
				mainTextureOffset.y -= 1f;
			}
			item.material.mainTextureOffset = mainTextureOffset;
		}
	}

	private void GetColourMaterials()
	{
		_materialsToColour = new List<Material>(8);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (renderer.material.HasProperty("_Saturation"))
			{
				_materialsToColour.Add(renderer.material);
			}
		}
	}

	private void SetColour()
	{
		GetTargetColour();
		if (!(_colourChangeTime <= 0f))
		{
			_colourChangeTime -= Time.deltaTime;
			float value = _colourChangeTime / _ColourAnimTime;
			value = Mathf.Clamp01(value);
			value = 1f - value;
			AvalancheColour avalancheColour = ((_targetColourLevel <= _lastColourLevel) ? _FlashColourBad : _FlashColourGood);
			if (value < 0.5f)
			{
				value *= 2f;
				_currentColour.Lerp(_lastColour, avalancheColour, value);
			}
			else
			{
				value *= 2f;
				value -= 1f;
				_currentColour.Lerp(avalancheColour, _targetColour, value);
			}
			UseColour(_currentColour);
		}
	}

	private void GetTargetColour()
	{
		if (CurrentGameMode.Type == GameModeType.MonsterLove)
		{
			GetTargetColourForMonsterLove();
		}
		else
		{
			GetStandardTargetColour();
		}
	}

	private void GetStandardTargetColour()
	{
		SetTargetColour(_StandardColour);
	}

	private void SetTargetColour(AvalancheColour colour)
	{
		if (_targetColour != colour)
		{
			_lastColour = _targetColour;
			_targetColour = colour;
			if (_lastColour == null)
			{
				UseColour(_targetColour);
			}
			else
			{
				_colourChangeTime = _ColourAnimTime;
			}
		}
	}

	private void UseColour(AvalancheColour colour)
	{
		foreach (Material item in _materialsToColour)
		{
			colour.UseOnMaterial(item);
		}
	}

	private void GetTargetColourForMonsterLove()
	{
		if (HillCollapser.Instance.CurrentState <= HillCollapser.State.Rolling)
		{
			int levelIndex = HillCollapser.Instance.LevelIndex;
			if (levelIndex < _MonsterLoveLevelColours.Count)
			{
				SetTargetColour(_MonsterLoveLevelColours[levelIndex]);
				if (_targetColourLevel != levelIndex)
				{
					_lastColourLevel = _targetColourLevel;
					_targetColourLevel = levelIndex;
				}
				return;
			}
		}
		_lastColourLevel = (_targetColourLevel = 1);
		SetTargetColour(_MonsterLoveFinalColour);
	}
}
