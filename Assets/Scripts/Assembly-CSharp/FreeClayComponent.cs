using System.Collections;
using UnityEngine;

public class FreeClayComponent : MonoBehaviour
{
	private const string ServerTransInAnim = "ServerPanelTransIn";

	private const string ServerTransOutAnim = "ServerPanelTransOut";

	public Animation PanelAnim;

	public GameObject Panel;

	public LocalisableText _PurchasedCompleteText;

	private bool _isHiding;

	public static FreeClayComponent Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More tahn one FreeClayComponent", base.gameObject);
		}
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void Start()
	{
		Debug.Log("FreeClayComponent Start");
		Panel.SetActive(false);
	}

	public void ShowRewardMessagePanel(int clayAmount)
	{
		string arg = Localization.PunctuatedNumber(clayAmount, int.MaxValue);
		_PurchasedCompleteText.text = string.Format(Localization.instance.Get("IAP_complete"), arg);
		_PurchasedCompleteText.Activate(true);
		Panel.SetActive(true);
		PanelAnim.Play("ServerPanelTransIn");
		_isHiding = false;
		StartCoroutine(WaitForAnimateOut());
	}

	public void HidePanel()
	{
		if (!_isHiding)
		{
			_isHiding = true;
			PanelAnim.Play("ServerPanelTransOut");
		}
	}

	public IEnumerator WaitForAnimateOut()
	{
		yield return new WaitForSeconds(2f);
		HidePanel();
	}
}
