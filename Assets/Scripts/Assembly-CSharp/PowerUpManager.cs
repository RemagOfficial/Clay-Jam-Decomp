using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
	public GameObject _FlamePrefab;

	public GameObject _RingsPrefab;

	public GameObject _SplatFingerPrefab;

	private float DefaultSplatFingerTime;

	private float FlameCountDown;

	private float SquashCountDown;

	private float SplatFingerCountDown;

	private float FlameFallOffTime;

	private float AccelerationMultiplier;

	private float MaxSpeedMultiplier;

	private GameObject FlameInstance;

	private GameObject RingInstance;

	private float _squashFlashTimer;

	public bool Active { get; private set; }

	public bool FlameIsOn { get; private set; }

	public bool SquashIsOn { get; private set; }

	public bool HeavyIsOn
	{
		get
		{
			return FlameIsOn;
		}
	}

	public bool SuitIsOn { get; private set; }

	public bool SplatFingerIsOn { get; private set; }

	public float SquashSize { get; private set; }

	public float SquashVisuals { get; private set; }

	public float SuitSize { get; private set; }

	public float FlameTimeRemaining
	{
		get
		{
			return (!FlameIsOn) ? 0f : FlameCountDown;
		}
	}

	public float SquashTimeRemaining
	{
		get
		{
			return (!SquashIsOn) ? 0f : SquashCountDown;
		}
	}

	public float SplatTimeRemaining
	{
		get
		{
			return (!SplatFingerIsOn) ? 0f : SplatFingerCountDown;
		}
	}

	public bool FlameIsAvailable { get; set; }

	public bool BoostIsAvailable { get; set; }

	public bool ShrinkIsAvailable { get; set; }

	public bool SplatIsAvailable { get; set; }

	public int FlameMultiplier
	{
		get
		{
			if (HeavyIsOn)
			{
				if (PowerupDatabase.Instance.IsFlameUpgraded)
				{
					return 4;
				}
				return 2;
			}
			return 1;
		}
	}

	[method: MethodImpl(32)]
	public static event Action FlameOnHandler;

	[method: MethodImpl(32)]
	public static event Action SquashOnHandler;

	[method: MethodImpl(32)]
	public static event Action SuitUpHandler;

	[method: MethodImpl(32)]
	public static event Action SplatFingerHandler;

	public void ToggleFlameAvailable()
	{
		bool flag = !FlameIsAvailable;
		if (flag || BoostIsAvailable || ShrinkIsAvailable || SplatIsAvailable)
		{
			FlameIsAvailable = flag;
		}
		SaveData.Instance.Progress._powerplayPicker[0].Set = FlameIsAvailable;
		SaveData.Instance.Progress._hasUsedPowerPlayPicker.Set = true;
	}

	public void ToggleBoostAvailable()
	{
		bool flag = !BoostIsAvailable;
		if (flag || FlameIsAvailable || ShrinkIsAvailable || SplatIsAvailable)
		{
			BoostIsAvailable = flag;
		}
		SaveData.Instance.Progress._powerplayPicker[1].Set = BoostIsAvailable;
		SaveData.Instance.Progress._hasUsedPowerPlayPicker.Set = true;
	}

	public void ToggleShrinkAvailable()
	{
		bool flag = !ShrinkIsAvailable;
		if (flag || BoostIsAvailable || FlameIsAvailable || SplatIsAvailable)
		{
			ShrinkIsAvailable = flag;
		}
		SaveData.Instance.Progress._powerplayPicker[2].Set = ShrinkIsAvailable;
		SaveData.Instance.Progress._hasUsedPowerPlayPicker.Set = true;
	}

	public void ToggleSplatAvailable()
	{
		bool flag = !SplatIsAvailable;
		if (flag || BoostIsAvailable || ShrinkIsAvailable || FlameIsAvailable)
		{
			SplatIsAvailable = flag;
		}
		SaveData.Instance.Progress._powerplayPicker[3].Set = SplatIsAvailable;
		SaveData.Instance.Progress._hasUsedPowerPlayPicker.Set = true;
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(RingInstance);
		if (FlameInstance != null)
		{
			UnityEngine.Object.Destroy(FlameInstance);
			FlameInstance = null;
		}
	}

	private void FixedUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (FlameIsOn)
		{
			FlameCountDown -= deltaTime;
			if (FlameCountDown <= 0f)
			{
				FlameOff();
			}
		}
		if (SplatFingerIsOn)
		{
			SplatFingerCountDown -= deltaTime;
			if (SplatFingerCountDown <= 0f)
			{
				SplatFingerOff();
			}
		}
		if (!SquashIsOn)
		{
			return;
		}
		SquashCountDown -= deltaTime;
		if (SquashCountDown <= 1f)
		{
			_squashFlashTimer -= deltaTime;
			if (_squashFlashTimer < 0f)
			{
				if (SquashVisuals == 1.25f)
				{
					SquashVisuals = 0.75f;
				}
				else
				{
					SquashVisuals = 1.25f;
				}
				_squashFlashTimer = 0.15f;
			}
		}
		else
		{
			_squashFlashTimer = 0f;
		}
		if (SquashCountDown <= 0f)
		{
			SquashOff();
		}
	}

	public void StartRun()
	{
		DefaultSplatFingerTime = PowerupDatabase.Instance._SplatTime;
		FlameFallOffTime = PowerupDatabase.Instance._FlameFalloffTime;
		AccelerationMultiplier = PowerupDatabase.Instance._FlameAccelerationMultiplier;
		MaxSpeedMultiplier = PowerupDatabase.Instance._FlameVelocityMultiplier;
		FlameIsAvailable = SaveData.Instance.Progress._powerplayPicker[0].Set;
		BoostIsAvailable = SaveData.Instance.Progress._powerplayPicker[1].Set;
		ShrinkIsAvailable = SaveData.Instance.Progress._powerplayPicker[2].Set;
		SplatIsAvailable = SaveData.Instance.Progress._powerplayPicker[3].Set;
		ResetForRun();
	}

	public void ActivateRandomPowerup()
	{
		int num = 0;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		if (BoostIsAvailable)
		{
			num2 = num;
			num++;
		}
		if (FlameIsAvailable)
		{
			num3 = num;
			num++;
		}
		if (ShrinkIsAvailable)
		{
			num4 = num;
			num++;
		}
		if (SplatIsAvailable)
		{
			num5 = num;
			num++;
		}
		int num6 = UnityEngine.Random.Range(0, num);
		if (num6 == num3)
		{
			FlameOn();
		}
		else if (num6 == num2)
		{
			SuitOn();
		}
		else if (num6 == num5)
		{
			SplatFingerOn();
		}
		else if (num6 == num4)
		{
			SquashOn();
		}
		else
		{
			Debug.LogError("BAD CODING - NO CHOICE OF POWERPILL");
		}
	}

	public void FlameOn()
	{
		FlameIsOn = true;
		FlameCountDown = PowerupDatabase.Instance._FlameTime;
		if (FlameInstance == null)
		{
			FlameInstance = UnityEngine.Object.Instantiate(_FlamePrefab) as GameObject;
		}
		if (PowerUpManager.FlameOnHandler != null)
		{
			PowerUpManager.FlameOnHandler();
		}
	}

	public void SplatFingerOn()
	{
		SplatFingerIsOn = true;
		SplatFingerCountDown = DefaultSplatFingerTime;
		if (PowerUpManager.SplatFingerHandler != null)
		{
			PowerUpManager.SplatFingerHandler();
		}
	}

	public void SquashOn()
	{
		int currentZoomLevel = ZoomLevel.Instance.CurrentZoomLevel;
		SquashIsOn = true;
		SquashCountDown = PowerupDatabase.Instance._SquashTime;
		SquashSize = Pebble.Instance.Size;
		SquashVisuals = 0.75f;
		if (PowerUpManager.SquashOnHandler != null)
		{
			PowerUpManager.SquashOnHandler();
		}
		UnityEngine.Object.Destroy(RingInstance);
		RingInstance = UnityEngine.Object.Instantiate(_RingsPrefab) as GameObject;
		RingInstance.animation.Play("Level" + ((currentZoomLevel <= 0) ? "1" : currentZoomLevel.ToString()));
	}

	public void SuitOn()
	{
		SuitIsOn = true;
		Pebble.Instance.DoJacket();
		if (PowerUpManager.SuitUpHandler != null)
		{
			PowerUpManager.SuitUpHandler();
		}
	}

	public ClayData GiveClay(ClayData clay)
	{
		if (HeavyIsOn)
		{
			ClayData clayData = clay.Clone();
			clayData._Amount *= PowerupDatabase.Instance._HeavyMultiplier;
			return clayData;
		}
		return clay;
	}

	public float LoseClay(float clay)
	{
		if (SuitIsOn && Pebble.Instance.Size - clay < SuitSize)
		{
			return 0f;
		}
		return clay;
	}

	public float GetAcceleration(float acc)
	{
		if (FlameIsOn)
		{
			return acc * AccelerationMultiplier;
		}
		return acc;
	}

	public float GetMaxSpeed(float spd)
	{
		if (FlameIsOn)
		{
			float num = Mathf.Min(1f, FlameCountDown / FlameFallOffTime);
			float num2 = (spd * MaxSpeedMultiplier - spd) * num;
			return spd + num2;
		}
		return spd;
	}

	private void ResetForRun()
	{
		FlameOff();
		SquashOff();
		SuitOff();
		SplatFingerOff();
	}

	private void FlameOff()
	{
		if (FlameIsOn)
		{
			FlameIsOn = false;
			if (FlameInstance != null)
			{
				UnityEngine.Object.Destroy(FlameInstance);
				FlameInstance = null;
			}
		}
	}

	private void SplatFingerOff()
	{
		if (SplatFingerIsOn)
		{
			SplatFingerIsOn = false;
		}
	}

	private void SquashOff()
	{
		SquashVisuals = 1f;
		if (SquashIsOn)
		{
			SquashIsOn = false;
		}
	}

	private void SuitOff()
	{
		if (SuitIsOn)
		{
			SuitIsOn = false;
			Pebble.Instance.RemoveJacket();
		}
	}

	private bool CanAwardSquash()
	{
		object spawnedObstacleOnScreen_TooBigToSquash = HillObstacles.Instance.GetSpawnedObstacleOnScreen_TooBigToSquash(new Rect(0f, Screen.height / 2, Screen.width, Screen.height / 2));
		if (spawnedObstacleOnScreen_TooBigToSquash == null)
		{
			return false;
		}
		return true;
	}
}
