using System.Text;
using UnityEngine;

public class BuildDetails : MonoBehaviour
{
	public static BuildDetails Instance;

	public string _VersionNumber = "1.0";

	public bool _UseLeapIfAvailable = true;

	public bool _OnlyUseLeap = true;

	public bool _DemoMode;

	public bool _IsAmazonAppStore;

	public bool _UsingPCAssets;

	public AnalyticsController.Config _Analytics;

	public bool _HasRewardedAds;

	public IAPGlobal.IAPProvider IAPProvider = IAPGlobal.IAPProvider.DEBUG;

	[HideInInspector]
	public string _CurrentSubBuild;

	[HideInInspector]
	public string[] _HillScenes = new string[5] { "hill1", "hill2", "hill3", "hill4", "hill5" };

	public string Description { get; private set; }

	public bool _HasIAP
	{
		get
		{
			return IAPProvider != IAPGlobal.IAPProvider.NONE;
		}
	}

	public string GlobalSceneName
	{
		get
		{
			return (!_UsingPCAssets) ? "Global" : "Global_PC";
		}
	}

	public string JVPSceneName
	{
		get
		{
			if (_UsingPCAssets)
			{
				return (!_DemoMode) ? "JVP_PC" : "JVP_PC_Demo";
			}
			return "JVP";
		}
	}

	public string FrontendSceneName
	{
		get
		{
			return (!_UsingPCAssets) ? "Frontend" : "Frontend_PC";
		}
	}

	public string HillGenericSceneName
	{
		get
		{
			if (_UsingPCAssets)
			{
				return (!_DemoMode) ? "hillgeneric_PC" : "hillgeneric_PC_Demo";
			}
			return "hillgeneric";
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one BuildDetails instance", base.gameObject);
		}
		Instance = this;
	}

	private void Start()
	{
		SetDescription();
		Debug.Log(string.Format("Running build {0}", Description));
	}

	public void SetDescription()
	{
		StringBuilder stringBuilder = new StringBuilder(128);
		stringBuilder.Append(_CurrentSubBuild);
		stringBuilder.Append("_V");
		stringBuilder.Append(_VersionNumber);
		if (Application.genuineCheckAvailable)
		{
			if (Application.genuine)
			{
				stringBuilder.Append("_Gt");
			}
			else
			{
				stringBuilder.Append("_Gf");
			}
		}
		Description = stringBuilder.ToString();
	}
}
