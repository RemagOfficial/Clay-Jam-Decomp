using UnityEngine;

[RequireComponent(typeof(UISprite))]
public class UISpriteHSVColourer : MonoBehaviour
{
	private UIAtlas _colouredAtlas;

	private UISprite _UISprite;

	private void InitSprite()
	{
		_UISprite = GetComponent<UISprite>();
		_colouredAtlas = base.gameObject.AddComponent<UIAtlas>();
		_colouredAtlas.spriteList = _UISprite.atlas.spriteList;
		_colouredAtlas.coordinates = _UISprite.atlas.coordinates;
		_colouredAtlas.spriteMaterial = _UISprite.atlas.spriteMaterial;
		_UISprite.atlas = _colouredAtlas;
	}

	public void UseColour(HSVColour hsvColour)
	{
		if (_colouredAtlas == null)
		{
			InitSprite();
		}
		Material material = new Material(_colouredAtlas.spriteMaterial);
		hsvColour.UseOnHSVMaterial(material);
		_colouredAtlas.spriteMaterial = material;
	}
}
