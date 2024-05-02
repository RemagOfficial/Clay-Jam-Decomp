using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/ClayJam/Sprite per Level")]
[RequireComponent(typeof(UISprite))]
public class UISpriteOnLevel : MonoBehaviour
{
	public List<string> _LevelSpriteNames;

	private UISprite Sprite;

	private void OnEnable()
	{
		int sprite = CurrentHill.Instance.ID - 1;
		SetSprite(sprite);
	}

	private void SetSprite(int id)
	{
		if (Sprite == null)
		{
			Sprite = GetComponent<UISprite>();
		}
		Sprite.spriteName = _LevelSpriteNames[id];
	}
}
