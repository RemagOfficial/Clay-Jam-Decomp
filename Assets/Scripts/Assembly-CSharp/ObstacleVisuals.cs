using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Obstacle Creation/Obstacle Visuals")]
public class ObstacleVisuals : MonoBehaviour
{
	public UnityEngine.Object _Model;

	private GameObject _modelObject;

	public UnityEngine.Object _AnimatedSpritePrefab;

	private List<Material> _animatedMaterials = new List<Material>();

	public UnityEngine.Object _AnimatedSpritePrefabExtra;

	private Material _animatedMaterialExtra;

	private Material _animatedMaterialExtraShadow;

	private List<Material> _materialsToColour = new List<Material>();

	private HSVColour _colour;

	private GameObject _notSquashableGO;

	private Vector3 _notSquashableOffset;

	private static string _billboardedNode = "Character";

	private static string _mainMeshName = "CharacterModel";

	private static string _altMeshName = "CharacterModelAlt";

	private static string _extraMeshName = "CharacterModelExtra";

	private static string _shadowMeshName = "CharacterShadow";

	private static string _squashableIndicatorMeshName = "Squashable";

	private static string _squashableIndicatorShadowMeshName = "SquashableShad";

	private static Material _mainMaterialHSV;

	private static string _mainMaterialResourcePathHSV = "Obstacles/Materials/ObstacleHSVMaterial";

	private static Material _mainMaterialHSVSolid;

	private static string _mainMaterialResourcePathHSVSolid = "Obstacles/Materials/ObstacleHSVMaterialSolid";

	private static Material _mainMaterial;

	private static string _mainMaterialResourcePath = "Obstacles/Materials/ObstacleMaterial";

	private static Material _mainMaterialSolid;

	private static string _mainMaterialResourcePathSolid = "Obstacles/Materials/ObstacleMaterialSolid";

	private static Material _mainMaterialVertCol;

	private static string _mainMaterialResourcePathVertCol = "Obstacles/Materials/ObstacleMatVertCol";

	private static Material _mainMaterialHSVVertCol;

	private static string _mainMaterialResourcePathHSVVertCol = "Obstacles/Materials/ObstacleHSVMaterialVert";

	private static Material _altMaterialHSV;

	private static string _altMaterialResourcePathHSV = "Obstacles/Materials/ObstacleHSVMaterialAlt";

	private static Material _shadowMaterial;

	private static string _shadowMaterialResourcePath = "Obstacles/Materials/ObstacleShadowMaterial";

	private static Material _shadowMaterialSolid;

	private static string _shadowMaterialResourcePathSolid = "Obstacles/Materials/ObstacleShadowVertColMat";

	private static Material _notSquashableMaterial;

	private static string _notSquashableMaterialResourcePath = "Obstacles/Materials/SquashableMat";

	private static Material _notSquashableShadowMaterial;

	private static string _notSquashableShadowMaterialResourcePath = "Obstacles/Materials/SquashableShadMat";

	private static string _baseMaterilaPath = "Obstacles/Materials/";

	private static SpriteAnimationData _exclamationAnimationData;

	private static string _exclamationAnimationDataResourcePath = "Obstacles/Animations/Exclamation";

	private string _modelMovementAnimation;

	private string _spriteMovementAnimation;

	private ObstacleMould _obstacleMould;

	private ObstacleDefinition _obstacleDefintion;

	private Action _animCallBack;

	private int PreviousSpriteAnimationFrame;

	private bool _playingOneShot;

	private Renderer[] _childRenderers;

	private Transform _transformToBillBoard;

	private Vector3 _touchExtents;

	private static string SplatPointerResourcePath = "Obstacles/Prefabs/SplatPointer";

	private static UnityEngine.Object SplatPointerPrefab;

	private GameObject _splatPointerObject;

	private static string HeartPrefabResourcePath = "Obstacles/Prefabs/HeartPointer";

	private static UnityEngine.Object HeartPrefab;

	private GameObject _heartObject;

	private bool _squashPosIsForcedOnScreen;

	public AnimatedSprite AnimatedSprite { get; private set; }

	private AnimatedSprite AnimatedSpriteExtra { get; set; }

