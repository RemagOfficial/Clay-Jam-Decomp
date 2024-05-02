using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[AddComponentMenu("NGUI/Internal/Localization")]
public class Localization : MonoBehaviour
{
	private static Localization mInstance;

	private static char Interpunct = '\u00a0';

	public string[] languagesWhichUseSystemFont;

	[NonSerialized]
	public bool shouldUseSystemFont;

	public string startingLanguage;

	public string debugLanguageOverride;

	public TextAsset[] languages;

	public LanguageFileMap[] languageMap;

	private Dictionary<string, string> mDictionary = new Dictionary<string, string>();

	private string mLanguage;

	public string _Chris;

	public static bool isActive
	{
		get
		{
			return mInstance != null;
		}
	}

	public static Localization instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = UnityEngine.Object.FindObjectOfType(typeof(Localization)) as Localization;
				if (mInstance == null)
				{
					GameObject gameObject = new GameObject("_Localization");
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
					mInstance = gameObject.AddComponent<Localization>();
				}
			}
			return mInstance;
		}
	}

	public string currentLanguage
	{
		get
		{
			if (string.IsNullOrEmpty(mLanguage) && string.IsNullOrEmpty(mLanguage))
			{
				Debug.LogError("Getting Language before its been set. Call Localization.instance.Init() before now");
			}
			return mLanguage;
		}
		set
		{
			if (!(mLanguage != value))
			{
				return;
			}
			for (int i = 0; i < languagesWhichUseSystemFont.Length; i++)
			{
				if (value == languagesWhichUseSystemFont[i])
				{
					shouldUseSystemFont = true;
					break;
				}
			}
			if (!string.IsNullOrEmpty(value))
			{
				if (languages != null)
				{
					int j = 0;
					for (int num = languages.Length; j < num; j++)
					{
						TextAsset textAsset = languages[j];
						if (textAsset != null && textAsset.name == value)
						{
							Load(textAsset);
							return;
						}
					}
				}
				TextAsset textAsset2 = Resources.Load(value, typeof(TextAsset)) as TextAsset;
				if (textAsset2 != null)
				{
					Load(textAsset2);
					return;
				}
			}
			mDictionary.Clear();
			PlayerPrefs.DeleteKey("Language");
		}
	}

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
	}

	private void OnDestroy()
	{
		if (mInstance == this)
		{
			mInstance = null;
		}
	}

	private void Load(TextAsset asset)
	{
		mLanguage = asset.name;
		PlayerPrefs.SetString("Language", mLanguage);
		ByteReader byteReader = new ByteReader(asset);
		mDictionary = byteReader.ReadDictionary();
		UIRoot.Broadcast("OnLocalize", this);
	}

	public string Get(string key)
	{
		string value;
		if (mDictionary.TryGetValue(key + " Mobile", out value))
		{
			return value;
		}
		return (!mDictionary.TryGetValue(key, out value)) ? key : value;
	}

	public static string Localize(string key)
	{
		return (!(instance != null)) ? key : instance.Get(key);
	}

	public string GetFor3DText(string key)
	{
		string text = Get(key);
		return text.Replace("\\n", "\n");
	}

	public string GetFor3DText(string key, object param1)
	{
		string for3DText = GetFor3DText(key);
		return for3DText.Replace("{0}", param1.ToString());
	}

	public string GetFor3DText(string key, object param1, object param2)
	{
		string for3DText = GetFor3DText(key);
		for3DText = for3DText.Replace("{0}", param1.ToString());
		return for3DText.Replace("{1}", param2.ToString());
	}

	public string GetFor3DText(string key, object param1, object param2, object param3)
	{
		string for3DText = GetFor3DText(key);
		for3DText = for3DText.Replace("{0}", param1.ToString());
		for3DText = for3DText.Replace("{1}", param2.ToString());
		return for3DText.Replace("{2}", param3.ToString());
	}

	private string GetLanguageFileFromSystemLanguage()
	{
		if (!string.IsNullOrEmpty(debugLanguageOverride))
		{
			Debug.Log(string.Format("Debug language {0} used instead of system language {1}", debugLanguageOverride, Application.systemLanguage));
			return debugLanguageOverride;
		}
		SystemLanguage systemLanguage = Application.systemLanguage;
		Predicate<LanguageFileMap> match = (LanguageFileMap lfm) => lfm.Languages.Contains(systemLanguage);
		LanguageFileMap languageFileMap = Array.Find(languageMap, match);
		if (languageFileMap != null)
		{
			string text = languageFileMap.TextFile.name;
			Debug.Log(string.Format("Using {0} language file for system language {1}", text, systemLanguage));
			return text;
		}
		Debug.Log(string.Format("No language file supports system language {0} so using {1}", systemLanguage, startingLanguage));
		return startingLanguage;
	}

	public void Init()
	{
		currentLanguage = GetLanguageFileFromSystemLanguage();
	}

	public static string PunctuatedNumber(float val, int limit = 9999999)
	{
		CultureInfo provider = new CultureInfo(instance.currentLanguage);
		return (!(val < (float)limit)) ? string.Format("{0}{1}", limit, "+") : string.Format(provider, "{0:N0}", val).Replace(Interpunct.ToString(), ". ");
	}
}
