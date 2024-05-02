using UnityEngine;

[RequireComponent(typeof(UIPanel))]
public class UIIsLoadingLabel : MonoBehaviour
{
	public bool _ActivateOnLoading;

	protected UIPanel _panel;

	private void OnEnable()
	{
		_panel = GetComponent<UIPanel>();
		if (_panel == null)
		{
			Debug.Log("UIIsLoadingLabel needs to be on an object with a UIPanel");
		}
		if (_ActivateOnLoading && InGameController.Instance.IsLoading)
		{
			_panel.enabled = true;
		}
		else if (!_ActivateOnLoading && !InGameController.Instance.IsLoading)
		{
			_panel.enabled = true;
		}
		else
		{
			_panel.enabled = false;
		}
	}

	private void Update()
	{
		if (_ActivateOnLoading && InGameController.Instance.IsLoading)
		{
			_panel.enabled = true;
		}
		else if (!_ActivateOnLoading && !InGameController.Instance.IsLoading)
		{
			_panel.enabled = true;
		}
		else
		{
			_panel.enabled = false;
		}
	}
}
