using System;
using UnityEngine;

[Serializable]
public class AvalancheColour
{
	public Color _Color;

	public Color _Saturation;

	internal void Lerp(AvalancheColour colour1, AvalancheColour colour2, float t)
	{
		_Color = Color.Lerp(colour1._Color, colour2._Color, t);
		_Saturation = Color.Lerp(colour1._Saturation, colour2._Saturation, t);
	}

	public void UseOnMaterial(Material mat)
	{
		mat.SetColor("_Saturation", _Saturation);
		mat.SetColor("_Color", _Color);
	}
}
