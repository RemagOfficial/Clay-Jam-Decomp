using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FrontendController : MonoBehaviour
{
	private enum State
	{
		NotStarted = 0,
		Loading = 1,
		ResettingForRun = 2,
		Running = 3
	}

	private const float ResetForRunTime = 0.75f;

	public bool PlayRequested;

	private AsyncOperation _sceneLoader;

	private float _runTime;

	public string _Ciaran;

	public static FrontendController Instance { get; private set; }

	private State CurrentState { get; set; }

	public bool Loaded { get; private set; }

	[method: MethodImpl(32)]
	public static event Action PlayPressedEvent;

	[method: MethodImpl(32)]
	public static event Action FrontendLoaded;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("FrontendController created twice. Here.", base.gameObject);
			Debug.LogError("and here", Instance.gameObject);
		}
		Instance = this;
		FrontendWorldController.StartGamePressed += PressedPlay;
		CurrentState = State.NotStarted;
	}

	private void OnDestroy()
	{
		FrontendWorldController.StartGamePressed -= PressedPlay;
	}

	private void Update()
	{
		switch (CurrentState)
		{
		case State.NotStarted:
			break;
		case State.Loading:
			UpdateLoading();
			break;
		case State.ResettingForRun:
			UpdateResetting();
			break;
		case State.Running:
			UpdateRunning();
			break;
		}
	}

	public void CloseDown()
	{
		if ((bool)FrontendComponentManager.Instance)
		{
			FrontendComponentManager.Instance.Unload();
		}
		if ((bool)JVPComponentManager.Instance)
		{
			JVPComponentManager.Instance.CloseDown();
		}
		GC.Collect();
		Resources.UnloadUnusedAssets();
		CurrentState = State.NotStarted;
	}

	public void StartUp()
	{
		if ((bool)FrontendComponentManager.Instance)
		{
			ResetForRun();
		}
		else
		{
			StartLoading();
		}
	}

	private void StartLoading()
	{
		Loaded = false;
		CurrentState = State.Loading;
		StartCoroutine(LoadAsync());
	}

	private IEnumerator LoadAsync()
	{
		int startFrame = Time.frameCount;
		string sceneName = BuildDetails.Instance.FrontendSceneName;
		_sceneLoader = Application.LoadLevelAdditiveAsync(sceneName);
		yield return _sceneLoader;
		if (startFrame + 1 >= Time.frameCount)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	private void UpdateLoading()
	{
		if ((_sceneLoader == null || _sceneLoader.isDone) && !(FrontendComponentManager.Instance == null) && FrontendComponentManager.Instance.Initialise())
		{
			Loaded = true;
			if (FrontendController.FrontendLoaded != null)
			{
				FrontendController.FrontendLoaded();
			}
			WaitForSplashScreen();
		}
	}

	private void WaitForSplashScreen()
	{
		if (SplashScreen.Instance != null)
		{
			SplashScreen.Instance.FinalScreenFadeOutStarted += SplashScreenFinished;
		}
		else
		{
			ReadyToStartFrontend();
		}
	}

	private void SplashScreenFinished()
	{
		SplashScreen.Instance.FinalScreenFadeOutStarted -= SplashScreenFinished;
		ReadyToStartFrontend();
	}

	private void ReadyToStartFrontend()
	{
		ResetForRun();
	}

	private void UpdateRunning()
	{
		if (!PlayRequested)
		{
			return;
		}
		Tutorial tutorial = TutorialDatabase.Instance.Tutorials.Find((Tutorial t) => t.IsShowing);
		if (tutorial == null)
		{
			if (FrontendController.PlayPressedEvent != null)
			{
				FrontendController.PlayPressedEvent();
			}
			PlayRequested = false;
		}
	}

	public void ResetForRun()
	{
		FrontendComponentManager.Instance.ResetForRun();
		JVPComponentManager.Instance.ResetForRun();
		_runTime = Time.time + 0.75f;
		CurrentState = State.ResettingForRun;
	}

	private void UpdateResetting()
	{
		if (!(Time.time < _runTime) && StoryController.Instance.ReadyToRunFrontend)
		{
			Run();
		}
	}

	private void Run()
	{
		FrontendComponentManager.Instance.Run();
		JVPComponentManager.Instance.Run();
		CurrentState = State.Running;
	}

	public static void MakeGameObjectInteractive(GameObject gameObject, int layer)
	{
		Collider collider = gameObject.GetComponent<Collider>();
		if (collider == null)
		{
			MeshFilter component = gameObject.GetComponent<MeshFilter>();
			if (component == null)
			{
				Debug.LogError(string.Format("Can't make {0} interactive without a collider or mesh", gameObject.name, gameObject));
				return;
			}
			MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
			meshCollider.sharedMesh = component.mesh;
			collider = meshCollider;
		}
		collider.isTrigger = true;
		gameObject.layer = layer;
	}

	private void PressedPlay()
	{
		PlayRequested = true;
	}
}
