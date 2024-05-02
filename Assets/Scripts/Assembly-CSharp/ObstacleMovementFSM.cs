using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMovementFSM : ObstacleMovement
{
	[Serializable]
	public class AnimSection
	{
		public int _FirstFrame;

		public int _LastFrame;

		public bool IncludesFrame(int frame)
		{
			return frame >= _FirstFrame && frame <= _LastFrame;
		}
	}

	public float _SpeedMPS;

	public bool _HasEastMovement = true;

	public bool _HasWestMovement = true;

	public List<AnimSection> _StopSections;

	private bool _changedDirection;

	private AnimatedSprite _anim;

	private void Start()
	{
		_anim = _obstacle.Visuals.AnimatedSprite;
		SetNextDirection();
	}

	public void Update()
	{
		if (_anim.CurrentFrame == _anim.LastFrame)
		{
			if (!_changedDirection)
			{
				SetNextDirection();
			}
		}
		else
		{
			_changedDirection = false;
		}
	}

	public override void UpdateRegularMovement()
	{
		foreach (AnimSection stopSection in _StopSections)
		{
			if (stopSection.IncludesFrame(_anim.CurrentFrame))
			{
				return;
			}
		}
		Vector3 position = base.transform.position;
		float num = _SpeedMPS * Time.fixedDeltaTime;
		if (Direction == Heading.West)
		{
			num *= -1f;
		}
		position.x += num;
		if (!HasBreachedBounds(position.x))
		{
			base.transform.position = position;
		}
	}

	private Heading GetNextDirection()
	{
		if (_HasEastMovement && !_HasWestMovement)
		{
			return Heading.East;
		}
		if (_HasWestMovement && !_HasEastMovement)
		{
			return Heading.West;
		}
		if (!_HasEastMovement && !_HasWestMovement)
		{
			return Heading.Stationary;
		}
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			return Heading.West;
		}
		return Heading.East;
	}

	private void SetNextDirection()
	{
		SetDirection(GetNextDirection());
	}
}
