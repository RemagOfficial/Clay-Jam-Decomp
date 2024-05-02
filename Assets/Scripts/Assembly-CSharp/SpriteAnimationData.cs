using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpriteAnimationData
{
	public string _Name;

	public List<Texture2D> _Textures = new List<Texture2D>();

	public int _NumTilesX = 1;

	public int _NumTilesY = 1;

	public int _Fps = 10;

	public List<int> _ScriptedFrames = new List<int>();

	public int _StartTile;

	public int _EndTile = -1;

	public SpriteAnimationType _Type;

	private bool _looped;

	private bool _aboutToLoop;

	private float _playTime;

	private bool _forwards = true;

	private bool _paused;

	private bool _scaled;

	private int _rowsPerTexture;

	public int CurrentFrame { get; private set; }

	public FrameData CurrentFrameData { get; private set; }

	public int CurrentTileIndex { get; private set; }

	public bool AboutToLoop
	{
		get
		{
			return _aboutToLoop;
		}
	}

	private Vector2 TileSize { get; set; }

	public int LastFrame
	{
		get
		{
			return NumFrames - 1;
		}
	}

	private bool UsingScriptedFrames
	{
		get
		{
			return _ScriptedFrames.Count > 0;
		}
	}

	public int NumFrames
	{
		get
		{
			if (UsingScriptedFrames)
			{
				return _ScriptedFrames.Count;
			}
			return _EndTile - _StartTile + 1;
		}
	}

	public bool Finished
	{
		get
		{
			return _looped;
		}
	}

	public void PlayForward()
	{
		bool forwards = _forwards;
		_forwards = true;
		if (!forwards)
		{
			StartFromFrame(NumFrames - 1 - CurrentFrame);
		}
		else
		{
			_paused = false;
		}
	}

	public void PlayBackward()
	{
		bool flag = !_forwards;
		_forwards = false;
		if (!flag)
		{
			StartFromFrame(NumFrames - 1 - CurrentFrame);
		}
		else
		{
			_paused = false;
		}
	}

	public void Stop()
	{
		_paused = true;
	}

	public bool Initialise(AnimatedSprite parentSprite)
	{
		_scaled = parentSprite._ScaleInCode;
		if (_NumTilesX <= 0)
		{
			Debug.LogError(string.Format("SpriteAnimationData._NumTilesX must be greater than 0 not {0}", _NumTilesX));
			return false;
		}
		if (_NumTilesY <= 0)
		{
			Debug.LogError(string.Format("SpriteAnimationData._NumTilesY must be greater than 0 not {0}", _NumTilesY));
			return false;
		}
		if (_Textures.Count == 0)
		{
			Debug.LogError("SpriteAnimationData._Textures must have some entries");
			return false;
		}
		if (_Textures.Count > _NumTilesY)
		{
			Debug.LogError("SpriteAnimationData._Textures must not be less than _NumTilesY");
			return false;
		}
		if (_EndTile == -1)
		{
			_EndTile = _NumTilesX * _NumTilesY - 1;
		}
		_rowsPerTexture = _NumTilesY / _Textures.Count;
		TileSize = new Vector2(1f / (float)_NumTilesX, 1f / (float)_rowsPerTexture);
		CurrentFrameData = new FrameData();
		Start();
		return true;
	}

	public void Start()
	{
		StartFromFrame(0);
	}

	public void StartFromFrame(int firstFrame)
	{
		_looped = false;
		_playTime = _Fps * firstFrame;
		_paused = false;
		SetNewFrame(firstFrame);
	}

	public void Continue()
	{
		_paused = false;
	}

	public void Update()
	{
		if (_paused)
		{
			return;
		}
		_playTime += Time.deltaTime;
		int num = (int)(_playTime * (float)_Fps) % NumFrames;
		if (CurrentFrame > num || (_ScriptedFrames.Count == 1 && CurrentFrame == 0))
		{
			_looped = true;
			_aboutToLoop = false;
			if (_Type == SpriteAnimationType.Bounce)
			{
				_forwards = !_forwards;
			}
		}
		else if (CurrentFrame == num || (_ScriptedFrames.Count == 1 && CurrentFrame == 0))
		{
			_aboutToLoop = true;
		}
		if (_Type == SpriteAnimationType.Once && _looped)
		{
			num = LastFrame;
		}
		if (num != CurrentFrame)
		{
			SetNewFrame(num);
		}
	}

	public int FindFirstMatchingFrame(SpriteAnimationData otherAnim)
	{
		if (otherAnim._Textures[0] != _Textures[0])
		{
			return -1;
		}
		int num = otherAnim.TileIndexFromFrame(otherAnim.CurrentFrame);
		for (int i = 1; i < NumFrames; i++)
		{
			if (TileIndexFromFrame(i) == num)
			{
				return i;
			}
		}
		return -1;
	}

	private void SetNewFrame(int frame)
	{
		CurrentFrame = frame;
		SetDataForFrame(CurrentFrame);
	}

	private int TileIndexFromFrame(int frame)
	{
		int num = frame;
		num = (UsingScriptedFrames ? _ScriptedFrames[frame] : ((!_forwards) ? (_EndTile - frame) : (_StartTile + frame)));
		return --num;
	}

	private void SetDataForFrame(int frame)
	{
		CurrentTileIndex = TileIndexFromFrame(frame);
		int num = CurrentTileIndex % _NumTilesX;
		int num2 = CurrentTileIndex / _NumTilesX;
		int index = num2 / _rowsPerTexture;
		num2 %= _rowsPerTexture;
		Texture2D texture = _Textures[index];
		Vector2 offset = ((!_scaled) ? new Vector2((float)num * TileSize.x, 0f - (float)num2 * TileSize.y) : new Vector2((float)num * TileSize.x, 1f - ((float)num2 + 1f) * TileSize.y));
		CurrentFrameData.Texture = texture;
		CurrentFrameData.Offset = offset;
		CurrentFrameData.Scale = TileSize;
	}
}
