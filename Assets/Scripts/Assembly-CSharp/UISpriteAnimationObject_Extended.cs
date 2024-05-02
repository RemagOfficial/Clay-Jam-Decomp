using System;
using System.Collections.Generic;

[Serializable]
public class UISpriteAnimationObject_Extended
{
	public string _Prefix;

	public List<string> _Frames = new List<string>();

	public int _FPS = 30;

	public bool _Repeat;
}
