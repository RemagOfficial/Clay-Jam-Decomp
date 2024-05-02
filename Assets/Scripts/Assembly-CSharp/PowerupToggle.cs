using UnityEngine;

public class PowerupToggle : MonoBehaviour
{
	public enum Type
	{
		Flame = 0,
		Boost = 1,
		Shrink = 2,
		Splat = 3
	}

	private static HSVColour NotAvailableCol = new HSVColour
	{
		_Value = 1f
	};

	private static HSVColour AvailableCol = new HSVColour
	{
		_Saturation = 1f,
		_Value = 1f
	};

	public Type _Type;

	public UISprite _Icon;

	public GameObject _Upgraded;

	private UISprite _NotAvailableSprite;

	private void Awake()
	{
		_Icon.material = new Material(_Icon.material);
		_NotAvailableSprite = GetComponent<UISprite>();
	}

	private void Update()
	{
		if (_NotAvailableSprite.enabled)
		{
			NotAvailableCol.UseOnHSVMaterial(_Icon.material);
		}
		else
		{
			AvailableCol.UseOnHSVMaterial(_Icon.material);
		}
	}

	public void ShowUserSetting()
	{
		bool flag = false;
		switch (_Type)
		{
		case Type.Flame:
			flag = Pebble.Instance.PowerUpManager.FlameIsAvailable;
			break;
		case Type.Boost:
			flag = Pebble.Instance.PowerUpManager.BoostIsAvailable;
			break;
		case Type.Shrink:
			flag = Pebble.Instance.PowerUpManager.ShrinkIsAvailable;
			break;
		case Type.Splat:
			flag = Pebble.Instance.PowerUpManager.SplatIsAvailable;
			break;
		}
		_NotAvailableSprite.enabled = !flag;
		ShowUpgraded();
	}

	private void TurnUpgradedIconOn(bool on)
	{
		_Upgraded.SetActiveRecursively(on);
	}

	public void ShowUpgraded()
	{
		if (!PowerupDatabase.Instance.PowerupsAreUpgradeable)
		{
			TurnUpgradedIconOn(false);
			return;
		}
		bool on = false;
		switch (_Type)
		{
		case Type.Flame:
			on = PowerupDatabase.Instance.IsFlameUpgraded;
			break;
		case Type.Boost:
			on = PowerupDatabase.Instance.IsBoostUpgraded;
			break;
		case Type.Shrink:
			on = PowerupDatabase.Instance.IsShrinkUpgraded;
			break;
		case Type.Splat:
			on = PowerupDatabase.Instance.IsSplatUpgraded;
			break;
		}
		TurnUpgradedIconOn(on);
	}

	public void TurnOn()
	{
		_NotAvailableSprite.enabled = false;
	}

	public void DoToggle(GameObject buttonTarget)
	{
		if (base.gameObject == buttonTarget)
		{
			switch (_Type)
			{
			case Type.Flame:
				Pebble.Instance.PowerUpManager.ToggleFlameAvailable();
				break;
			case Type.Boost:
				Pebble.Instance.PowerUpManager.ToggleBoostAvailable();
				break;
			case Type.Shrink:
				Pebble.Instance.PowerUpManager.ToggleShrinkAvailable();
				break;
			case Type.Splat:
				Pebble.Instance.PowerUpManager.ToggleSplatAvailable();
				break;
			}
		}
	}
}
