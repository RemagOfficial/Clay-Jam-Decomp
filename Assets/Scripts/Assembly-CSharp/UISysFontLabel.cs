using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/SysFont Label")]
public class UISysFontLabel : UIWidget, ISysFontTexturable
{
	[SerializeField]
	protected SysFontTexture _texture = new SysFontTexture();

	protected static Shader _shader;

	protected Material _createdMaterial;

	protected Vector3[] _vertices;

	protected Vector2 _uv;

	public UILabel referenceLabel;

	public string Text
	{
		get
		{
			return _texture.Text;
		}
		set
		{
			_texture.Text = value;
		}
	}

	public string AppleFontName
	{
		get
		{
			return _texture.AppleFontName;
		}
		set
		{
			_texture.AppleFontName = value;
		}
	}

	public string AndroidFontName
	{
		get
		{
			return _texture.AndroidFontName;
		}
		set
		{
			_texture.AndroidFontName = value;
		}
	}

	public string FontName
	{
		get
		{
			return _texture.FontName;
		}
		set
		{
			_texture.FontName = value;
		}
	}

	public int FontSize
	{
		get
		{
			return _texture.FontSize;
		}
		set
		{
			_texture.FontSize = value;
		}
	}

	public bool IsBold
	{
		get
		{
			return _texture.IsBold;
		}
		set
		{
			_texture.IsBold = value;
		}
	}

	public bool IsItalic
	{
		get
		{
			return _texture.IsItalic;
		}
		set
		{
			_texture.IsItalic = value;
		}
	}

	public SysFont.Alignment Alignment
	{
		get
		{
			return _texture.Alignment;
		}
		set
		{
			_texture.Alignment = value;
		}
	}

	public bool IsMultiLine
	{
		get
		{
			return _texture.IsMultiLine;
		}
		set
		{
			_texture.IsMultiLine = value;
		}
	}

	public int MaxWidthPixels
	{
		get
		{
			return _texture.MaxWidthPixels;
		}
		set
		{
			_texture.MaxWidthPixels = value;
		}
	}

	public int MaxHeightPixels
	{
		get
		{
			return _texture.MaxHeightPixels;
		}
		set
		{
			_texture.MaxHeightPixels = value;
		}
	}

	public int WidthPixels
	{
		get
		{
			return _texture.WidthPixels;
		}
	}

	public int HeightPixels
	{
		get
		{
			return _texture.HeightPixels;
		}
	}

	public int TextWidthPixels
	{
		get
		{
			return _texture.TextWidthPixels;
		}
	}

	public int TextHeightPixels
	{
		get
		{
			return _texture.TextHeightPixels;
		}
	}

	public Texture2D Texture
	{
		get
		{
			return _texture.Texture;
		}
	}

	public override bool keepMaterial
	{
		get
		{
			return true;
		}
	}

	public void SortOut()
	{
		if (referenceLabel != null)
		{
			_texture.Update();
			_uv = new Vector2((float)_texture.TextWidthPixels / (float)_texture.WidthPixels, (float)_texture.TextHeightPixels / (float)_texture.HeightPixels);
			Vector3 localScale = referenceLabel.transform.localScale;
			float num = (float)_texture.TextWidthPixels / (float)_texture.TextHeightPixels;
			localScale.x = localScale.y * num;
			base.transform.localScale = localScale;
		}
	}

	public override void Update()
	{
		base.Update();
		if (_texture.NeedsRedraw && _texture.Update())
		{
			_uv = new Vector2((float)_texture.TextWidthPixels / (float)_texture.WidthPixels, (float)_texture.TextHeightPixels / (float)_texture.HeightPixels);
		}
	}

	public override void MakePixelPerfect()
	{
		Vector3 localScale = base.cachedTransform.localScale;
		if (_texture.TextWidthPixels <= 1)
		{
			localScale.x = 1f;
		}
		else
		{
			localScale.x = _texture.TextWidthPixels;
		}
		if (_texture.TextHeightPixels <= 1)
		{
			localScale.y = 1f;
		}
		else
		{
			localScale.y = _texture.TextHeightPixels;
		}
		base.cachedTransform.localScale = localScale;
		base.MakePixelPerfect();
	}

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		if (_vertices == null)
		{
			_vertices = new Vector3[4];
			_vertices[0] = new Vector3(1f, 0f, 0f);
			_vertices[1] = new Vector3(1f, -1f, 0f);
			_vertices[2] = new Vector3(0f, -1f, 0f);
			_vertices[3] = new Vector3(0f, 0f, 0f);
		}
		verts.Add(_vertices[0]);
		verts.Add(_vertices[1]);
		verts.Add(_vertices[2]);
		verts.Add(_vertices[3]);
		uvs.Add(_uv);
		uvs.Add(new Vector2(_uv.x, 0f));
		uvs.Add(Vector2.zero);
		uvs.Add(new Vector2(0f, _uv.y));
		cols.Add(base.color);
		cols.Add(base.color);
		cols.Add(base.color);
		cols.Add(base.color);
		MakePixelPerfect();
		if (material.mainTexture != _texture.Texture)
		{
			material.mainTexture = _texture.Texture;
		}
	}

	protected override void OnEnable()
	{
		if (_shader == null)
		{
			_shader = Shader.Find("Unlit/Transparent Colored (Packed)");
		}
		if (_createdMaterial == null)
		{
			_createdMaterial = new Material(_shader);
			_createdMaterial.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
			_createdMaterial.mainTexture = _texture.Texture;
			material = _createdMaterial;
		}
	}

	protected void OnDestroy()
	{
		material = null;
		SysFont.SafeDestroy(_createdMaterial);
		if (_texture != null)
		{
			_texture.Destroy();
			_texture = null;
		}
	}
}
