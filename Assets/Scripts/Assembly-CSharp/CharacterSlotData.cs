using UnityEngine;

public struct CharacterSlotData
{
	public Renderer Renderer;

	public CastData CastData;

	public CharacterSlotData(Renderer renderer, CastData castData, Texture2D icon)
	{
		Renderer = renderer;
		CastData = castData;
		Renderer.material.mainTexture = icon;
		CastData.Colour.UseOnHSVFastMaterial(renderer.material);
	}

	public void TurnOnIfPurchasedOnHill(int hillID)
	{
		if (CastData.GetStateOnHill(hillID) == LockState.Purchased)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	public void TurnOn()
	{
		Renderer.enabled = true;
	}

	public void TurnOff()
	{
		Renderer.enabled = false;
	}
}
