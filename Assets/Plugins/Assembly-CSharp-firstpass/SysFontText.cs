using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("SysFont/Text")]
public class SysFontText : MonoBehaviour, ISysFontTexturable
{
	public enum PivotAlignment
	{
		TopLeft = 0,
		Top = 1,
		TopRight = 2,
		Left = 3,
		Center = 4,
		Right = 5,
		BottomLeft = 6,
		Bottom = 7,
		BottomRight = 8
	}

	[SerializeField]
	protected SysFontTexture _texture = new SysFontTexture();

	[SerializeField]
	protected Color _fontColor = Color.white;

	[SerializeField]
	protected PivotAlignment _pivot = PivotAlignment.Center;

	protected Color _lastFontColor;

	protected PivotAlignment _lastPivot;

	protected Transform _transform;

	protected Material _createdMaterial;

	protected Material _material;

	protected Vector3[] _vertices;

	protected Vector2[] _uv;

	protected int[] _triangles;

	protected Mesh _mesh;

	protected MeshFilter _filter;

	protected MeshRenderer _renderer;

	protected static Shader _shader;

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

	public Color FontColor
	{
		get
		{
			return _fontColor;
		}
		set
		{
			if (_fontColor != value)
			{
				_fontColor = value;
			}
		}
	}

	public PivotAlignment Pivot
	{
		get
		{
			return _pivot;
		}
		set
		{
			if (_pivot != value)
			{
				_pivot = value;
			}
		}
	}

	protected void UpdateMesh()
	{
		if (_filter == null)
		{
			_filter = base.gameObject.GetComponent<MeshFilter>();
			if (_filter == null)
			{
				_filter = base.gameObject.AddComponent<MeshFilter>();
				_filter.hideFlags = HideFlags.HideInInspector;
			}
		}
		if (_renderer == null)
		{
			_renderer = base.gameObject.GetComponent<MeshRenderer>();
			if (_renderer == null)
			{
				_renderer = base.gameObject.AddComponent<MeshRenderer>();
				_renderer.hideFlags = HideFlags.HideInInspector;
			}
			if (_shader == null)
			{
				_shader = Shader.Find("SysFont/Unlit Transparent");
			}
			if (_createdMaterial == null)
			{
				_createdMaterial = new Material(_shader);
			}
			_createdMaterial.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
			_material = _createdMaterial;
			_renderer.sharedMaterial = _material;
		}
		_material.color = _fontColor;
		_lastFontColor = _fontColor;
		if (_uv == null)
		{
			_uv = new Vector2[4];
			_triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
		}
		Vector2 vector = new Vector2((float)_texture.TextWidthPixels / (float)_texture.WidthPixels, (float)_texture.TextHeightPixels / (float)_texture.HeightPixels);
		_uv[0] = Vector2.zero;
		_uv[1] = new Vector2(vector.x, 0f);
		_uv[2] = new Vector2(0f, vector.y);
		_uv[3] = vector;
		UpdatePivot();
		UpdateScale();
	}

	protected void UpdatePivot()
	{
		if (_vertices == null)
		{
			_vertices = new Vector3[4];
			_vertices[0] = Vector3.zero;
			_vertices[1] = Vector3.zero;
			_vertices[2] = Vector3.zero;
			_vertices[3] = Vector3.zero;
		}
		if (_pivot == PivotAlignment.TopLeft || _pivot == PivotAlignment.Left || _pivot == PivotAlignment.BottomLeft)
		{
			_vertices[0].x = (_vertices[2].x = 0f);
			_vertices[1].x = (_vertices[3].x = 1f);
		}
		else if (_pivot == PivotAlignment.TopRight || _pivot == PivotAlignment.Right || _pivot == PivotAlignment.BottomRight)
		{
			_vertices[0].x = (_vertices[2].x = -1f);
			_vertices[1].x = (_vertices[3].x = 0f);
		}
		else
		{
			_vertices[0].x = (_vertices[2].x = -0.5f);
			_vertices[1].x = (_vertices[3].x = 0.5f);
		}
		if (_pivot == PivotAlignment.TopLeft || _pivot == PivotAlignment.Top || _pivot == PivotAlignment.TopRight)
		{
			_vertices[0].y = (_vertices[1].y = -1f);
			_vertices[2].y = (_vertices[3].y = 0f);
		}
		else if (_pivot == PivotAlignment.BottomLeft || _pivot == PivotAlignment.Bottom || _pivot == PivotAlignment.BottomRight)
		{
			_vertices[0].y = (_vertices[1].y = 0f);
			_vertices[2].y = (_vertices[3].y = 1f);
		}
		else
		{
			_vertices[0].y = (_vertices[1].y = -0.5f);
			_vertices[2].y = (_vertices[3].y = 0.5f);
		}
		if (_mesh == null)
		{
			_mesh = new Mesh();
			_mesh.name = "SysFontTextMesh";
			_mesh.hideFlags = HideFlags.DontSave;
		}
		_mesh.vertices = _vertices;
		_mesh.uv = _uv;
		_mesh.triangles = _triangles;
		_mesh.RecalculateBounds();
		_filter.mesh = _mesh;
		_lastPivot = _pivot;
	}

	public void UpdateScale()
	{
		Vector3 localScale = _transform.localScale;
		localScale.x = _texture.TextWidthPixels;
		localScale.y = _texture.TextHeightPixels;
		_transform.localScale = localScale;
	}

	protected virtual void Awake()
	{
		_transform = base.transform;
	}

	protected virtual void Update()
	{
		if (_texture.NeedsRedraw)
		{
			if (!_texture.Update())
			{
				return;
			}
			UpdateMesh();
			_material.mainTexture = Texture;
		}
		if (_texture.IsUpdated)
		{
			if (_fontColor != _lastFontColor && _material != null)
			{
				_material.color = _fontColor;
				_lastFontColor = _fontColor;
			}
			if (_lastPivot != _pivot)
			{
				UpdatePivot();
			}
		}
	}

	protected void OnDestroy()
	{
		if (_texture != null)
		{
			_texture.Destroy();
			_texture = null;
		}
		SysFont.SafeDestroy(_mesh);
		SysFont.SafeDestroy(_createdMaterial);
		_createdMaterial = null;
		_material = null;
		_vertices = null;
		_uv = null;
		_triangles = null;
		_mesh = null;
		_filter = null;
		_renderer = null;
	}
}
