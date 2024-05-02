using System.Collections.Generic;
using UnityEngine;

public class ColourDatabase : MonoBehaviour
{
	public static ColourDatabase Instance;

	public HSVColour _LockedIconColour;

	public HSVColour _JVPColour;

	public HSVColour _PowerPlayColour;

	public HSVColour _TrapSplatColour;

	public List<HSVColour> _CollectableColours;

	public static int NumCollectableColours
	{
		get
		{
			return Instance._CollectableColours.Count;
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("Second instance of ClayJamColour", base.gameObject);
		}
		Instance = this;
	}
}
