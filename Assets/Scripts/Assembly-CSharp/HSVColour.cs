using System;
using UnityEngine;

[Serializable]
public class HSVColour
{
	public float _Hue;

	public float _Saturation;

	public float _Value;

	private float _vsu;

	private float _vsw;

	private float _rr;

	private float _rg;

	private float _rb;

	private float _gr;

	private float _gg;

	private float _gb;

	private float _br;

	private float _bg;

	private float _bb;

	public static HSVColour NoShift = new HSVColour
	{
		_Saturation = 1f,
		_Value = 1f
	};

	public void UseOnHSVMaterial(Material HSVMaterial)
	{
		HSVMaterial.SetFloat("_HueShift", _Hue);
		HSVMaterial.SetFloat("_Sat", _Saturation);
		HSVMaterial.SetFloat("_Val", _Value);
	}

	public void UseOnHSVFastMaterial(Material HSVFastMaterial)
	{
		UseOnHSVMaterial(HSVFastMaterial);
		SetUpHSVFast(HSVFastMaterial);
	}

	public void SetFromHSVMaterial(Material HSVMaterial)
	{
		_Hue = HSVMaterial.GetFloat("_HueShift");
		_Saturation = HSVMaterial.GetFloat("_Sat");
		_Value = HSVMaterial.GetFloat("_Val");
	}

	public Color ShiftRGBColour(Color originalPixel)
	{
		Color result = default(Color);
		CalculateDerivedValues();
		result.r = _rr * originalPixel.r + _rg * originalPixel.g + _rb * originalPixel.b;
		result.g = _gr * originalPixel.r + _gg * originalPixel.g + _gb * originalPixel.b;
		result.b = _br * originalPixel.r + _bg * originalPixel.g + _bb * originalPixel.b;
		return result;
	}

	public void ApplyToTexture(Texture2D source, Texture2D destination)
	{
		for (int i = 0; i < destination.width; i++)
		{
			for (int j = 0; j < destination.height; j++)
			{
				Color pixel = source.GetPixel(i, j);
				Color color = ShiftRGBColour(pixel);
				destination.SetPixel(i, j, color);
			}
		}
		destination.Apply();
	}

	public void SetUpHSVFast(Material HSVFastMaterial)
	{
		CalculateDerivedValues();
		HSVFastMaterial.SetFloat("_VSU", _vsu);
		HSVFastMaterial.SetFloat("_VSW", _vsw);
		HSVFastMaterial.SetFloat("_RR", _rr);
		HSVFastMaterial.SetFloat("_RG", _rg);
		HSVFastMaterial.SetFloat("_RB", _rb);
		HSVFastMaterial.SetFloat("_GR", _gr);
		HSVFastMaterial.SetFloat("_GG", _gg);
		HSVFastMaterial.SetFloat("_GB", _gb);
		HSVFastMaterial.SetFloat("_BR", _br);
		HSVFastMaterial.SetFloat("_BG", _bg);
		HSVFastMaterial.SetFloat("_BB", _bb);
	}

	private void CalculateDerivedValues()
	{
		CalculateVSU();
		CalculateVSW();
		CalculateMatrix();
	}

	private void CalculateVSU()
	{
		_vsu = _Value * _Saturation * Mathf.Cos(_Hue * (float)Math.PI / 180f);
	}

	private void CalculateVSW()
	{
		_vsw = _Value * _Saturation * Mathf.Sin(_Hue * (float)Math.PI / 180f);
	}

	private void CalculateMatrix()
	{
		_rr = 0.299f * _Value + 0.701f * _vsu + 0.168f * _vsw;
		_rg = 0.587f * _Value - 0.587f * _vsu + 0.33f * _vsw;
		_rb = 0.114f * _Value - 0.114f * _vsu - 0.497f * _vsw;
		_gr = 0.299f * _Value - 0.299f * _vsu - 0.328f * _vsw;
		_gg = 0.587f * _Value + 0.413f * _vsu + 0.035f * _vsw;
		_gb = 0.114f * _Value - 0.114f * _vsu + 0.292f * _vsw;
		_br = 0.299f * _Value - 0.3f * _vsu + 1.25f * _vsw;
		_bg = 0.587f * _Value - 0.588f * _vsu - 1.05f * _vsw;
		_bb = 0.114f * _Value + 0.886f * _vsu - 0.203f * _vsw;
	}
}
