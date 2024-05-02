using UnityEngine;

public class LocalisableText : MonoBehaviour
{
	private TextMesh _textMesh;

	private SysFontText _sysFontText;

	private UILabel _UILabel;

	private UISysFontLabel _UISysFontLabel;

	public bool _AlwaysUseSysFont;

	public string LocalisationKey;

	private bool ShouldUseSysFont
	{
		get
		{
			return Localization.instance.shouldUseSystemFont || _AlwaysUseSysFont;
		}
	}

	public string text
	{
		set
		{
			if (ShouldUseSysFont)
			{
				if (_sysFontText != null)
				{
					_sysFontText.Text = value;
				}
				else if (_UISysFontLabel != null)
				{
					_UISysFontLabel.Text = value;
				}
			}
			else if (_textMesh != null)
			{
				_textMesh.text = value;
			}
			else if (_UILabel != null)
			{
				_UILabel.text = value;
			}
			LocalisationKey = value;
		}
	}

	private void OnEnable()
	{
		Localise();
	}

	private void OnDisable()
	{
		HideText();
	}

	private void Localise()
	{
		FindText();
		if (!string.IsNullOrEmpty(LocalisationKey))
		{
			if (_textMesh != null || _sysFontText != null || _UISysFontLabel != null)
			{
				text = Localization.instance.GetFor3DText(LocalisationKey);
			}
			else
			{
				text = Localization.instance.Get(LocalisationKey);
			}
		}
		else if ((bool)_textMesh)
		{
			text = _textMesh.text;
		}
		else if ((bool)_UILabel)
		{
			text = _UILabel.text;
		}
		ShowText(ShouldUseSysFont);
	}

	private void FindText()
	{
		base.gameObject.SetActiveRecursively(true);
		_textMesh = GetComponentInChildren<TextMesh>();
		_sysFontText = GetComponentInChildren<SysFontText>();
		_UILabel = GetComponentInChildren<UILabel>();
		_UISysFontLabel = GetComponentInChildren<UISysFontLabel>();
	}

	private void ShowText(bool useSysFont)
	{
		if (_textMesh != null)
		{
			_textMesh.gameObject.active = !useSysFont || _sysFontText == null;
		}
		if (_sysFontText != null)
		{
			_sysFontText.gameObject.active = useSysFont || _textMesh == null;
		}
		if (_UILabel != null)
		{
			_UILabel.gameObject.active = !useSysFont || _UISysFontLabel == null;
		}
		if (_UISysFontLabel != null)
		{
			_UISysFontLabel.gameObject.active = useSysFont || _UILabel == null;
		}
	}

	private void HideText()
	{
		if (_textMesh != null)
		{
			_textMesh.gameObject.active = false;
		}
		if (_sysFontText != null)
		{
			_sysFontText.gameObject.active = false;
		}
		if (_UILabel != null)
		{
			_UILabel.gameObject.active = false;
		}
		if (_UISysFontLabel != null)
		{
			_UISysFontLabel.gameObject.active = false;
		}
	}

	private void Update()
	{
		if (_sysFontText != null && _textMesh != null && _sysFontText.gameObject.active && _textMesh.gameObject.active)
		{
			ShowText(ShouldUseSysFont);
		}
		else if (_UILabel != null && _UISysFontLabel != null && _UILabel.gameObject.active && _UISysFontLabel.gameObject.active)
		{
			ShowText(ShouldUseSysFont);
		}
	}

	public void HideSysFontText()
	{
		FindText();
		ShowText(false);
	}

	public void HideUnityText()
	{
		FindText();
		ShowText(true);
	}

	public void MarkAsChanged()
	{
		if (_UISysFontLabel != null)
		{
			_UISysFontLabel.MarkAsChanged();
		}
		if (_UILabel != null)
		{
			_UILabel.MarkAsChanged();
		}
	}

	public void Activate(bool activate)
	{
		if (!activate)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else
		{
			Localise();
		}
	}

	public void SetMaterial(Material material)
	{
		if (_textMesh != null)
		{
			_textMesh.renderer.material = material;
		}
	}
}
