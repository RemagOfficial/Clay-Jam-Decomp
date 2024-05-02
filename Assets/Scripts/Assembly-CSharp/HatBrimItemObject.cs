using UnityEngine;

public class HatBrimItemObject
{
	private static string TitleObjectName = "NameText";

	private static string CostObjectName = "CostText";

	private static string NumberObjectName = "InfoNumberText";

	private static string PurchaseResourceAddress = "BigJigger/TileMaterials/BigJiggerSquare";

	private static string PurchaseResourceFormat = "{0}{1}";

	public static float hatItemUpScale = 1.15f;

	public GameObject GameObject { get; set; }

	public Material Material { get; set; }

	public LocalisableText TitleText { get; set; }

	public TextMesh CostTextOLD { get; set; }

	public LocalisableText CostText { get; set; }

	public TextMesh NumberText { get; set; }

	public void Initialise(GameObject gameObject)
	{
		GameObject = gameObject;
		Renderer renderer = gameObject.renderer;
		Material material = renderer.material;
		Material = material;
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == TitleObjectName)
			{
				TitleText = transform.gameObject.GetComponent<LocalisableText>();
			}
			if (transform.name == CostObjectName)
			{
				CostTextOLD = transform.gameObject.GetComponent<TextMesh>();
				CostText = transform.gameObject.GetComponent<LocalisableText>();
			}
			if (transform.name == NumberObjectName)
			{
				NumberText = transform.gameObject.GetComponent<TextMesh>();
			}
		}
		if (TitleText == null)
		{
			Debug.LogError(string.Format("No {0} child object with LocalisableText component found on hat brim object {1}", TitleObjectName, gameObject.name), gameObject);
		}
		if (CostText == null)
		{
			if (CostTextOLD == null)
			{
				Debug.LogError(string.Format("No {0} child object with TextMesh component found on hat brim object {1}", CostObjectName, gameObject.name), gameObject);
			}
			else
			{
				Debug.LogWarning(string.Format("{0} needs to change to a localiseable text", CostObjectName), gameObject);
			}
		}
		if (NumberText == null)
		{
			Debug.LogError(string.Format("No {0} child object with TextMesh component found on hat brim object {1}", NumberObjectName, gameObject.name), gameObject);
		}
	}

	private void LoadPurchaseState(HatBrimItem item)
	{
		Renderer renderer = GameObject.transform.parent.renderer;
		Material[] array = new Material[3];
		for (int i = 0; i < array.Length; i++)
		{
			Material material = (Material)Resources.Load(string.Format(PurchaseResourceFormat, PurchaseResourceAddress, i));
			if (material == null)
			{
				Debug.LogError(string.Format("Error Loading Tile Material!"));
			}
			else
			{
				array[i] = material;
			}
		}
		if (item.CanAfford && item.LockState == LockState.Unlocked)
		{
			renderer.material = array[1];
		}
		else if (item.LockState == LockState.Purchased)
		{
			renderer.material = array[2];
		}
		else
		{
			renderer.material = array[0];
		}
	}

	public void SetupForItem(HatBrimItem item)
	{
		Material.mainTexture = item.Texture;
		LoadPurchaseState(item);
		switch (item.Type)
		{
		case HatBrimItem.ItemType.Cast:
			SetLockStateColours(item.CastData.Colour, item);
			if ((bool)NumberText)
			{
				NumberText.text = string.Empty;
			}
			break;
		case HatBrimItem.ItemType.Hill:
			SetLockStateColours(CurrentHill.Instance.Definition._ColourFromOrange, item);
			if ((bool)NumberText)
			{
				NumberText.text = ((item.UpgradeLevel <= 0) ? string.Empty : item.UpgradeLevel.ToString());
			}
			break;
		case HatBrimItem.ItemType.PowerPlay:
			SetLockStateColours(CurrentHill.Instance.Definition._PowerPlayColour, item);
			if ((bool)NumberText)
			{
				NumberText.text = string.Format("{0:0}", PowerupDatabase.Instance.PowerPlaysInPack(item.PowerPlayPackIndex, CurrentHill.Instance.ID));
			}
			break;
		}
		if ((bool)TitleText)
		{
			TitleText.text = item.Name;
		}
		if ((bool)CostText)
		{
			if (Localization.instance.shouldUseSystemFont)
			{
				CostText.text = ((!item.IsPurchased) ? string.Format("{0:0}", Localization.PunctuatedNumber(item.Cost, int.MaxValue)) : Localization.instance.GetFor3DText("JVP_Purchased"));
			}
			else
			{
				CostText.text = ((!item.IsPurchased) ? string.Format("#{0:0}", Localization.PunctuatedNumber(item.Cost, int.MaxValue)) : Localization.instance.GetFor3DText("JVP_Purchased"));
			}
		}
		else if ((bool)CostTextOLD)
		{
			if (Localization.instance.shouldUseSystemFont)
			{
				CostTextOLD.text = ((!item.IsPurchased) ? string.Format("{0:0}", Localization.PunctuatedNumber(item.Cost, int.MaxValue)) : Localization.instance.GetFor3DText("JVP_Purchased"));
			}
			else
			{
				CostTextOLD.text = ((!item.IsPurchased) ? string.Format("#{0:0}", Localization.PunctuatedNumber(item.Cost, int.MaxValue)) : Localization.instance.GetFor3DText("JVP_Purchased"));
			}
		}
	}

	private void SetLockStateColours(HSVColour defaultColour, HatBrimItem item)
	{
		LockState lockState = item.LockState;
		JVPController instance = JVPController.Instance;
		if (lockState == LockState.Locked)
		{
			HSVColour lockedIconColour = ColourDatabase.Instance._LockedIconColour;
			lockedIconColour.UseOnHSVFastMaterial(Material);
		}
		else
		{
			HSVColour hSVColour = new HSVColour();
			hSVColour._Hue = defaultColour._Hue;
			hSVColour._Saturation = defaultColour._Saturation;
			hSVColour._Value = defaultColour._Value;
			if (!item.CanAfford && lockState != LockState.Purchased)
			{
				hSVColour._Saturation = 0f;
			}
			hSVColour.UseOnHSVFastMaterial(Material);
		}
		switch (lockState)
		{
		case LockState.Purchased:
			SetTextMaterial(instance._PurchasedTextMaterial, instance._PurchasedNumberMaterial);
			break;
		default:
			if (item.CanAfford)
			{
				SetTextMaterial(instance._CanAffordTextMaterial, instance._CanAffordNumberMaterial);
				break;
			}
			goto case LockState.Locked;
		case LockState.Locked:
			SetTextMaterial(instance._CantAffordTextMaterial, instance._CantAffordNumberMaterial);
			break;
		}
	}

	private void SetTextMaterial(Material costAndTitleMat, Material numberMat = null)
	{
		if ((bool)TitleText)
		{
			TitleText.SetMaterial(costAndTitleMat);
		}
		if ((bool)CostText)
		{
			CostText.SetMaterial(costAndTitleMat);
		}
		if ((bool)NumberText)
		{
			NumberText.renderer.material = ((!numberMat) ? costAndTitleMat : numberMat);
		}
	}

	public void Scale(bool up = true)
	{
		GameObject.transform.parent.localScale = Vector3.one;
		if (up)
		{
			GameObject.transform.parent.localScale *= hatItemUpScale;
		}
	}
}
