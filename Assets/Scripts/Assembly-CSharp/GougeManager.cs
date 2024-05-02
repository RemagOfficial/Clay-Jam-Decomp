using System.Collections.Generic;
using Fabric;
using UnityEngine;

public class GougeManager : MonoBehaviour
{
	private const int MaxGougesAtOnce = 3;

	private Queue<Gouge> _gouges = new Queue<Gouge>();

	public static GougeManager Instance { get; private set; }

	public int NumGouges
	{
		get
		{
			return _gouges.Count;
		}
	}

	private bool GougingAllowed { get; set; }

	private void Awake()
	{
		Instance = this;
		InGameController.StateChanged += OnInGameStateChanged;
		EnableGouging();
	}

	public Gouge StartNewGouge(Vector3 point, bool newInput)
	{
		if (!GougingAllowed)
		{
			return null;
		}
		Gouge gouge = CreateObject(point);
		gouge.Initialise(newInput);
		_gouges.Enqueue(gouge);
		if (_gouges.Count >= 3)
		{
			Gouge gouge2 = _gouges.Dequeue();
			gouge2.Die();
		}
		return gouge;
	}

	private Gouge CreateObject(Vector3 point)
	{
		GameObject gameObject = new GameObject("Gouge");
		Vector3 position = point;
		position.y = 0.05f;
		gameObject.transform.position = position;
		return gameObject.AddComponent<Gouge>();
	}

	public void RemoveGouge(Gouge gouge)
	{
	}

	private void OnInGameStateChanged(InGameController.State newState)
	{
		if (newState != InGameController.State.Flying && newState != InGameController.State.ResettingForRun)
		{
			return;
		}
		foreach (Gouge gouge in _gouges)
		{
			Object.Destroy(gouge.gameObject);
		}
		_gouges.Clear();
		InGameAudio.PostFabricEvent("Gouge", EventAction.StopSound);
		GougeSpline.NextSplineOrder = 0;
	}

	public void DisableGouging()
	{
		GougingAllowed = false;
	}

	public void EnableGouging()
	{
		GougingAllowed = true;
	}

	public bool AnyGougeFinished()
	{
		return _gouges.Peek().Finished;
	}
}
