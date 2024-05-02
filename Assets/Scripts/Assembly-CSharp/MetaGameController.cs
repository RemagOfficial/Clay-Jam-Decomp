using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MetaGameController : MonoBehaviour
{
	public enum State
	{
		LoadingGlobal = 0,
		LoadingFrontend = 1,
		Frontend = 2,
		WaitingForGameToTransitionOut = 3,
		WaitingForLoadScreen = 4,
		LoadingInGame = 5,
		InGame = 6
	}

	private AsyncOperation _sceneLoader;

	private bool _allGlobalScenesLoaded;

	private int _frameToAllowLoadingToShow;

	public string _Mike;

	public static MetaGameController Instance { get; private set; }

	public static GameObject LoadingScreenCamera { get; set; }

	public bool LoadingFromBootup { get; private set; }

	public bool LoadingScreenHasTips { get; private set; }

	public State CurrentState { get; private set; }

	[method: MethodImpl(32)]
	public static event Action GameLoadedEvent;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("MetagameController created twice. Here.", base.gameObject);
			Debug.LogError("and here", Instance.gameObject);
		}
		Instance = this;
		FrontendController.PlayPressedEvent += GotoModeChoice;
		InGameController.PlayPressedEvent += GotoModeChoice;
		InGameController.ReturnToFrontendRequested += OnInGameReturnRequested;
		UIEvents.StartGame += OnStartGamePressed;
	}

	private void Start()
	{
		LoadGlobalAssets();
	}

	private void OnDestroy()
	{
		FrontendController.PlayPressedEvent -= GotoModeChoice;
		InGameController.PlayPressedEvent -= GotoModeChoice;
		InGameController.ReturnToFrontendRequested -= OnInGameReturnRequested;
		UIEvents.StartGame -= OnStartGamePressed;
	}

	private void Update()
	{
		switch (CurrentState)
		{
		case State.LoadingGlobal:
			UpdateLoadingGlobal();
			break;
		case State.LoadingFrontend:
			UpdateLoadingFrontend();
			break;
		case State.Frontend:
			break;
		case State.WaitingForGameToTransitionOut:
			UpdateWaitingForGameToTransitionOut();
			break;
		case State.WaitingForLoadScreen:
			UpdateWaitingForLoadScreen();
			break;
		case State.LoadingInGame:
			UpdateLoadingInGame();
			break;
		case State.InGame:
			break;
		}
	}

	private void SetState(State newState)
	{
		Debug.Log(string.Format("Meta State {0}", newState));
		CurrentState = newState;
	}

	private void UpdateLoadingFrontend()
	{
		if ((bool)FrontendController.Instance && FrontendController.Instance.Loaded)
		{
			SetState(State.Frontend);
			HideLoading();
		}
	}

	private void UpdateWaitingForGameToTransitionOut()
	{
		if (NGUIPanelManager.Instance.TopPanel == null)
		{
			GotoGame();
		}
	}

	private void UpdateWaitingForLoadScreen()
	{
		if (Time.frameCount >= _frameToAllowLoadingToShow)
		{
			SetState(State.LoadingInGame);
			FrontendController.Instance.CloseDown();
			InGameController.Instance.StartRun();
		}
	}

	private void UpdateLoadingInGame()
	{
		if ((bool)InGameController.Instance && InGameController.Instance.ReadyToShow)
		{
			SetState(State.InGame);
			HideLoading();
		}
	}

	private void OnStartGamePressed()
	{
		CurrentHill.Instance.ProgressData._NumPlays++;
		NGUIPanelManager.Instance.PopPanelFromStack(NGUIPanelManager.Instance._ModeChoicePanel);
		SetState(State.WaitingForGameToTransitionOut);
	}

	private void OnInGameReturnRequested()
	{
		GotoFrontend(false);
	}

	private void GotoGame()
	{
		ShowLoading(true);
		IAPProductRequester.Instance.RequestProducts();
		_frameToAllowLoadingToShow = Time.frameCount + 3;
		SetState(State.WaitingForLoadScreen);
	}

	private void GotoFrontend(bool loadingFromBootup)
	{
		LoadingFromBootup = loadingFromBootup;
		if (!loadingFromBootup)
		{
			MusicController.Instance.StartFrontend();
			ShowLoading(false);
		}
		if (CurrentState == State.InGame)
		{
			InGameController.Instance.CloseDown();
		}
		FrontendController.Instance.StartUp();
		SetState(State.LoadingFrontend);
	}

	private void LoadGlobalAssets()
	{
		SetState(State.LoadingGlobal);
		GougeRender.LoadMeshes();
		Localization.instance.Init();
		Application.targetFrameRate = 30;
		StartCoroutine(LoadAsync());
	}

	private IEnumerator LoadAsync()
	{
		int startFrame2 = Time.frameCount;
		_allGlobalScenesLoaded = false;
		Debug.Log(ToString() + " BuildDetails.Instance: " + (BuildDetails.Instance != null));
		string sceneName2 = BuildDetails.Instance.GlobalSceneName;
		_sceneLoader = Application.LoadLevelAdditiveAsync(sceneName2);
		yield return _sceneLoader;
		if (startFrame2 + 1 >= Time.frameCount)
		{
			yield return new WaitForEndOfFrame();
		}
		startFrame2 = Time.frameCount;
		sceneName2 = BuildDetails.Instance.JVPSceneName;
		_sceneLoader = Application.LoadLevelAdditiveAsync(sceneName2);
		yield return _sceneLoader;
		if (startFrame2 + 1 >= Time.frameCount)
		{
			yield return new WaitForEndOfFrame();
		}
		_allGlobalScenesLoaded = true;
	}

	private void UpdateLoadingGlobal()
	{
		if (_allGlobalScenesLoaded && JVPComponentManager.Instance.Initialise())
		{
			if (MetaGameController.GameLoadedEvent != null)
			{
				MetaGameController.GameLoadedEvent();
			}
			IAPProductRequester.Instance.RequestProducts();
			GotoFrontend(true);
		}
	}

	private void ShowLoading(bool withTips)
	{
		if (LoadingScreenCamera == null)
		{
			Debug.LogError("A Camera needs to declare itself as the LoadingScreen camera before now");
		}
		else
		{
			LoadingScreenCamera.camera.enabled = true;
		}
		LoadingScreenHasTips = withTips;
		NGUIPanelManager.Instance._LoadingPanel.SetActiveRecursively(true);
	}

	private void HideLoading()
	{
		SaveData.Instance.SaveIfNeeded();
		if (LoadingScreenCamera == null)
		{
			Debug.LogError("A Camera needs to declare itself as the LoadingScreen camera before now");
		}
		else
		{
			LoadingScreenCamera.camera.enabled = false;
		}
		NGUIPanelManager.Instance._LoadingPanel.SetActiveRecursively(false);
	}

	private void ReportMemory(string eventName)
	{
		long totalMemory = GC.GetTotalMemory(true);
		Debug.Log(string.Format("Mem Used going into state {0}: {1}", eventName, totalMemory));
	}

	private void GotoModeChoice()
	{
		if (BuildDetails.Instance._DemoMode)
		{
			SaveData.Instance.ClayCollected.Clear();
			NGUIPanelManager.Instance.PopPanelFromStack(NGUIPanelManager.Instance.TopPanel);
			SetState(State.WaitingForGameToTransitionOut);
		}
		else
		{
			NGUIPanelManager.Instance.ResetToPanel(NGUIPanelManager.Instance._ModeChoicePanel);
		}
	}
}
