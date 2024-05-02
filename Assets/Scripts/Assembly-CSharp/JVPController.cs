using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class JVPController : ManagedComponent
{
	public enum State
	{
		Inactive = 0,
		ZoomedOut = 1,
		ZoomingIn = 2,
		MenuAnimating = 3,
		Interactive = 4,
		Chewing = 5,
		ZoomingOut = 6,
		BuyingHillItem = 7
	}

	public enum ResultsCameraState
	{
		Off = 0,
		Results_Offscren = 1,
		Results_CountingClay = 2,
		Results_ReadyToShop = 3,
		Results_InShop = 4
	}

	private const string ArrowAppearAnim = "ArrowOn";

	private const string ArrowDisappearAnim = "ArrowOff";

	private static string SpriteAnimResourcesPath = "BigJigger/SpriteAnimations";

	private List<AnimatedSprite> _animatedSprites;

	private List<Animation> _animatedModels;

	private JVPItemData _currentItemData = new JVPItemData();

	public JVPMouthClayObjects _MouthClay;

	public GameObject _CameraObject;

	public GameObject _HatBrimObject;

	public GameObject _HighlightObject;

	public ObjectAsButtonButton _NoseButton;

	public ObjectAsButtonButton _ShopButton;

	public JVPInfoBoard _InfoBoard;

	public GameObject _blackBoard;

	public GameObject _ShopText;

	public Material _CanAffordTextMaterial;

	public Material _CantAffordTextMaterial;

	public Material _CanAffordNumberMaterial;

	public Material _CantAffordNumberMaterial;

	public Material _PurchasedTextMaterial;

	public Material _PurchasedNumberMaterial;

	public GameObject _makeArrowObject;

	public bool InResults;

	public bool collectingDone = true;

	public GameObject _NewCreatureObject;

	public GameObject _NewUnlockObject;

	public GameObject _NewUpgrade;

	public GameObject _NewPowerPlay;

	private Animation _blackBoardAnimComponent;

	public string[] _materialNamesForHSVShift;

	public string[] _featureMaterialNamesForHSVShift;

	private List<Material> _materialsForHSVShift;

	private List<Material> _featureMaterialsForHSVShift;

	public GameObject NumberSpeechBubbleFrontend;

	public TextMesh NumberSpeechBubbleFrontendNumber;

	public GameObject NumberSpeechBubbleInGame;

	public TextMesh NumberSpeechBubbleInGameNumber;

	private State _currentState;

	private Queue<string> _animQueue = new Queue<string>(16);

	private string _currentAnim;

	public Camera _ResultsCamera;

	public Camera _ResultsCameraStartPosition;

	public Camera _ResultsCameraAssessmentPosition;

	public Camera _ResultsCameraReadyPosition;

	public Camera _ResultsCameraShopPosition;

	private ResultsCameraState _currentCameraState;

	private Camera _targetCamera;

	private float _targetCameraTransitionTime;

	private bool _triedToBuyWithoutEnoughClay;

	public ParticleEmitter[] _CollectingClayParticles;

	private bool _makeArrowVisible = true;

	private bool _flownAway;

	public static JVPController Instance { get; private set; }

	public List<string> AvailableAnimations { get; private set; }

	public JVPItemData CurrentItemData
	{
		get
		{
			return _currentItemData;
		}
	}

	public int NumAffordableItems
	{
		get
		{
			if (BuildDetails.Instance._DemoMode)
			{
				return 0;
			}
			if (HatBrim == null)
			{
				return 0;
			}
			return HatBrim.GetNumAffordableItems();
		}
	}

	public HatBrim HatBrim { get; private set; }

	public List<AnimatedSprite> SpriteAnimations
	{
		get
		{
			return _animatedSprites;
		}
	}

	private bool StoryPlaying { get; set; }

	public State CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			bool shopButtonAvailable = ShopButtonAvailable;
			_currentState = value;
			if (JVPController.StateChangeEvent != null)
			{
				JVPController.StateChangeEvent(_currentState);
			}
			if (_currentState == State.ZoomingIn && JVPController.JVPBecameActiveEvent != null)
			{
				JVPController.JVPBecameActiveEvent();
			}
			if (JVPController.JVPBecameInactiveEvent != null)
			{
				if (InInGameMode)
				{
					if (_currentState == State.ZoomedOut || _currentState == State.Inactive)
					{
						JVPController.JVPBecameInactiveEvent();
					}
				}
				else if (_currentState == State.ZoomingOut || _currentState == State.Inactive)
				{
					JVPController.JVPBecameInactiveEvent();
				}
			}
			bool shopButtonAvailable2 = ShopButtonAvailable;
			if (shopButtonAvailable && !shopButtonAvailable2 && JVPController.ShopButtonHiddenEvent != null)
			{
				JVPController.ShopButtonHiddenEvent();
			}
			if (!shopButtonAvailable && shopButtonAvailable2 && JVPController.ShopButtonShownEvent != null)
			{
				JVPController.ShopButtonShownEvent();
			}
		}
	}

	public bool IsActive
	{
		get
		{
			return CurrentState == State.Interactive;
		}
	}

	public bool ZoomedIn
	{
		get
		{
			return CurrentState == State.Interactive || CurrentState == State.Chewing;
		}
	}

	public bool ZoomedOut
	{
		get
		{
			return CurrentState == State.ZoomedOut;
		}
	}

	private bool InInGameMode
	{
		get
		{
			return MetaGameController.Instance.CurrentState == MetaGameController.State.InGame || MetaGameController.Instance.CurrentState == MetaGameController.State.LoadingInGame;
		}
	}

	private bool MouthOpen { get; set; }

	private bool ShopButtonAvailable
	{
		get
		{
			if (BuildDetails.Instance._DemoMode)
			{
				return false;
			}
			if (_currentState != State.ZoomedOut)
			{
				return false;
			}
			if (InInGameMode)
			{
				return _currentCameraState == ResultsCameraState.Results_ReadyToShop;
			}
			return StoryController.Instance.ReadyToRunFrontend && SaveData.Instance.Progress._FinishedOneLevel.Set;
		}
	}

	[method: MethodImpl(32)]
	public static event Action<State> StateChangeEvent;

	[method: MethodImpl(32)]
	public static event Action ShopButtonShownEvent;

	[method: MethodImpl(32)]
	public static event Action ShopButtonHiddenEvent;

	[method: MethodImpl(32)]
	public static event Action JVPBecameActiveEvent;

	[method: MethodImpl(32)]
	public static event Action JVPBecameInactiveEvent;

	[method: MethodImpl(32)]
	public static event Action GoodHighlightEvent;

	[method: MethodImpl(32)]
	public static event Action BadHighlightEvent;

	[method: MethodImpl(32)]
	public static event Action SuccessfulPurchaseEvent;

	[method: MethodImpl(32)]
	public static event Action<HatBrimItem> SuccessfulPurchaseItemEvent;

	protected override void OnAwake()
	{
		if (Instance != null)
		{
			Debug.LogError("Second instance of JVPController", base.gameObject);
		}
		Instance = this;
		JVPController.StateChangeEvent = (Action<State>)Delegate.Combine(JVPController.StateChangeEvent, new Action<State>(OnStateChange));
		HatBrim.NewHighlightEvent += OnNewHatBrimHighlight;
		HatBrim.NewHighlightChosenEvent += OnNewHatBrimItemChosen;
		NGUIPanelManager.PanelActivated += OnPanelActivated;
		StoryController.StoryStartedEvent += OnStoryStarted;
		StoryController.StoryFinishedEvent += OnStoryFinished;
		StoryController.StoryCompleteStartedEvent += OnStoryStarted;
		StoryController.StoryCompleteFinishedEvent += OnStoryFinished;
		FrontendCameraDirector.CameraReadyEvent += OnCameraReady;
		FrontendWorldController.NewHillSelectedEvent += OnHillChange;
		GetMaterialsForHSVShifting();
		SetColourToMatchHill();
		SetResultsCameraState(ResultsCameraState.Off);
		base.OnAwake();
	}

	private void OnDestroy()
	{
		JVPController.StateChangeEvent = (Action<State>)Delegate.Remove(JVPController.StateChangeEvent, new Action<State>(OnStateChange));
		HatBrim.NewHighlightEvent -= OnNewHatBrimHighlight;
		HatBrim.NewHighlightChosenEvent -= OnNewHatBrimItemChosen;
		NGUIPanelManager.PanelActivated -= OnPanelActivated;
		StoryController.StoryStartedEvent -= OnStoryStarted;
		StoryController.StoryFinishedEvent -= OnStoryFinished;
		StoryController.StoryCompleteStartedEvent -= OnStoryStarted;
		StoryController.StoryCompleteFinishedEvent -= OnStoryFinished;
		FrontendCameraDirector.CameraReadyEvent -= OnCameraReady;
		FrontendWorldController.NewHillSelectedEvent -= OnHillChange;
		Instance = null;
	}

	protected override bool DoInitialise()
	{
		LoadSpriteAnimations();
		GetModelReferences();
		CreateButtons();
		InitialiseMouthClay();
		LoadHatBrim();
		return true;
	}

	public override void ResetForRun()
	{
		MouthOpen = true;
		CloseMouth();
		ZoomOut();
	}

	private void GetMaterialsForHSVShifting()
	{
		_materialsForHSVShift = new List<Material>(_materialNamesForHSVShift.Length);
		_featureMaterialsForHSVShift = new List<Material>(_featureMaterialNamesForHSVShift.Length);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			for (int j = 0; j < _materialNamesForHSVShift.Length; j++)
			{
				Material material = componentsInChildren[i].material;
				if (material.name.Contains(_materialNamesForHSVShift[j]))
				{
					_materialsForHSVShift.Add(material);
				}
				else if (j < _featureMaterialNamesForHSVShift.Length && material.name.Contains(_featureMaterialNamesForHSVShift[j]))
				{
					_featureMaterialsForHSVShift.Add(material);
				}
			}
		}
	}

	private void SetColourToMatchHill()
	{
		HSVColour jVPColour = CurrentHill.Instance.Definition._JVPColour;
		HSVColour jVPFeatureColour = CurrentHill.Instance.Definition._JVPFeatureColour;
		for (int i = 0; i < _materialsForHSVShift.Count; i++)
		{
			jVPColour.UseOnHSVFastMaterial(_materialsForHSVShift[i]);
		}
		for (int j = 0; j < _featureMaterialNamesForHSVShift.Length; j++)
		{
			jVPFeatureColour.UseOnHSVFastMaterial(_featureMaterialsForHSVShift[j]);
		}
	}

	private void Update()
	{
		if (!StoryPlaying)
		{
			UpdateResultsCamera();
			UpdateStateAndAnims();
			CheckButtonPresses();
			SetColourToMatchHill();
		}
	}

	private bool CurrentAnimFinished()
	{
		bool result = true;
		foreach (AnimatedSprite animatedSprite in _animatedSprites)
		{
			if (animatedSprite.HasAnim(_currentAnim) && !animatedSprite.Finished)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private bool CurrentModelAnimFinished()
	{
		bool result = true;
		foreach (Animation animatedModel in _animatedModels)
		{
			if (animatedModel.isPlaying)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void UpdateStateAndAnims()
	{
		bool flag = CurrentAnimFinished();
		if (flag)
		{
			bool flag2 = PlayQueuedAnim();
			flag = !flag2;
		}
		if (CurrentState == State.Chewing && flag)
		{
			CurrentState = State.Interactive;
			if (_triedToBuyWithoutEnoughClay && BuildDetails.Instance._HasIAP)
			{
				GameObject target = ((!InInGameMode) ? FrontendNGUI.Instance._IAPPanel : InGameNGUI.Instance._IAPPanel);
				StaticIAPItems._forcedUserToStore = true;
				UIEvents.SendEvent(UIEventType.ResetToPanel, target);
			}
		}
		if (CurrentState == State.ZoomingIn || CurrentState == State.MenuAnimating)
		{
			flag = flag && CurrentModelAnimFinished();
		}
		if (flag)
		{
			if (CurrentState == State.MenuAnimating)
			{
				CurrentState = State.Interactive;
			}
			PlayIdleAnimation();
		}
	}

	private void PlayIdleAnimation()
	{
		if (ZoomedOut || _currentState == State.ZoomingOut)
		{
			if (_currentCameraState == ResultsCameraState.Results_CountingClay)
			{
				if (ClayCounterGUIController.IsCounting)
				{
					ForcePlayAnim("RecieveClay");
					InGameAudio.PostFabricEvent("JVPVeryHappy");
				}
				else
				{
					ForcePlayAnim("Happy");
				}
			}
			else if (_currentCameraState == ResultsCameraState.Results_ReadyToShop)
			{
				ForcePlayAnim("Happy");
			}
			else
			{
				int num = UnityEngine.Random.Range(0, 4);
				num++;
				ForcePlayAnim(string.Format("{0}{1}{2}", "Idle", num, "Off"));
			}
		}
		else if (MouthOpen)
		{
			int num2 = UnityEngine.Random.Range(0, 2);
			num2++;
			ForcePlayAnim(string.Format("{0}{1}", "OpenIdle", num2));
		}
		else
		{
			int num3 = UnityEngine.Random.Range(0, 4);
			num3++;
			ForcePlayAnim(string.Format("{0}{1}", "Idle", num3));
		}
	}

	public void Deactivate()
	{
		CurrentState = State.Inactive;
	}

	public void ForcePlayAnim(string animName)
	{
		_animQueue.Clear();
		PlayAnim(animName);
	}

	private void PlayAnim(string animName)
	{
		PlaySpriteAnimation(animName);
		PlayModelAnimation(animName);
		if (ZoomedIn || CurrentState == State.BuyingHillItem)
		{
			PlayAnimAudio(animName);
		}
		_currentAnim = animName;
	}

	private void PlayAnimAudio(string animName)
	{
		string postedEventName = string.Format("JVP{0}", animName);
		InGameAudio.PostFabricEvent(postedEventName);
	}

	public bool PlayQueuedAnim()
	{
		if (_animQueue.Count == 0)
		{
			return false;
		}
		string animName = _animQueue.Dequeue();
		PlayAnim(animName);
		return true;
	}

	public void ZoomIn()
	{
		DisplayHat(true);
		HideNumberSpeechBubbles();
		CurrentState = State.ZoomingIn;
	}

	public void SetActive()
	{
		ZoomIn();
		CurrentState = State.MenuAnimating;
	}

	private void ZoomOut()
	{
		PlayMainAnim();
		ForcePlayAnim("HatIn");
		if (!InInGameMode && CurrentState != 0 && CurrentState != State.ZoomedOut && CurrentState != State.ZoomingOut)
		{
			CurrentState = State.ZoomingOut;
		}
		else
		{
			CurrentState = State.ZoomedOut;
		}
		_currentItemData.ClearData();
		RefreshItemData();
		_NoseButton.ShouldPlayIdleAnim = false;
		_NoseButton.Activate(false);
		if (ShopButtonAvailable)
		{
			ShowNumberSpeechBubble();
		}
		else
		{
			HideNumberSpeechBubbles();
		}
	}

	private void DisplayHat(bool display)
	{
		HatBrim.DisplayHat(display);
	}

	private void DisplayShopElements(bool display)
	{
		_MouthClay._MainObject.SetActiveRecursively(display);
		_HighlightObject.SetActiveRecursively(display);
		_blackBoard.SetActiveRecursively(display);
		_InfoBoard._Text.gameObject.active = display;
		_NoseButton.ShouldPlayIdleAnim = display;
		_NoseButton.Activate(display);
	}

	private void ResetClaySize()
	{
		if (_currentItemData.Item == null || _currentItemData.Item.LockState != LockState.Unlocked)
		{
			CloseMouth();
		}
		else
		{
			OpenMouth();
		}
	}

	private void PlaySpriteAnimation(string animName)
	{
		foreach (AnimatedSprite animatedSprite in _animatedSprites)
		{
			if (animatedSprite.HasAnim(animName))
			{
				animatedSprite.ForcePlay(animName);
			}
		}
	}

	private void PlayModelAnimation(string animName)
	{
		foreach (Animation animatedModel in _animatedModels)
		{
			if (animatedModel[animName] != null)
			{
				animatedModel.Stop();
				animatedModel.Play(animName);
			}
		}
	}

	private void CreateButtons()
	{
		FrontendController.MakeGameObjectInteractive(_HatBrimObject, 14);
		_NoseButton.Initialise(14);
		if (!BuildDetails.Instance._DemoMode)
		{
			_ShopButton.Initialise(14);
			_ShopButton.ShouldPlayIdleAnim = true;
		}
	}

	private void GetModelReferences()
	{
		GetModelAnimations();
	}

	private void GetModelAnimations()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		_animatedModels = new List<Animation>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			Animation component = transform.GetComponent<Animation>();
			if ((bool)component && component.GetClipCount() > 0)
			{
				_animatedModels.Add(component);
			}
		}
	}

	private void LoadSpriteAnimations()
	{
		InstantiateSpriteAnimationPrefabs();
		SetMaterials();
		SetAvailableAnimations();
	}

	private void LoadHatBrim()
	{
		HatBrim = _HatBrimObject.AddComponent<HatBrim>();
	}

	private void InstantiateSpriteAnimationPrefabs()
	{
		GameObject gameObject = CreateContainer("SpriteAnims");
		UnityEngine.Object[] array = Resources.LoadAll(SpriteAnimResourcesPath, typeof(GameObject));
		_animatedSprites = new List<AnimatedSprite>(array.Length);
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object original in array2)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(original, base.transform.position, base.transform.rotation) as GameObject;
			gameObject2.transform.parent = gameObject.transform;
			AnimatedSprite component = gameObject2.GetComponent<AnimatedSprite>();
			_animatedSprites.Add(component);
			StripObjectName(gameObject2, "SpriteAnim(Clone)");
		}
	}

	private void SetMaterials()
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		Renderer childRenderer;
		for (int i = 0; i < array.Length; i++)
		{
			childRenderer = array[i];
			AnimatedSprite animatedSprite = _animatedSprites.Find((AnimatedSprite s) => s.name == childRenderer.name);
			if ((bool)animatedSprite)
			{
				animatedSprite.Material = childRenderer.material;
			}
		}
	}

	private void SetAvailableAnimations()
	{
		AvailableAnimations = new List<string>(16);
		foreach (AnimatedSprite animatedSprite in _animatedSprites)
		{
			foreach (SpriteAnimationData anim in animatedSprite._Anims)
			{
				if (!AvailableAnimations.Contains(anim._Name))
				{
					AvailableAnimations.Add(anim._Name);
				}
			}
		}
	}

	private GameObject CreateContainer(string name)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = base.transform;
		gameObject.name = name;
		return gameObject;
	}

	private void StripObjectName(GameObject gameObj, string toStrip)
	{
		string text = gameObj.name;
		int num = text.IndexOf(toStrip);
		if (num >= 0)
		{
			gameObj.name = text.Remove(num);
		}
	}

	private void ChooseColour(HSVColour colour)
	{
		RefreshItemData();
	}

	private void RefreshItemColour()
	{
		HSVColour hSVColour = _currentItemData.Colour;
		if (_currentItemData.Item == null || !_currentItemData.Item.CanAfford)
		{
			hSVColour = HSVColour.NoShift;
		}
		_MouthClay.UseColour(hSVColour);
		hSVColour.UseOnHSVMaterial(_HighlightObject.renderer.material);
	}

	private void CheckButtonPresses()
	{
		if (CurrentState != State.Interactive && CurrentState != State.Chewing && CurrentState != State.ZoomedOut)
		{
			return;
		}
		string mouseOverButton = "none";
		Vector3 mouseHitPoint = Vector3.zero;
		Ray ray;
		if (InInGameMode)
		{
			if (!_ResultsCamera.enabled)
			{
				return;
			}
			ray = _ResultsCamera.ScreenPointToRay(ClayJamInput.CursorScreenPosition);
		}
		else
		{
			ray = Camera.main.ScreenPointToRay(ClayJamInput.CursorScreenPosition);
		}
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, 100f, 16384))
		{
			mouseOverButton = hitInfo.collider.name;
			mouseHitPoint = hitInfo.point;
		}
		if (CurrentState == State.ZoomedOut)
		{
			CheckButtonPressesZoomedOut(mouseOverButton);
		}
		else if (CurrentState == State.Interactive || CurrentState == State.Chewing)
		{
			CheckButtonPressesZoomedIn(mouseOverButton, mouseHitPoint);
		}
	}

	private void CheckButtonPressesZoomedOut(string mouseOverButton)
	{
		if (ShopButtonAvailable && _ShopButton.UpdateState(mouseOverButton))
		{
			_ShopButton.PopOut();
			if (InInGameMode)
			{
				InGameController.Instance.GoToShop();
			}
			else
			{
				ZoomIn();
			}
		}
	}

	private void CheckButtonPressesZoomedIn(string mouseOverButton, Vector3 mouseHitPoint)
	{
		bool mouseOverSpinner = mouseOverButton == _HatBrimObject.name;
		HatBrim.UpdateSpinInput(mouseOverSpinner, mouseHitPoint);
		if (HatBrim.ReadyToChoose && _NoseButton.UpdateState(mouseOverButton))
		{
			_NoseButton.PopOut();
			TryToBuyHighlightItem();
		}
	}

	private void TryToBuyHighlightItem()
	{
		bool flag = false;
		bool transitionToHill = false;
		_triedToBuyWithoutEnoughClay = false;
		if (_currentItemData.Item.LockState == LockState.Unlocked && !_currentItemData.Item.CanAfford)
		{
			_triedToBuyWithoutEnoughClay = true;
		}
		if (_currentItemData.CanPurchase)
		{
			switch (_currentItemData.Type)
			{
			case HatBrimItem.ItemType.Cast:
				flag = SaveData.Instance.Casts.PurchaseCast(_currentItemData.CastData.Id);
				break;
			case HatBrimItem.ItemType.Hill:
				flag = CurrentHill.Instance.Upgrade();
				transitionToHill = false;
				break;
			case HatBrimItem.ItemType.PowerPlay:
				flag = CurrentHill.Instance.ProgressData.PurchasePowerplays(_currentItemData.Item.PowerPlayPackIndex);
				break;
			}
		}
		if (flag)
		{
			SucceedToBuyHighlightItem(transitionToHill);
			HatBrim.StopSpinning();
		}
		else
		{
			FailToBuyHighlightItem();
		}
	}

	private void SucceedToBuyHighlightItem(bool transitionToHill)
	{
		ForcePlayAnim("NoseSuccess");
		_animQueue.Enqueue("VeryHappy");
		InGameAudio.PostFabricEvent("JVPNoseSuccess");
		MouthOpen = false;
		CurrentState = State.Chewing;
		SetSpatOutItem();
		CurrentHill.Instance.ProgressData._LastSeenNumberAffordableItems = NumAffordableItems;
		SaveData.Instance.Progress._HavePressedGoodJVPItem.Set = true;
		SaveData.Instance.MarkAsNeedToSave(true);
		SaveData.Instance.Save();
		if (transitionToHill)
		{
			CurrentState = State.BuyingHillItem;
		}
		if (JVPController.SuccessfulPurchaseEvent != null)
		{
			JVPController.SuccessfulPurchaseEvent();
		}
		if (JVPController.SuccessfulPurchaseItemEvent != null)
		{
			JVPController.SuccessfulPurchaseItemEvent(_currentItemData.Item);
		}
		RefreshItemData();
	}

	private void FailToBuyHighlightItem()
	{
		if (MouthOpen)
		{
			CloseMouth();
			_animQueue.Enqueue("NoseFail");
		}
		else
		{
			ForcePlayAnim("NoseFail");
		}
		InGameAudio.PostFabricEvent("JVPNoseFail");
		CurrentState = State.Chewing;
	}

	private void SetSpatOutItem()
	{
		HSVColour colour = _currentItemData.Colour;
		colour.UseOnHSVFastMaterial(_NewCreatureObject.renderer.material);
		_NewCreatureObject.renderer.material.mainTexture = _currentItemData.Item.Texture;
		TurnOffAllSpatOutItems();
		switch (_currentItemData.Item.Type)
		{
		case HatBrimItem.ItemType.Cast:
			_NewCreatureObject.GetComponent<Renderer>().enabled = true;
			break;
		case HatBrimItem.ItemType.Hill:
			if (_currentItemData.Item.UpgradeLevel < 1)
			{
				colour.UseOnHSVFastMaterial(_NewUnlockObject.renderer.material);
				_NewUnlockObject.GetComponent<Renderer>().enabled = true;
			}
			else
			{
				colour.UseOnHSVFastMaterial(_NewUpgrade.renderer.material);
				_NewUpgrade.GetComponent<Renderer>().enabled = true;
			}
			break;
		case HatBrimItem.ItemType.PowerPlay:
			_NewCreatureObject.GetComponent<Renderer>().enabled = true;
			CurrentHill.Instance.Definition._PowerPlayColour.UseOnHSVFastMaterial(_NewCreatureObject.renderer.material);
			break;
		}
	}

	private void OnNewHatBrimHighlight(HatBrimItem newHighlight)
	{
		_currentItemData.Item = newHighlight;
		RefreshItemData();
		if (IsActive && _currentItemData.CanPurchase)
		{
			if (JVPController.GoodHighlightEvent != null)
			{
				JVPController.GoodHighlightEvent();
			}
			if (!SaveData.Instance.Progress._HaveSeenGoodJVPItem.Set)
			{
				SaveData.Instance.Progress._HaveSeenGoodJVPItem.Set = true;
			}
		}
		else if (JVPController.BadHighlightEvent != null)
		{
			JVPController.BadHighlightEvent();
		}
	}

	private void RefreshItemData()
	{
		if (CurrentState == State.ZoomedOut && CurrentState != State.ZoomingOut)
		{
			_currentItemData.ClearData();
		}
		RefreshItemColour();
		if (CurrentState != State.Chewing && CurrentState != State.BuyingHillItem)
		{
			ResetClaySize();
		}
		_InfoBoard.SetInfo(_currentItemData);
		if (_currentItemData == null || _currentItemData.Item == null)
		{
			return;
		}
		if (_currentItemData.CanPurchase)
		{
			if (!_makeArrowVisible)
			{
				_makeArrowObject.animation.Play("ArrowOn");
			}
			_makeArrowVisible = true;
			_NoseButton.ShouldPlayIdleAnim = true;
		}
		else
		{
			if (_makeArrowVisible)
			{
				_makeArrowObject.animation.Play("ArrowOff");
			}
			_makeArrowVisible = false;
			_NoseButton.ShouldPlayIdleAnim = false;
		}
	}

	private void OnNewHatBrimItemChosen()
	{
		if (_currentItemData.Item != null && CurrentState == State.Interactive)
		{
			if (_currentItemData.CanPurchase)
			{
				InGameAudio.PostFabricEvent("ButtonBecomesActive");
			}
			else if (_currentItemData.Item.LockState == LockState.Unlocked)
			{
				InGameAudio.PostFabricEvent("JVPNotEnoughClay");
			}
		}
	}

	private void OnPanelActivated(GameObject panel)
	{
		if (panel.name == "MapScreenPanel")
		{
			if (!ZoomedOut && CurrentState != State.ZoomingOut)
			{
				ZoomOut();
			}
		}
		else if (panel.name == "IAPPanel")
		{
			Deactivate();
		}
		else if (panel.name == "JVPPanel")
		{
			KillCollectingClayParticles();
			if (InInGameMode)
			{
				SetActive();
			}
		}
		else
		{
			Deactivate();
		}
	}

	private void InitialiseMouthClay()
	{
		_MouthClay.Initialise();
	}

	private void CloseMouth()
	{
		if (MouthOpen)
		{
			ForcePlayAnim("MouthClose");
			MouthOpen = false;
			_MouthClay.Close();
		}
	}

	private void OpenMouth()
	{
		if (!MouthOpen)
		{
			ForcePlayAnim("MouthOpen");
			MouthOpen = true;
		}
		_MouthClay.Open(_currentItemData.Item.Cost);
	}

	private void OnStateChange(State state)
	{
		switch (CurrentState)
		{
		case State.ZoomingIn:
			_InfoBoard._Text.Activate(false);
			DisplayShopElements(true);
			ForcePlayAnim("HatOut");
			InGameAudio.PostFabricEvent("JVPHatOut");
			break;
		case State.MenuAnimating:
			base.animation.Stop();
			RefreshItemData();
			break;
		case State.Interactive:
			_InfoBoard._Text.Activate(true);
			_currentItemData.ForceReCheck();
			_makeArrowVisible = true;
			OnNewHatBrimHighlight(_currentItemData.Item);
			CurrentHill.Instance.ProgressData._LastSeenNumberAffordableItems = NumAffordableItems;
			break;
		case State.ZoomedOut:
			HatBrim.DisplayHat(false);
			break;
		case State.ZoomingOut:
			DisplayShopElements(false);
			break;
		case State.Chewing:
			break;
		}
	}

	private void OnCameraReady(FrontendCameraVO cameraVO)
	{
		switch (CurrentState)
		{
		case State.ZoomingIn:
			CurrentState = State.MenuAnimating;
			break;
		case State.ZoomingOut:
			CurrentState = State.ZoomedOut;
			break;
		case State.BuyingHillItem:
			if (cameraVO._CameraName == CameraNames.Frontend.JVP)
			{
				CurrentState = State.Interactive;
			}
			break;
		case State.MenuAnimating:
		case State.Interactive:
		case State.Chewing:
			break;
		}
	}

	private void OnStoryStarted()
	{
		StoryPlaying = true;
		TurnOffAllSpatOutItems();
	}

	private void OnStoryFinished()
	{
		StoryPlaying = false;
		PlayMainAnim(true);
		TurnOffAllSpatOutItems();
	}

	private void TurnOffAllSpatOutItems()
	{
		_NewCreatureObject.GetComponent<Renderer>().enabled = false;
		_NewUnlockObject.GetComponent<Renderer>().enabled = false;
		_NewUpgrade.GetComponent<Renderer>().enabled = false;
		_NewPowerPlay.GetComponent<Renderer>().enabled = false;
	}

	public void ZoomInFromResultsScreen()
	{
		_ResultsCamera.enabled = true;
		SetResultsCameraState(ResultsCameraState.Results_InShop);
		SetActive();
	}

	public void ZoomOutToResultsScreen()
	{
		_ResultsCamera.enabled = true;
		SetResultsCameraState(ResultsCameraState.Results_ReadyToShop);
		ZoomOut();
	}

	public void PositionJustOffResultsScreen()
	{
		base.gameObject.SetActiveRecursively(true);
		SetResultsCameraState(ResultsCameraState.Results_Offscren);
		TurnOffAllSpatOutItems();
		ZoomOut();
	}

	public void PositionForClayCounting()
	{
		_ResultsCamera.enabled = true;
		SetResultsCameraState(ResultsCameraState.Results_CountingClay);
		ZoomOut();
	}

	public void PositionReadyOnResultsScreen()
	{
		_ResultsCamera.enabled = true;
		SetResultsCameraState(ResultsCameraState.Results_ReadyToShop);
		ZoomOut();
	}

	public void HideFromResultsScreen()
	{
		SetResultsCameraState(ResultsCameraState.Off);
		_ResultsCamera.enabled = false;
		base.gameObject.SetActiveRecursively(false);
	}

	public void HideFromFrontend()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	public void ShowForFrontend()
	{
		base.gameObject.SetActiveRecursively(true);
	}

	public void UpdateResultsCamera()
	{
		if (_targetCamera != null)
		{
			Vector3 position = _targetCamera.transform.position;
			Vector3 position2 = _ResultsCamera.transform.position;
			float orthographicSize = _targetCamera.orthographicSize;
			float orthographicSize2 = _ResultsCamera.orthographicSize;
			float num = _targetCameraTransitionTime - Time.time;
			float value = ((!(num > 0f)) ? 1f : (Time.deltaTime / num));
			value = Mathf.Clamp01(value);
			value = Mathf.Sin((float)Math.PI / 2f * value);
			_ResultsCamera.transform.position = Vector3.Lerp(position2, position, value);
			_ResultsCamera.orthographicSize = Mathf.Lerp(orthographicSize2, orthographicSize, value);
			if (value == 1f)
			{
				_targetCamera = null;
			}
		}
	}

	public void EnableCollectingClayParticles(bool on)
	{
		for (int i = 0; i < _CollectingClayParticles.Length; i++)
		{
			_CollectingClayParticles[i].gameObject.SetActiveRecursively(on);
			_CollectingClayParticles[i].emit = on;
		}
	}

	public void KillCollectingClayParticles()
	{
		for (int i = 0; i < _CollectingClayParticles.Length; i++)
		{
			_CollectingClayParticles[i].ClearParticles();
			_CollectingClayParticles[i].emit = false;
			_CollectingClayParticles[i].gameObject.SetActiveRecursively(false);
		}
	}

	public void ChangeStateToZoomedOut()
	{
		CurrentState = State.ZoomedOut;
	}

	private void SetResultsCameraState(ResultsCameraState newCameraState)
	{
		bool shopButtonAvailable = ShopButtonAvailable;
		_currentCameraState = newCameraState;
		bool shopButtonAvailable2 = ShopButtonAvailable;
		if (shopButtonAvailable && !shopButtonAvailable2 && JVPController.ShopButtonHiddenEvent != null)
		{
			JVPController.ShopButtonHiddenEvent();
		}
		if (!shopButtonAvailable && shopButtonAvailable2 && JVPController.ShopButtonShownEvent != null)
		{
			JVPController.ShopButtonShownEvent();
		}
		switch (_currentCameraState)
		{
		case ResultsCameraState.Off:
			_targetCamera = null;
			break;
		case ResultsCameraState.Results_Offscren:
			_targetCamera = _ResultsCameraStartPosition;
			_targetCameraTransitionTime = Time.time + 0.5f;
			break;
		case ResultsCameraState.Results_CountingClay:
			_targetCamera = _ResultsCameraAssessmentPosition;
			_targetCameraTransitionTime = Time.time + 1f;
			break;
		case ResultsCameraState.Results_ReadyToShop:
			_targetCamera = _ResultsCameraReadyPosition;
			_targetCameraTransitionTime = Time.time + 0.5f;
			break;
		case ResultsCameraState.Results_InShop:
			_targetCamera = _ResultsCameraShopPosition;
			_targetCameraTransitionTime = Time.time + 1f;
			break;
		}
	}

	private void ShowNumberSpeechBubble()
	{
		int numAffordableItems = NumAffordableItems;
		if (numAffordableItems == 0)
		{
			HideNumberSpeechBubbles();
		}
		else if (InInGameMode)
		{
			NumberSpeechBubbleFrontend.SetActiveRecursively(false);
			NumberSpeechBubbleInGame.SetActiveRecursively(true);
			NumberSpeechBubbleInGameNumber.text = numAffordableItems.ToString();
			NumberSpeechBubbleInGame.animation.Play("Bubble3TransIn");
		}
		else
		{
			NumberSpeechBubbleInGame.SetActiveRecursively(false);
			NumberSpeechBubbleFrontend.SetActiveRecursively(true);
			NumberSpeechBubbleFrontendNumber.text = numAffordableItems.ToString();
			NumberSpeechBubbleFrontend.animation.Play("Bubble2TransIn");
		}
	}

	private void HideNumberSpeechBubbles()
	{
		if (InInGameMode)
		{
			NumberSpeechBubbleFrontend.SetActiveRecursively(false);
			NumberSpeechBubbleInGame.SetActiveRecursively(true);
			NumberSpeechBubbleInGame.animation.Play("Bubble3TransOut");
		}
		else
		{
			NumberSpeechBubbleInGame.SetActiveRecursively(false);
			NumberSpeechBubbleFrontend.SetActiveRecursively(true);
			NumberSpeechBubbleFrontend.animation.Play("Bubble2TransOut");
		}
	}

	private void OnHillChange()
	{
		ShowNumberSpeechBubble();
		HatBrim.ResetWheel();
	}

	public void PlayMainAnim(bool justPlayedStory = false)
	{
		if (!SaveData.Instance.Progress._FinishedOneLevel.Set)
		{
			if (!_flownAway || justPlayedStory)
			{
				base.animation.Play("JVPTransOut");
				_flownAway = true;
			}
		}
		else if (StoryController.Instance == null || !StoryController.Instance.PlayingCompleteStory)
		{
			if (BuildDetails.Instance._UsingPCAssets)
			{
				base.animation.Play("JVPfloat_PC");
			}
			else
			{
				base.animation.Play("JVPfloat");
			}
		}
	}

	public void FreezeSpinner()
	{
		HatBrim.FreezeSpinner();
	}

	public void UnFreezeSpinner()
	{
		HatBrim.UnFreezeSpinner();
	}
}
