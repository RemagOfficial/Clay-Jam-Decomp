using System;
using UnityEngine;

[Serializable]
public class SysFontTexture : ISysFontTexturable
{
	[SerializeField]
	protected string _text = string.Empty;

	[SerializeField]
	protected string _appleFontName = string.Empty;

	[SerializeField]
	protected string _androidFontName = string.Empty;

	[SerializeField]
	protected int _fontSize;

	[SerializeField]
	protected bool _isBold;

	[SerializeField]
	protected bool _isItalic;

	[SerializeField]
	protected SysFont.Alignment _alignment;

	[SerializeField]
	protected bool _isMultiLine = true;

	[SerializeField]
	protected int _maxWidthPixels = 2048;

	[SerializeField]
	protected int _maxHeightPixels = 2048;

	protected string _lastText;

	protected string _lastFontName;

	protected int _lastFontSize;

	protected bool _lastIsBold;

	protected bool _lastIsItalic;

	protected SysFont.Alignment _lastAlignment;

	protected bool _lastIsMultiLine;

	protected int _lastMaxWidthPixels;

	protected int _lastMaxHeightPixels;

	protected int _widthPixels = 1;

	protected int _heightPixels = 1;

	protected int _textWidthPixels;

	protected int _textHeightPixels;

	protected int _textureId;

	protected Texture2D _texture;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (_text != value)
			{
				_text = value;
			}
		}
	}

	public string AppleFontName
	{
		get
		{
			return _appleFontName;
		}
		set
		{
			if (_appleFontName != value)
			{
				_appleFontName = value;
			}
		}
	}

	public string AndroidFontName
	{
		get
		{
			return _androidFontName;
		}
		set
		{
			if (_androidFontName != value)
			{
				_androidFontName = value;
			}
		}
	}

	public string FontName
	{
		get
		{
			return AndroidFontName;
		}
		set
		{
			AndroidFontName = value;
		}
	}

	public int FontSize
	{
		get
		{
			return _fontSize;
		}
		set
		{
			if (_fontSize != value)
			{
				_fontSize = value;
			}
		}
	}

	public bool IsBold
	{
		get
		{
			return _isBold;
		}
		set
		{
			if (_isBold != value)
			{
				_isBold = value;
			}
		}
	}

	public bool IsItalic
	{
		get
		{
			return _isItalic;
		}
		set
		{
			if (_isItalic != value)
			{
				_isItalic = value;
			}
		}
	}

	public SysFont.Alignment Alignment
	{
		get
		{
			return _alignment;
		}
		set
		{
			if (_alignment != value)
			{
				_alignment = value;
			}
		}
	}

	public bool IsMultiLine
	{
		get
		{
			return _isMultiLine;
		}
		set
		{
			if (_isMultiLine != value)
			{
				_isMultiLine = value;
			}
		}
	}

	public int MaxWidthPixels
	{
		get
		{
			return _maxWidthPixels;
		}
		set
		{
			if (_maxWidthPixels != value)
			{
				_maxWidthPixels = value;
			}
		}
	}

	public int MaxHeightPixels
	{
		get
		{
			return _maxHeightPixels;
		}
		set
		{
			if (_maxHeightPixels != value)
			{
				_maxHeightPixels = value;
			}
		}
	}

	public int WidthPixels
	{
		get
		{
			return _widthPixels;
		}
	}

	public int HeightPixels
	{
		get
		{
			return _heightPixels;
		}
	}

	public int TextWidthPixels
	{
		get
		{
			return _textWidthPixels;
		}
	}

	public int TextHeightPixels
	{
		get
		{
			return _textHeightPixels;
		}
	}

	public bool IsUpdated
	{
		get
		{
			return _texture != null && _textureId != 0;
		}
	}

	public Texture2D Texture
	{
		get
		{
			return _texture;
		}
	}

	public bool NeedsRedraw
	{
		get
		{
			return _text != _lastText || FontName != _lastFontName || _fontSize != _lastFontSize || _isBold != _lastIsBold || _isItalic != _lastIsItalic || _alignment != _lastAlignment || _isMultiLine != _lastIsMultiLine || _maxWidthPixels != _lastMaxWidthPixels || _maxHeightPixels != _lastMaxHeightPixels;
		}
	}

	public bool Update()
	{
		if (_texture == null)
		{
			_texture = new Texture2D(1, 1, TextureFormat.Alpha8, false);
			_texture.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
			_texture.filterMode = FilterMode.Point;
			_texture.wrapMode = TextureWrapMode.Clamp;
			_texture.Apply(false, true);
			_textureId = 0;
		}
		if (_textureId == 0)
		{
			_textureId = _texture.GetNativeTextureID();
			if (_textureId == 0)
			{
				return false;
			}
		}
		SysFont.QueueTexture(_text, FontName, _fontSize, _isBold, _isItalic, _alignment, _isMultiLine, _maxWidthPixels, _maxHeightPixels, _textureId);
		_textWidthPixels = SysFont.GetTextWidth(_textureId);
		_textHeightPixels = SysFont.GetTextHeight(_textureId);
		_widthPixels = SysFont.GetTextureWidth(_textureId);
		_heightPixels = SysFont.GetTextureHeight(_textureId);
		SysFont.UpdateQueuedTexture(_textureId);
		_lastText = _text;
		_lastFontName = FontName;
		_lastFontSize = _fontSize;
		_lastIsBold = _isBold;
		_lastIsItalic = _isItalic;
		_lastAlignment = _alignment;
		_lastIsMultiLine = _isMultiLine;
		_lastMaxWidthPixels = _maxWidthPixels;
		_lastMaxHeightPixels = _maxHeightPixels;
		return true;
	}

	public void Destroy()
	{
		if (_texture != null)
		{
			if (_textureId != 0)
			{
				SysFont.DequeueTexture(_textureId);
				_textureId = 0;
			}
			SysFont.SafeDestroy(_texture);
			_texture = null;
		}
	}
}
