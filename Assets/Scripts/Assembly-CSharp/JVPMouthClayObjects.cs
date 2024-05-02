using System;
using UnityEngine;

[Serializable]
public class JVPMouthClayObjects
{
	public GameObject _MainObject;

	public UnityEngine.Object _ClaySizeAnimPrefab;

	private Material _mainMaterial;

	private AnimatedSprite _claySizeAnim;

	private static float[] ClaySizePerFrame = new float[12]
	{
		1f,
		5f,
		10f,
		15f,
		25f,
		40f,
		70f,
		100f,
		150f,
		200f,
		300f,
		float.MaxValue
	};

	public void Initialise()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_ClaySizeAnimPrefab) as GameObject;
		gameObject.transform.parent = _MainObject.transform;
		_claySizeAnim = gameObject.GetComponent<AnimatedSprite>();
		_mainMaterial = _MainObject.renderer.material;
	}

	private void UpdateFromAnim()
	{
		_claySizeAnim.CurrentFrameData.ApplyToMaterial(_mainMaterial);
	}

	public void UseColour(HSVColour colour)
	{
		colour.UseOnHSVMaterial(_mainMaterial);
	}

	public void Open(float cost)
	{
		int frame = ClaySizeFrameForCost(cost);
		_claySizeAnim.ShowFrame(frame);
		UpdateFromAnim();
	}

	public void Close()
	{
	}

	private static int ClaySizeFrameForCost(float cost)
	{
		int i;
		for (i = 0; ClaySizePerFrame[i] < cost; i++)
		{
		}
		return i;
	}
}
