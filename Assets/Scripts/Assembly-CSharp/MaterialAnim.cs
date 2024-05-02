using System;
using UnityEngine;

public class MaterialAnim : MonoBehaviour
{
	public enum PropertyType
	{
		Color = 0,
		Float = 1
	}

	[Serializable]
	public class KeyFrame
	{
		public Color _ColourValue;

		public float _FloatValue;

		public float _time;
	}

	public string _PropertyName = "_Color";

	public PropertyType _PropertyType;

	public KeyFrame[] _KeyFrames;

	private bool _ready;

	private void Awake()
	{
		_ready = false;
		if (_KeyFrames == null || _KeyFrames.Length == 0)
		{
			Debug.LogError("No keyframes set for material animation", base.gameObject);
			return;
		}
		Material material = null;
		if (base.renderer != null)
		{
			material = base.renderer.material;
		}
		if (material != null && !material.HasProperty(_PropertyName))
		{
			material = null;
		}
		if (material == null)
		{
			Debug.LogError(string.Format("MaterialAnim needs to be on an object with a renderer and material using {0} property", _PropertyName), base.gameObject);
		}
		else
		{
			_ready = true;
		}
	}

	public void SetToEnd()
	{
		int num = _KeyFrames.Length - 1;
		Apply(_KeyFrames[num], _KeyFrames[num], 1f);
	}

	public void UpdateAnim(float animTime)
	{
		if (!_ready)
		{
			return;
		}
		int num = _KeyFrames.Length - 1;
		int i;
		for (i = 0; i < num && _KeyFrames[i]._time < animTime; i++)
		{
		}
		if (i == 0)
		{
			Apply(_KeyFrames[i], _KeyFrames[i], 0f);
			return;
		}
		if (_KeyFrames[i]._time < animTime)
		{
			Apply(_KeyFrames[i], _KeyFrames[i], 0f);
			return;
		}
		int num2 = i - 1;
		float time = _KeyFrames[i]._time;
		float time2 = _KeyFrames[num2]._time;
		float num3 = time - time2;
		if (num3 <= 0f)
		{
			Apply(_KeyFrames[num2], _KeyFrames[num2], 0f);
			Debug.LogError("KeyFrame found with equal or higher time than previous frame");
		}
		else
		{
			float num4 = animTime - time2;
			float t = num4 / num3;
			Apply(_KeyFrames[num2], _KeyFrames[i], t);
		}
	}

	private void Apply(KeyFrame previousFrame, KeyFrame nextFrame, float t)
	{
		switch (_PropertyType)
		{
		case PropertyType.Color:
		{
			Color color = Color.Lerp(previousFrame._ColourValue, nextFrame._ColourValue, t);
			base.renderer.material.SetColor(_PropertyName, color);
			break;
		}
		case PropertyType.Float:
		{
			float value = Mathf.Lerp(previousFrame._FloatValue, nextFrame._FloatValue, t);
			base.renderer.material.SetFloat(_PropertyName, value);
			break;
		}
		}
	}
}
