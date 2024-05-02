using System.Collections.Generic;
using UnityEngine;

public class InGameAssets : ManagedComponent
{
	public Object _PebbelPrefab;

	private Pebble _pebble;

	private List<GameObject> _loadedObjects;

	protected override void OnAwake()
	{
		LoadEverything();
	}

	protected void OnDestroy()
	{
		UnloadEverything();
	}

	private void UnloadEverything()
	{
		foreach (GameObject loadedObject in _loadedObjects)
		{
			Object.Destroy(loadedObject);
		}
	}

	public override void ResetForRun()
	{
	}

	protected override void OnRunStarted()
	{
		_pebble.StartRun();
	}

	private void LoadEverything()
	{
		_loadedObjects = new List<GameObject>();
		_pebble = LoadToComponent<Pebble>(_PebbelPrefab);
	}

	private T LoadToComponent<T>(Object prefab) where T : Component
	{
		GameObject gameObject = Load(prefab);
		T component = gameObject.GetComponent<T>();
		if (component == null)
		{
			Debug.LogError(string.Format("{0} is missinf vital component", prefab.name), gameObject);
		}
		return component;
	}

	private GameObject Load(Object prefab)
	{
		GameObject gameObject = Object.Instantiate(prefab) as GameObject;
		gameObject.name = prefab.name;
		_loadedObjects.Add(gameObject);
		return gameObject;
	}
}
