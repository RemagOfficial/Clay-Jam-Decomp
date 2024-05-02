using UnityEngine;

public class HSVFastMaterialInitialiser : MonoBehaviour
{
	private void Start()
	{
		InitialiseChildRenderers(base.gameObject);
	}

	public static void InitialiseChildRenderers(GameObject gameObject)
	{
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (IsHSVFastMaterial(renderer.material))
			{
				HSVColour hSVColour = new HSVColour();
				hSVColour.SetFromHSVMaterial(renderer.material);
				hSVColour.SetUpHSVFast(renderer.material);
			}
		}
	}

	public static bool IsHSVFastMaterial(Material material)
	{
		if (!material.HasProperty("_HueShift"))
		{
			return false;
		}
		if (!material.HasProperty("_VSW"))
		{
			return false;
		}
		return true;
	}
}
