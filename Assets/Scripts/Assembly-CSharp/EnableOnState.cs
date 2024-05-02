using System.Collections.Generic;
using UnityEngine;

public class EnableOnState : MonoBehaviour
{
	public List<InGameController.State> _EnabledStates = new List<InGameController.State>();

	private void Awake()
	{
		InGameController.StateChanged += OnStateChanged;
	}

	private void OnDestroy()
	{
		InGameController.StateChanged -= OnStateChanged;
	}

	private void OnStateChanged(InGameController.State state)
	{
		base.gameObject.SetActiveRecursively(_EnabledStates.Exists((InGameController.State x) => x == state));
	}
}
