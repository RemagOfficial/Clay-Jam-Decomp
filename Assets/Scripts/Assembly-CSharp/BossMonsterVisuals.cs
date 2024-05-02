using System;
using System.Collections.Generic;
using UnityEngine;

public class BossMonsterVisuals : MonoBehaviour
{
	public UnityEngine.Object _Model;

	private GameObject _modelObject;

	public UnityEngine.Object _AnimatedSpritePrefab;

	private List<Material> _animatedMaterials = new List<Material>();

	private GameObject _frontObject;

	private GameObject _sideObject;

	private static string _frontMeshName = "BossFrontModel";

	private static string _sideMeshName = "BossSideModel";

	private static Material _mainMaterial;

	private static string _mainMaterialResourcePath = "Obstacles/Materials/ObstacleMaterial";

	private static Material _defeatedMaterial;

	private static string _defeatedMaterialResourcePath = "Obstacles/Materials/BossDefeated";

	private Action _animCallBack;

	private AnimationState currentPlayingAnim;

	public AnimatedSprite AnimatedSprite { get; private set; }

	private void Update()
	{
		if (AnimatedSprite.Finished)
		{
			if (_animCallBack != null)
			{
				_animCallBack();
			}
			_animCallBack = null;
		}
		SetMaterialTexturesFromSpriteAnimation();
	}

	public void Initialise()
	{
		LoadMaterials();
		InitModel();
		InitSpriteAnimation();
		SetMaterialTexturesFromSpriteAnimation();
		currentPlayingAnim = null;
	}

	public void SwitchToSide()
	{
		if (_sideObject != null)
		{
			_frontObject.active = false;
			_sideObject.active = true;
		}
	}

	public void SwitchToFront()
	{
		if (_sideObject != null)
		{
			_frontObject.active = true;
			_sideObject.active = false;
		}
	}

	private void SetMaterialTexturesFromSpriteAnimation()
	{
		for (int i = 0; i < _animatedMaterials.Count; i++)
		{
			_animatedMaterials[i].mainTexture = AnimatedSprite.CurrentFrameData.Texture;
			_animatedMaterials[i].mainTextureOffset = AnimatedSprite.CurrentFrameData.Offset;
		}
	}

	public void PlayAnimationWithCallBack(string name, Action callback)
	{
		_animCallBack = callback;
		PlayAnimation(name);
	}

	public void PlayAnimation(string name)
	{
		if (ModelHasAnimation(name))
		{
			currentPlayingAnim = _modelObject.animation[name];
			currentPlayingAnim.speed = 1f;
			currentPlayingAnim.time = 0f;
			_modelObject.animation.Play(name);
		}
		else if ((bool)currentPlayingAnim)
		{
			currentPlayingAnim.time = 0f;
			currentPlayingAnim.speed = 0f;
		}
		AnimatedSprite.ForcePlay(name);
	}

	public void PauseAnimation()
	{
		if (ModelHasAnyAnimation())
		{
			_modelObject.animation.Stop();
		}
		AnimatedSprite.Stop();
	}

	private bool ModelHasAnyAnimation()
	{
		return _modelObject.animation != null && _modelObject.animation.GetClipCount() > 0;
	}

	private bool ModelHasAnimation(string animName)
	{
		if (ModelHasAnyAnimation() && _modelObject.animation[animName] != null)
		{
			return true;
		}
		return false;
	}

	private void LoadMaterials()
	{
		if (_mainMaterial == null)
		{
			LoadMaterial(ref _mainMaterial, _mainMaterialResourcePath);
		}
		if (_defeatedMaterial == null)
		{
			LoadMaterial(ref _defeatedMaterial, _defeatedMaterialResourcePath);
		}
	}

	private void LoadMaterial(ref Material material, string resourcePath)
	{
		material = Resources.Load(resourcePath) as Material;
		if (material == null)
		{
			SetupError(string.Format("Path not found :{0}", resourcePath));
		}
	}

	private void InitModel()
	{
		if (InstantiateModel())
		{
			SetMaterialsAndObjectNodes();
		}
	}

	private bool InstantiateModel()
	{
		if (_Model == null)
		{
			SetupError(string.Format("Model is missing from BossMonsterVisuals {0}", base.name));
			return false;
		}
		_modelObject = UnityEngine.Object.Instantiate(_Model) as GameObject;
		_modelObject.transform.parent = base.transform;
		_modelObject.transform.position = base.transform.position;
		_modelObject.transform.rotation = base.transform.rotation;
		_modelObject.name = "Model";
		return true;
	}

	private void SetMaterialsAndObjectNodes()
	{
		_sideObject = null;
		_frontObject = null;
		Renderer[] componentsInChildren = _modelObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (renderer.gameObject.name == _frontMeshName)
			{
				_frontObject = renderer.gameObject;
			}
			else if (renderer.gameObject.name == _sideMeshName)
			{
				_sideObject = renderer.gameObject;
			}
		}
		if (_frontObject == null)
		{
			SetupError(string.Format("BossMonsterVisuals {0} needs a {1} node", base.name, _frontMeshName));
			return;
		}
		ChooseMaterial();
		SwitchToFront();
	}

	public void ChooseMaterial(bool forceDefeated = false)
	{
		Material material = null;
		material = ((!forceDefeated && CurrentHill.Instance.ProgressData._BeastDefeatedCount == 0) ? _mainMaterial : _defeatedMaterial);
		_frontObject.renderer.material = material;
		_sideObject.renderer.material = material;
		_animatedMaterials.Clear();
		_animatedMaterials.Add(_frontObject.renderer.material);
		_animatedMaterials.Add(_sideObject.renderer.material);
	}

	private void InitSpriteAnimation()
	{
		if (InstantiateSpriteAnimation())
		{
		}
	}

	private bool InstantiateSpriteAnimation()
	{
		if (_AnimatedSpritePrefab == null)
		{
			SetupError(string.Format("AnimatedSprite is missing from BossMonsterVisuals {0}", base.name));
			return false;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(_AnimatedSpritePrefab) as GameObject;
		AnimatedSprite = gameObject.GetComponent<AnimatedSprite>();
		if (AnimatedSprite == null)
		{
			SetupError(string.Format("AnimatedSpritePrefab {0} does not have an AnimatedSprite component on object {1}", _AnimatedSpritePrefab.name, base.name));
			return false;
		}
		AnimatedSprite.transform.parent = base.transform;
		AnimatedSprite.name = "SpriteAnim";
		return true;
	}

	private void SetupError(string error)
	{
		Debug.LogError(error, base.gameObject);
		base.enabled = false;
	}

	public void PlayDeathAnimPlusSprite(string spriteAnim)
	{
		_modelObject.animation.Stop();
		_frontObject.animation.Play("BossDieAnim");
		_frontObject.animation.Play("BossResetAnim", AnimationPlayMode.Queue);
		AnimatedSprite.ForcePlay(spriteAnim);
	}

	public bool IsPlayingDeathAnimation()
	{
		return _frontObject.animation.IsPlaying("BossDieAnim") && _frontObject.animation["BossDieAnim"].time < 5f;
	}
}
