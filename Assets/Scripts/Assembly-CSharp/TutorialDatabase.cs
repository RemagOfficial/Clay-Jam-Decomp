using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TutorialDatabase : MonoBehaviour
{
	public static TutorialDatabase Instance { get; private set; }

	public List<Tutorial> Tutorials { get; private set; }

	[method: MethodImpl(32)]
	public static event Action ScreenTapped;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one TutirialDatabse instance", base.gameObject);
		}
		Instance = this;
		CreateList();
		if ((bool)SaveData.Instance && SaveData.Instance.Loaded)
		{
			Reset();
		}
		else
		{
			SaveData.LoadEvent += OnSaveDataLoaded;
		}
		SaveData.SaveEvent += OnSaveDataChanged;
	}

	private void OnDisable()
	{
		SaveData.SaveEvent -= OnSaveDataChanged;
	}

	private void Update()
	{
		if (TutorialDatabase.ScreenTapped != null && ClayJamInput.AnythingPressed)
		{
			TutorialDatabase.ScreenTapped();
		}
	}

	public Tutorial GetTutorial(string tutorialName)
	{
		return Tutorials.Find((Tutorial t) => t.name == tutorialName);
	}

	private void CreateList()
	{
		Tutorial[] componentsInChildren = GetComponentsInChildren<Tutorial>();
		Tutorials = new List<Tutorial>(componentsInChildren.Length);
		Tutorial[] array = componentsInChildren;
		foreach (Tutorial item in array)
		{
			Tutorials.Add(item);
		}
	}

	private void Reset()
	{
		foreach (Tutorial tutorial in Tutorials)
		{
			tutorial.Reset();
		}
	}

	private void OnSaveDataLoaded()
	{
		SaveData.LoadEvent -= OnSaveDataLoaded;
		Reset();
	}

	private void OnSaveDataChanged()
	{
		Reset();
	}
}
