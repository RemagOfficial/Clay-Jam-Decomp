using System;
using UnityEngine;

[Serializable]
public class JVPInfoText
{
	public UnityEngine.Object _TextPrefab;

	private TextMesh _textMesh;

	public void Initialise(GameObject parent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_TextPrefab) as GameObject;
		gameObject.transform.parent = parent.transform;
		_textMesh = gameObject.GetComponent<TextMesh>();
	}

	public void SetText(string text)
	{
		_textMesh.text = text;
	}

	public void Show(bool show)
	{
		_textMesh.gameObject.active = show;
	}

	public void SetColour(Color color)
	{
		_textMesh.renderer.material.color = color;
	}
}
