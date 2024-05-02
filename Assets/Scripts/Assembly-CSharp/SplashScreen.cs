using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(GUITexture))]
public class SplashScreen : MonoBehaviour
{
	[Serializable]
	public class ScreenData
	{
		public float _PauseBeforeFadeInTime = 0.1f;

		public float _FadeInTime = 0.5f;

		public float _HoldTime = 0.5f;

		public float _FadeOutTime = 0.5f;

		public bool _FadeIn;

		public bool _FadeOut;

		public Texture2D _Texture;
	}

	private enum State
	{
		PreFadeIn = 0,
		FadingIn = 1,
		Held = 2,
		FadingOut = 3
	}

	private Rect _drawArea;

	private GUITexture _guiTexture;

	public ScreenData[] _Screens;

	private int _screenCount = 1;

	private int _currentScreenIndex;

	private float _timeInState;

	private bool _canFinnish;

	private bool _finalScreenFadeOutStarted;

	private State _currentState;

	public static SplashScreen Instance { get; private set; }

	private ScreenData CurrentScreen
	{
		get
		{
			return _Screens[_currentScreenIndex];
		}
	}

	private State CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			_currentState = value;
			_timeInState = 0f;
		}
	}

	[method: MethodImpl(32)]
	public event Action FinalScreenFadeOutStarted;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one SplashScreen instance");
		}
		Instance = this;
		FrontendController.FrontendLoaded += OnGameLoaded;
		InitialiseTexture();
	}

	private void Destroy()
	{
		FrontendController.FrontendLoaded -= OnGameLoaded;
	}

	private void OnGameLoaded()
	{
		_canFinnish = true;
	}

	private void FitTextureToscreen()
	{
		float num = (float)Screen.height / (float)Screen.width;
		float num2 = (float)_guiTexture.texture.height / (float)_guiTexture.texture.width;
		float x = num / num2;
		base.transform.localScale = new Vector3(x, 1f, 1f);
	}

	private void Update()
	{
		_timeInState += Time.deltaTime;
		switch (CurrentState)
		{
		case State.PreFadeIn:
			UpdatePreFadeIn();
			break;
		case State.FadingIn:
			UpdateFadingIn();
			break;
		case State.Held:
			UpdateHeld();
			break;
		case State.FadingOut:
			UpdateFadingOut();
			break;
		}
	}

	private void UpdatePreFadeIn()
	{
		if (_timeInState > CurrentScreen._PauseBeforeFadeInTime)
		{
			if (CurrentScreen._FadeIn)
			{
				CurrentState = State.FadingIn;
				return;
			}
			CutIn();
			CurrentState = State.Held;
		}
	}

	private void CutIn()
	{
		Color color = _guiTexture.color;
		color.a = 1f;
		_guiTexture.color = color;
	}

	private void UpdateFadingIn()
	{
		FadeOut(CurrentScreen._FadeInTime * -1f);
		if (_guiTexture.color.a >= 1f)
		{
			CurrentState = State.Held;
		}
	}

	private void UpdateHeld()
	{
		if (!(_timeInState < CurrentScreen._HoldTime))
		{
			if (!IsLastScreen())
			{
				CurrentState = State.FadingOut;
			}
			else if (_canFinnish)
			{
				CurrentState = State.FadingOut;
			}
		}
	}

	private void UpdateFadingOut()
	{
		FadeOut(CurrentScreen._FadeOutTime);
		if (IsLastScreen())
		{
			if (!_finalScreenFadeOutStarted)
			{
				StartingFinalFadeOut();
			}
			if (_guiTexture.color.a == 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else if (_guiTexture.color.a == 0f)
		{
			MoveToNextScreen();
		}
	}

	private void FadeOut(float fadeTime)
	{
		Color color = _guiTexture.color;
		float num = Time.deltaTime / fadeTime;
		if (color.a < 0.25f)
		{
			num *= 0.5f;
		}
		else if (color.a > 0.75f)
		{
			num *= 2f;
		}
		color.a -= num;
		color.a = Mathf.Clamp01(color.a);
		_guiTexture.color = color;
	}

	private void InitialiseTexture()
	{
		_guiTexture = GetComponent<GUITexture>();
		if (_guiTexture.texture != null && !Array.Exists(_Screens, (ScreenData s) => s._Texture == _guiTexture.texture))
		{
			Debug.LogError("Splash Screen GUI Texture initialised with unused screen texture");
		}
		if (_Screens.Length == 0)
		{
			Debug.LogError("Splash Screen requires screens data");
			return;
		}
		_screenCount = _Screens.Length;
		UseScreen(0);
		FitTextureToscreen();
	}

	private void UseScreen(int screenIndex)
	{
		_currentScreenIndex = screenIndex;
		if (_guiTexture.texture != CurrentScreen._Texture)
		{
			_guiTexture.texture = CurrentScreen._Texture;
		}
	}

	private bool IsLastScreen()
	{
		return _currentScreenIndex >= _screenCount - 1;
	}

	private void MoveToNextScreen()
	{
		_currentScreenIndex++;
		UseScreen(_currentScreenIndex);
		CurrentState = State.PreFadeIn;
	}

	private void StartingFinalFadeOut()
	{
		if (this.FinalScreenFadeOutStarted != null)
		{
			this.FinalScreenFadeOutStarted();
		}
		_finalScreenFadeOutStarted = true;
	}
}
