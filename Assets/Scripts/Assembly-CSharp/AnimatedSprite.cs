using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Obstacle Creation/Animated Sprite")]
public class AnimatedSprite : MonoBehaviour
{
	public bool _AlwaysUseAttachedRenderer;

	public bool _ScaleInCode;

	public List<SpriteAnimationData> _Anims = new List<SpriteAnimationData>();

	public int _FirstAnimation;

	private SpriteAnimationData _currentAnim;

	public Material Material { get; set; }

	public int CurrentFrame
	{
		get
		{
			return _currentAnim.CurrentFrame;
		}
	}

	public bool AboutToLoop
	{
		get
		{
			return _currentAnim.AboutToLoop;
		}
	}

	public bool NextFrameIsLast
	{
		get
		{
			return _currentAnim.CurrentFrame + 1 == _currentAnim._ScriptedFrames.Count;
		}
	}

	public FrameData CurrentFrameData
	{
		get
		{
			return _currentAnim.CurrentFrameData;
		}
	}

	public int CurrentTileIndex
	{
		get
		{
			return _currentAnim.CurrentTileIndex;
		}
	}

	public int LastFrame
	{
		get
		{
			return _currentAnim.LastFrame;
		}
	}

	public bool Finished
	{
		get
		{
			return _currentAnim.Finished;
		}
	}

	private void Awake()
	{
		DoInit();
	}

	public void DoInit()
	{
		if (_Anims.Count == 0)
		{
			return;
		}
		if (_FirstAnimation >= _Anims.Count)
		{
			Debug.LogError(string.Format("_FirstAnimation {0} is not in _Anims", _FirstAnimation), base.gameObject);
			_FirstAnimation = 0;
		}
		foreach (SpriteAnimationData anim in _Anims)
		{
			if (!anim.Initialise(this))
			{
				Debug.LogError("Bad SpriteAnimationData", base.gameObject);
			}
		}
		if (base.renderer != null)
		{
			Material = base.renderer.material;
		}
		StartAnimation(_Anims[_FirstAnimation], 0);
	}

	public void Play(string animName)
	{
		if (animName == _currentAnim._Name)
		{
			_currentAnim.Continue();
		}
		else
		{
			ForcePlay(animName);
		}
	}

	public void ForcePlay(string animName)
	{
		SpriteAnimationData spriteAnimationData = _Anims.Find((SpriteAnimationData x) => x._Name == animName);
		if (spriteAnimationData == null)
		{
			Debug.LogError(string.Format("Animation data {0} not found", animName), base.gameObject);
			return;
		}
		int firstFrame = -1;
		StartAnimation(spriteAnimationData, firstFrame);
	}

	public void PlayForward()
	{
		_currentAnim.PlayForward();
	}

	public void PlayBackward()
	{
		_currentAnim.PlayBackward();
	}

	public void Stop()
	{
		_currentAnim.Stop();
	}

	private void StartAnimation(SpriteAnimationData data, int firstFrame)
	{
		_currentAnim = data;
		if (firstFrame < 0)
		{
			_currentAnim.Start();
		}
		else
		{
			_currentAnim.StartFromFrame(firstFrame);
		}
	}

	private void Update()
	{
		_currentAnim.Update();
		if (_AlwaysUseAttachedRenderer && base.renderer != null && base.renderer.material != null)
		{
			Material = base.renderer.material;
		}
		if ((bool)Material)
		{
			SetTextureOnMaterial();
		}
	}

	public bool HasAnim(string animName)
	{
		return _Anims.Exists((SpriteAnimationData x) => x._Name == animName);
	}

	private void SetTextureOnMaterial()
	{
		Material.mainTexture = CurrentFrameData.Texture;
		if (_ScaleInCode)
		{
			Material.mainTextureOffset = CurrentFrameData.Offset;
			Material.mainTextureScale = CurrentFrameData.Scale;
		}
		else
		{
			Material.mainTextureOffset = CurrentFrameData.Offset;
		}
	}

	public void ShowFrame(int frame)
	{
		_currentAnim.StartFromFrame(frame);
		_currentAnim.Stop();
	}

	public SpriteAnimationData GetAnimData(string animName)
	{
		return _Anims.Find((SpriteAnimationData data) => data._Name == animName);
	}

	public void ShowFrameAtNormalisedTime(float p)
	{
		int num = Mathf.CeilToInt((float)_currentAnim.NumFrames * p);
		num--;
		ShowFrame(num);
	}
}
