using System.Collections.Generic;
using UnityEngine;

public class ComponentManager : MonoBehaviour
{
	private List<ManagedComponent> _Components = new List<ManagedComponent>();

	public bool Initialised { get; private set; }

	protected virtual void Awake()
	{
		GetComponents();
		Initialised = false;
	}

	public bool Initialise()
	{
		foreach (ManagedComponent component in _Components)
		{
			if (!component.Initialised)
			{
				component.Initialise();
				return false;
			}
		}
		Initialised = true;
		return true;
	}

	public void ResetForRun()
	{
		foreach (ManagedComponent component in _Components)
		{
			component.ResetForRun();
		}
	}

	public void Run()
	{
		if (!Initialised)
		{
			Debug.LogError("Run called early on ComponentManger - Make sure Initialse() returns true first");
			return;
		}
		foreach (ManagedComponent component in _Components)
		{
			component.Run();
		}
	}

	public void Unload()
	{
		Object.Destroy(base.transform.gameObject);
	}

	public void CloseDown()
	{
		foreach (ManagedComponent component in _Components)
		{
			component.CloseDown();
		}
	}

	private void GetComponents()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			ManagedComponent component = transform.GetComponent<ManagedComponent>();
			if (!(component == null))
			{
				_Components.Add(component);
			}
		}
	}
}
