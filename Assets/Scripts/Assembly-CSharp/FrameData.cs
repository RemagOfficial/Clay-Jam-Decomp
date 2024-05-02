using System;
using UnityEngine;

[Serializable]
public class FrameData
{
	public Texture2D Texture { get; set; }

	public Vector2 Offset { get; set; }

	public Vector2 Scale { get; set; }

	public void ApplyToMaterial(Material material)
	{
		material.mainTexture = Texture;
		material.mainTextureOffset = Offset;
	}

	public void ApplyToMaterialWithScale(Material material)
	{
		material.mainTexture = Texture;
		material.mainTextureOffset = Offset;
		material.mainTextureScale = Scale;
	}
}
