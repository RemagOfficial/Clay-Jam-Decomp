using System.Collections.Generic;
using UnityEngine;

public class FlyingLandscapeLayer : MonoBehaviour
{
	private int NumSections;

	public float parallaxFactor;

	private Object _MountainSectionPrefab;

	private List<GameObject> _mountainSections;

	private int _firstSection;

	private int _lastSection;

	private int _midSection;

	private float prevPebbleZ;

	private ParticleEmitter[][] _emitters;

	public void Init(Object sectionPrefab, int sectionCount)
	{
		NumSections = sectionCount;
		_MountainSectionPrefab = sectionPrefab;
		LoadMountains();
		GetEmitters();
	}

	private void Start()
	{
	}

	private void Update()
	{
		Vector3 position = Pebble.Position;
		float num = (position.z - prevPebbleZ) * parallaxFactor;
		prevPebbleZ = position.z;
		for (int i = 0; i < _mountainSections.Count; i++)
		{
			Vector3 position2 = _mountainSections[i].transform.position;
			position2.z += num;
			_mountainSections[i].transform.position = position2;
		}
		float z = _mountainSections[_midSection].transform.position.z;
		z += 31.995f;
		if (position.z > z)
		{
			float z2 = _mountainSections[_lastSection].transform.position.z;
			z2 += 63.99f;
			PositionMountainSection(pos: new Vector3(position.x, 0f, z2), sectionIndex: _firstSection);
			_lastSection = _firstSection;
			_firstSection++;
			if (_firstSection == NumSections)
			{
				_firstSection = 0;
			}
			_midSection++;
			if (_midSection == NumSections)
			{
				_midSection = 0;
			}
		}
	}

	public void Reset()
	{
		ResetSections();
		prevPebbleZ = Pebble.Position.z;
	}

	private void LoadMountains()
	{
		_mountainSections = new List<GameObject>(NumSections);
		for (int i = 0; i < NumSections; i++)
		{
			GameObject item = LoadMountain();
			_mountainSections.Add(item);
		}
		_emitters = new ParticleEmitter[_mountainSections.Count][];
	}

	private GameObject LoadMountain()
	{
		GameObject gameObject = Object.Instantiate(_MountainSectionPrefab) as GameObject;
		gameObject.transform.parent = base.transform;
		return gameObject;
	}

	private void ResetSections()
	{
		_firstSection = 0;
		_lastSection = NumSections - 1;
		_midSection = _lastSection / 2;
		Vector3 position = Pebble.Position;
		Vector3 position2 = position;
		position2.y = 0f;
		for (int i = 0; i < NumSections; i++)
		{
			float num = (float)(i - _midSection) * 63.99f;
			position2.z = position.z + num;
			_mountainSections[i].transform.position = position2;
		}
	}

	private void GetEmitters()
	{
		for (int i = 0; i < _mountainSections.Count; i++)
		{
			_emitters[i] = _mountainSections[i].GetComponentsInChildren<ParticleEmitter>();
		}
	}

	private void PositionMountainSection(int sectionIndex, Vector3 pos)
	{
		_mountainSections[_firstSection].transform.position = pos;
		ParticleEmitter[] array = _emitters[sectionIndex];
		foreach (ParticleEmitter particleEmitter in array)
		{
			particleEmitter.Emit();
		}
	}
}
