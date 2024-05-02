using UnityEngine;

public class ManagedComponent : MonoBehaviour
{
	public bool Initialised { get; private set; }

	private void Awake()
	{
		Initialised = false;
		base.enabled = false;
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	public void CloseDown()
	{
		base.enabled = false;
	}

	public void Initialise()
	{
		Initialised = DoInitialise();
	}

	protected virtual bool DoInitialise()
	{
		return true;
	}

	public virtual void ResetForRun()
	{
	}

	public void Run()
	{
		base.enabled = true;
		OnRunStarted();
	}

	protected virtual void OnRunStarted()
	{
	}
}