	private Renderer _notSquashableIndicator { get; set; }

	private Renderer _notSquashableIndicatorShadow { get; set; }

	public Transform CharacterTransform { get; private set; }

	private Transform CharacterTransformAlt { get; set; }

	private bool IsActive { get; set; }

	private void Update()
	{
		if (!IsActive)
		{
			return;
		}
		if (AnimatedSprite.Finished)
		{
			if (_animCallBack != null)
			{
				_animCallBack();
			}
			_animCallBack = null;
			if (_playingOneShot)
			{
				PlayMovementAnimation();
			}
		}
		SetMaterialTexturesFromSpriteAnimation();
		ChooseCharatcterTransformToShow();
		BillBoard();
		DebugShowSplatBounds();
	}

	private void LateUpdate()
	{
		UpdateNotSquashable();
		UpdateSplatPointer();
		UpdateHeart();
	}

	private void UpdateNotSquashable()
	{
		_squashPosIsForcedOnScreen = false;
		if (!(_notSquashableIndicator == null))
		{
			float num = CameraDirector.ScreenTop - 2.25f;
			float z = base.transform.position.z;
			Vector3 position = _notSquashableGO.transform.transform.position;
			if (num < z)
			{
				position.y = 1f;
				position.z = base.transform.position.z;
				_squashPosIsForcedOnScreen = true;
			}
			_notSquashableOffset.z = base.transform.position.z - Mathf.Min(num, z);
			position -= _notSquashableOffset;
			_notSquashableGO.transform.transform.position = position;
		}
	}

	private void UpdateHeart()
	{
		if (CurrentGameMode.Type == GameModeType.MonsterLove && !(_heartObject == null))
		{
			bool flag = GameModeStateMonsterLove.Instance.HasAHeart(_obstacleMould);
			flag &= !_notSquashableIndicator.enabled;
			TurnOnHeart(flag);
		}
	}

	private void TurnOnHeart(bool on)
	{
		if (_heartObject != null)
		{
			ShowObjectAtSquashablePos(on, _heartObject);
		}
	}

	private void UpdateSplatPointer()
	{
		if (!(_splatPointerObject == null))
		{
			bool splatFingerIsOn = Pebble.Instance.PowerUpManager.SplatFingerIsOn;
			ShowObjectAtSquashablePos(splatFingerIsOn, _splatPointerObject);
		}
	}

	private void ShowObjectAtSquashablePos(bool shouldShow, GameObject obj)
	{
		if (!shouldShow || _squashPosIsForcedOnScreen)
		{
			if (obj.active)
			{
				obj.SetActiveRecursively(false);
			}
			return;
		}
		if (!obj.active)
		{
			obj.SetActiveRecursively(true);
		}
		PositionAtNotSquashablePos(obj);
	}

	private void PositionAtNotSquashablePos(GameObject obj)
	{
		float x = 1f / base.transform.parent.localScale.x;
		float y = 1f / base.transform.parent.localScale.y;
		float z = 1f / base.transform.parent.localScale.z;
		obj.transform.localScale = new Vector3(x, y, z);
		obj.transform.rotation = CameraDirector.Instance.FixedBillboardRotation;
		obj.transform.position = _notSquashableGO.transform.position;
	}

	public void Initialise(ObstacleMould obstacle)
	{
		_obstacleMould = obstacle;
		_obstacleDefintion = obstacle.Definition;
		LoadMaterials();
		LoadPrefabs();
		if (_exclamationAnimationData == null)
		{
			GameObject gameObject = Resources.Load(_exclamationAnimationDataResourcePath) as GameObject;
			AnimatedSprite component = gameObject.GetComponent<AnimatedSprite>();
			_exclamationAnimationData = component._Anims[0];
		}
		InitModel();
		InitSpriteAnimation();
		SetMaterialTexturesFromSpriteAnimation();
		GetChildRenders();
		AddSplatPointer();
		AddHeart();
	}

	public void Activate()
	{
		_modelObject.animation.cullingType = AnimationCullingType.AlwaysAnimate;
		AnimatedSprite.enabled = true;
		if (AnimatedSpriteExtra != null)
		{
			AnimatedSpriteExtra.enabled = true;
		}
		PlayMovementAnimation();
		IsActive = true;
		InitialiseTouchExtents();
	}

