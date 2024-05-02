using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/PowerupActivationLabel")]
public class UIPowerUpActivationLabel : MonoBehaviour
{
	private LocalisableText _label;

	private void Awake()
	{
		_label = GetComponent<LocalisableText>();
		PowerUpManager.SquashOnHandler += OnSquashOn;
		PowerUpManager.SuitUpHandler += OnSuitOn;
		PowerUpManager.FlameOnHandler += OnFlameOn;
		PowerUpManager.SplatFingerHandler += OnSplatFinger;
		InGameController.RunStarted += OnRunStarted;
	}

	private void OnDestroy()
	{
		PowerUpManager.SquashOnHandler -= OnSquashOn;
		PowerUpManager.SuitUpHandler -= OnSuitOn;
		PowerUpManager.FlameOnHandler -= OnFlameOn;
		PowerUpManager.SplatFingerHandler -= OnSplatFinger;
		InGameController.RunStarted -= OnRunStarted;
	}

	private void OnEnable()
	{
		_label.text = string.Empty;
	}

	private void OnDisable()
	{
		_label.text = string.Empty;
	}

	private void OnSquashOn()
	{
		Trigger(Localization.instance.Get("POWERUP_Shrink"));
	}

	private void OnSuitOn()
	{
		Trigger(Localization.instance.Get("POWERUP_ClayBoost"));
	}

	private void OnFlameOn()
	{
		Trigger(Localization.instance.Get("POWERUP_FieryBonus"));
	}

	private void OnSplatFinger()
	{
		if (BuildDetails.Instance._UseLeapIfAvailable)
		{
			Trigger(Localization.instance.Get("POWERUP_Splat_Leap"));
		}
		else
		{
			Trigger(Localization.instance.Get("POWERUP_Splat"));
		}
	}

	private void Trigger(string name)
	{
		_label.text = name;
		base.animation.Rewind();
		base.animation.Play();
	}

	private void OnRunStarted()
	{
		_label.text = string.Empty;
	}

	private void Update()
	{
		if (Pebble.Instance.PowerUpManager.SplatTimeRemaining > 1f && base.animation[base.animation.clip.name].normalizedTime > 0.5f)
		{
			base.animation[base.animation.clip.name].normalizedTime = 0.5f;
		}
	}
}
