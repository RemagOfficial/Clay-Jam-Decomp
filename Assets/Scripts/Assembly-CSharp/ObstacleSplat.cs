using System.Collections.Generic;
using UnityEngine;

public class ObstacleSplat : MonoBehaviour
{
	public float _MinScale = 0.4f;

	public float _MaxScale = 1.8f;

	private List<Material> _materialsToColour;

	private void Awake()
	{
		GetMaterialsToColour();
	}

	private void Start()
	{
		Stop();
	}

	public void Go(float normalisedSize, HSVColour colour, Vector3 pos, Quaternion rotation)
	{
		base.enabled = true;
		base.animation.Play();
		SetScale(normalisedSize);
		SetColour(colour);
		base.transform.position = pos;
		base.transform.rotation = rotation;
	}

	private void Update()
	{
		if (!base.animation.isPlaying)
		{
			Stop();
		}
	}

	private void GetMaterialsToColour()
	{
		_materialsToColour = new List<Material>();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			_materialsToColour.Add(renderer.material);
			renderer.enabled = false;
		}
	}

	private void Stop()
	{
		base.enabled = false;
	}

	private void SetColour(HSVColour colour)
	{
		foreach (Material item in _materialsToColour)
		{
			colour.UseOnHSVMaterial(item);
		}
	}

	private void SetScale(float normalisedSize)
	{
		float num = Mathf.Lerp(_MinScale, _MaxScale, normalisedSize);
		base.transform.localScale = new Vector3(num, num, num);
	}
}