	public void Deactivate()
	{
		_modelObject.animation.Stop();
		_modelObject.animation.cullingType = AnimationCullingType.BasedOnRenderers;
		AnimatedSprite.enabled = false;
		if (AnimatedSpriteExtra != null)
		{
			AnimatedSpriteExtra.enabled = false;
		}
		IsActive = false;
	}

	public bool IsVisible()
	{
		Renderer[] childRenderers = _childRenderers;
		foreach (Renderer renderer in childRenderers)
		{
			if (renderer.isVisible)
			{
				return true;
			}
		}
		return false;
	}

	private void SetMaterialTexturesFromSpriteAnimation()
	{
		foreach (Material animatedMaterial in _animatedMaterials)
		{
			animatedMaterial.mainTexture = AnimatedSprite.CurrentFrameData.Texture;
			animatedMaterial.mainTextureOffset = AnimatedSprite.CurrentFrameData.Offset;
			if (AnimatedSprite._ScaleInCode)
			{
				animatedMaterial.mainTextureScale = AnimatedSprite.CurrentFrameData.Scale;
			}
		}
		if (_animatedMaterialExtraShadow != null)
		{
			_animatedMaterialExtraShadow.mainTexture = AnimatedSpriteExtra.CurrentFrameData.Texture;
			_animatedMaterialExtraShadow.mainTextureOffset = AnimatedSpriteExtra.CurrentFrameData.Offset;
			if (AnimatedSpriteExtra._ScaleInCode)
			{
				_animatedMaterialExtraShadow.mainTextureScale = AnimatedSpriteExtra.CurrentFrameData.Scale;
			}
		}
	}

	public void PlayAnimationIfAvailable(string name)
	{
		if (AnimatedSprite.HasAnim(name))
		{
			PlayAnimation(name);
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
			_modelObject.animation.Play(name);
		}
		AnimatedSprite.Stop();
		if (AnimatedSpriteExtra != null)
		{
			AnimatedSpriteExtra.Stop();
		}
		AnimatedSprite.Play(name);
		if (AnimatedSpriteExtra != null)
		{
			AnimatedSpriteExtra.Play(name);
		}
		_playingOneShot = true;
	}

	public void PauseAnimation()
	{
		if (ModelHasAnyAnimation())
		{
			_modelObject.animation.Stop();
		}
		AnimatedSprite.Stop();
		if (AnimatedSpriteExtra != null)
		{
			AnimatedSpriteExtra.Stop();
		}
	}

	private void PlayMovementAnimation()
	{
		if (_modelMovementAnimation != null)
		{
			_modelObject.animation.Rewind(_modelMovementAnimation);
			_modelObject.animation.Play(_modelMovementAnimation);
		}
		AnimatedSprite.ForcePlay(_spriteMovementAnimation);
		if (AnimatedSpriteExtra != null)
		{
			AnimatedSpriteExtra.ForcePlay(_spriteMovementAnimation);
		}
		_playingOneShot = false;
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

	public void SetMovementAnimation(string name)
	{
		_spriteMovementAnimation = name;
		if (ModelHasAnimation(name))
		{
			_modelMovementAnimation = name;
		}
		if (!_playingOneShot)
		{
			PlayMovementAnimation();
		}
	}

	private void LoadMaterials()
	{
		if (_mainMaterial == null)
		{
			LoadMaterial(ref _mainMaterial, _mainMaterialResourcePath);
		}
		if (_shadowMaterial == null)
		{
			LoadMaterial(ref _shadowMaterial, _shadowMaterialResourcePath);
		}
		if (_shadowMaterialSolid == null)
		{
			LoadMaterial(ref _shadowMaterialSolid, _shadowMaterialResourcePathSolid);
		}
		if (_mainMaterialHSV == null)
		{
			LoadMaterial(ref _mainMaterialHSV, _mainMaterialResourcePathHSV);
		}
		if (_mainMaterialSolid == null)
		{
			LoadMaterial(ref _mainMaterialSolid, _mainMaterialResourcePathSolid);
		}
		if (_mainMaterialVertCol == null)
		{
			LoadMaterial(ref _mainMaterialVertCol, _mainMaterialResourcePathVertCol);
		}
		if (_mainMaterialHSVVertCol == null)
		{
			LoadMaterial(ref _mainMaterialHSVVertCol, _mainMaterialResourcePathHSVVertCol);
		}
		if (_mainMaterialHSVSolid == null)
		{
			LoadMaterial(ref _mainMaterialHSVSolid, _mainMaterialResourcePathHSVSolid);
		}
		if (_altMaterialHSV == null)
		{
			LoadMaterial(ref _altMaterialHSV, _altMaterialResourcePathHSV);
		}
		if (_notSquashableMaterial == null)
		{
			LoadMaterial(ref _notSquashableMaterial, _notSquashableMaterialResourcePath);
		}
		if (_notSquashableShadowMaterial == null)
		{
			LoadMaterial(ref _notSquashableShadowMaterial, _notSquashableShadowMaterialResourcePath);
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
			SetMaterials();
			AddBillBoarding();
		}
	}

	private bool InstantiateModel()
	{
		if (_Model == null)
		{
			SetupError(string.Format("Model is missing from ObstacleVisuals {0}", base.name));
			return false;
		}
		_modelObject = UnityEngine.Object.Instantiate(_Model) as GameObject;
		_modelObject.transform.parent = base.transform;
		_modelObject.transform.localScale = base.transform.localScale;
		_modelObject.name = "Model";
		return true;
	}

	private void SetMaterials()
	{
		Renderer[] componentsInChildren = _modelObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			bool flag = false;
			if (renderer.gameObject.name == _mainMeshName)
			{
				if (_obstacleDefintion.UsesHSVShader)
				{
					if (_obstacleDefintion._IsVertColoured)
					{
						renderer.material = _mainMaterialHSVVertCol;
					}
					else if (_obstacleDefintion._IsPolyCutOut)
					{
						renderer.material = _mainMaterialHSVSolid;
					}
					else
					{
						renderer.material = _mainMaterialHSV;
					}
					_materialsToColour.Add(renderer.material);
				}
				else if (_obstacleDefintion._IsVertColoured)
				{
					renderer.material = _mainMaterialVertCol;
				}
				else if (_obstacleDefintion._IsPolyCutOut)
				{
					renderer.material = _mainMaterialSolid;
				}
				else
				{
					renderer.material = _mainMaterial;
				}
				CharacterTransform = renderer.transform;
				flag = true;
			}
			else if (renderer.gameObject.name == _altMeshName)
			{
				if (_obstacleDefintion.UsesHSVShader)
				{
					renderer.material = _altMaterialHSV;
					_materialsToColour.Add(renderer.material);
				}
				else
				{
					Debug.LogError(_altMeshName + " mesh for non HSV colooured obstacle is not supported.A non hsv - render below pebble materila would be needed");
				}
				CharacterTransformAlt = renderer.transform;
				flag = true;
			}
			else if (renderer.gameObject.name == _extraMeshName)
			{
				Material mat = new Material(renderer.material);
				SetExtraMaterial(ref mat);
				renderer.material = mat;
				_animatedMaterialExtra = renderer.material;
			}
			else if (renderer.gameObject.name == _shadowMeshName)
			{
				if (_obstacleDefintion._IsPolyCutOut)
				{
					renderer.material = _shadowMaterialSolid;
				}
				else
				{
					renderer.material = _shadowMaterial;
				}
				if (string.IsNullOrEmpty(_obstacleDefintion.ExtraMaterialName))
				{
					flag = true;
				}
				else
				{
					_animatedMaterialExtraShadow = renderer.material;
				}
			}
			else if (renderer.gameObject.name == _squashableIndicatorMeshName)
			{
				_notSquashableGO = renderer.gameObject;
				_notSquashableOffset = Vector3.zero;
				_notSquashableIndicator = renderer;
				_notSquashableIndicator.material = _notSquashableMaterial;
				SpriteAnimationData spriteAnimationData = new SpriteAnimationData();
				spriteAnimationData._Name = _exclamationAnimationData._Name;
				spriteAnimationData._Textures.Add(_exclamationAnimationData._Textures[0]);
				spriteAnimationData._NumTilesX = _exclamationAnimationData._NumTilesX;
				spriteAnimationData._NumTilesY = _exclamationAnimationData._NumTilesY;
				spriteAnimationData._StartTile = _exclamationAnimationData._StartTile;
				spriteAnimationData._EndTile = _exclamationAnimationData._EndTile;
				spriteAnimationData._Fps = _exclamationAnimationData._Fps;
				AnimatedSprite animatedSprite = renderer.gameObject.AddComponent<AnimatedSprite>();
				animatedSprite._Anims.Add(spriteAnimationData);
				animatedSprite._ScaleInCode = false;
				animatedSprite.DoInit();
			}
			else if (renderer.gameObject.name == _squashableIndicatorShadowMeshName)
			{
				_notSquashableIndicatorShadow = renderer;
				_notSquashableIndicatorShadow.material = _notSquashableShadowMaterial;
			}
			if (flag)
			{
				_animatedMaterials.Add(renderer.material);
			}
		}
	}

	private void AddBillBoarding()
	{
		Transform[] componentsInChildren = _modelObject.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.gameObject.name == _billboardedNode)
			{
				_transformToBillBoard = transform;
				break;
			}
		}
	}

	private void InitSpriteAnimation()
	{
		if (_AnimatedSpritePrefabExtra != null)
		{
			InstantiateSpriteAnimationExtra();
		}
		if (InstantiateSpriteAnimation())
		{
			SetDefaultMovementAnimation();
		}
	}

	private void InstantiateSpriteAnimationExtra()
	{
		if (_animatedMaterialExtra == null)
		{
			Debug.LogError(string.Format("We have an animated sprite 'extra' but there was no {0} model to assign a material to", _extraMeshName));
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(_AnimatedSpritePrefabExtra) as GameObject;
		AnimatedSpriteExtra = gameObject.GetComponent<AnimatedSprite>();
		if (AnimatedSpriteExtra == null)
		{
			SetupError(string.Format("AnimatedSpritePrefabExtra {0} does not have an AnimatedSpriteExtra component on object {1}", _AnimatedSpritePrefab.name, base.name));
			return;
		}
		AnimatedSpriteExtra.transform.parent = base.transform;
		AnimatedSpriteExtra.name = "SpriteAnimExtra";
		AnimatedSpriteExtra.Material = _animatedMaterialExtra;
	}

	private bool InstantiateSpriteAnimation()
	{
		if (_AnimatedSpritePrefab == null)
		{
			SetupError(string.Format("AnimatedSprite is missing from ObstacleVisuals {0}", base.name));
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

	private void SetDefaultMovementAnimation()
	{
		for (int i = 0; i < 9; i++)
		{
			string text = ObstacleMovement.AnimationName((ObstacleMovement.Heading)i);
			if ((bool)AnimatedSprite && AnimatedSprite.HasAnim(text))
			{
				_spriteMovementAnimation = text;
			}
			if (ModelHasAnimation(text))
			{
				_modelMovementAnimation = text;
			}
		}
	}

	private void SetupError(string error)
	{
		Debug.LogError(error, base.gameObject);
		base.enabled = false;
	}

	public void SetColour(HSVColour colour)
	{
		if (_materialsToColour.Count == 0)
		{
			Debug.LogError("Trying to set colour on an obstacle without an HSV material", base.gameObject);
		}
		_colour = colour;
		foreach (Material item in _materialsToColour)
		{
			_colour.UseOnHSVFastMaterial(item);
		}
	}

	public void MarkSquashable(bool squashable)
	{
		if ((bool)_notSquashableIndicator)
		{
			_notSquashableIndicator.enabled = !squashable;
		}
		if ((bool)_notSquashableIndicatorShadow)
		{
			_notSquashableIndicatorShadow.enabled = !squashable;
		}
	}

	private void GetChildRenders()
	{
		_childRenderers = GetComponentsInChildren<Renderer>();
	}

	private void BillBoard()
	{
		_transformToBillBoard.rotation = CameraDirector.Instance.FixedBillboardRotation;
	}

	private void ChooseCharatcterTransformToShow()
	{
		if (_obstacleDefintion._NonCollideableAnimTiles.Count == 0)
		{
			return;
		}
		int currentTileIndex = AnimatedSprite.CurrentTileIndex;
		if (_obstacleDefintion._NonCollideableAnimTiles.Contains(currentTileIndex))
		{
			if (!CharacterTransformAlt.gameObject.activeSelf)
			{
				CharacterTransformAlt.gameObject.SetActive(true);
				Debug.Log("tunring alt on", CharacterTransformAlt.gameObject);
			}
			if (CharacterTransform.gameObject.renderer.enabled)
			{
				CharacterTransform.gameObject.renderer.enabled = false;
				Debug.Log("tunring cha off", CharacterTransform.gameObject);
			}
		}
		else
		{
			if (!CharacterTransform.gameObject.renderer.enabled)
			{
				CharacterTransform.gameObject.renderer.enabled = true;
			}
			if (CharacterTransformAlt.gameObject.activeSelf)
			{
				CharacterTransformAlt.gameObject.SetActive(false);
			}
		}
	}

	public void OnSpawn()
	{
		BillBoard();
		if (CurrentGameMode.Type != GameModeType.MonsterLove)
		{
			TurnOnHeart(false);
		}
	}

	private void SetExtraMaterial(ref Material mat)
	{
		string resourcePath = string.Format("{0}{1}", _baseMaterilaPath, _obstacleDefintion.ExtraMaterialName);
		LoadMaterial(ref mat, resourcePath);
	}

	private void InitialiseTouchExtents()
	{
		_touchExtents = CharacterTransform.renderer.bounds.extents * 2f;
		if (_touchExtents.x < 1f)
		{
			_touchExtents.x = 1f;
		}
	}

	public bool TouchedByRay(Ray ray)
	{
		Bounds bounds = new Bounds(CharacterTransform.renderer.bounds.center, _touchExtents);
		return bounds.IntersectRay(ray);
	}

	private void DebugShowSplatBounds()
	{
		Bounds bounds = new Bounds(CharacterTransform.renderer.bounds.center, _touchExtents);
		Vector3 min = bounds.min;
		Vector3 min2 = bounds.min;
		min2.x = bounds.max.x;
		Debug.DrawLine(min, min2);
		min2 = bounds.min;
		min2.y = bounds.max.y;
		Debug.DrawLine(min, min2);
		min2 = bounds.min;
		min2.z = bounds.max.z;
		Debug.DrawLine(min, min2);
		min = bounds.max;
		min2 = bounds.max;
		min2.x = bounds.min.x;
		Debug.DrawLine(min, min2);
		min2 = bounds.max;
		min2.y = bounds.min.y;
		Debug.DrawLine(min, min2);
		min2 = bounds.max;
		min2.z = bounds.min.z;
		Debug.DrawLine(min, min2);
		Debug.DrawRay(HillInput.Instance.CurrentTouchRay.origin, HillInput.Instance.CurrentTouchRay.direction * 20f, Color.black);
	}

	private void LoadPrefabs()
	{
		if (SplatPointerPrefab == null)
		{
			SplatPointerPrefab = Resources.Load(SplatPointerResourcePath);
		}
		if (HeartPrefab == null)
		{
			HeartPrefab = Resources.Load(HeartPrefabResourcePath);
		}
	}

	private void AddSplatPointer()
	{
		if (_obstacleDefintion.Splattable)
		{
			_splatPointerObject = UnityEngine.Object.Instantiate(SplatPointerPrefab) as GameObject;
			_splatPointerObject.transform.parent = base.transform.parent;
		}
	}

	private void AddHeart()
	{
		if (GameModeStateMonsterLove.IsMatchable(_obstacleDefintion))
		{
			_heartObject = UnityEngine.Object.Instantiate(HeartPrefab) as GameObject;
			_heartObject.transform.parent = base.transform.parent;
			_materialsToColour.Add(_heartObject.GetComponentInChildren<Renderer>().material);
		}
	}
}
