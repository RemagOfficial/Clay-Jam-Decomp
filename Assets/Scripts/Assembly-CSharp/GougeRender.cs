using System;
using System.Collections.Generic;
using UnityEngine;

public class GougeRender : MonoBehaviour
{
	private static string _gougeMidResourceName = "GougeModels/GougeMid2";

	private static string _gougeEndResourceName = "GougeModels/GougeEnd";

	private static string _gougeMaterialName = "GougeMaterials/Gouge";

	private static GougeRenderMesh _gougeMeshMid;

	private static GougeRenderMesh _gougeMeshEnd;

	private static int _numEdgeVerts;

	private static int[] _edgeVertsMidBottom;

	private static int[] _edgeVertsMidTop;

	private static int[] _edgeVertsEnd;

	private static int _vertsPerEndMesh;

	private static int _vertsPerMidMesh;

	private static int _triangleIndiciesPerMidMesh;

	private static int _triangleIndiciesPerEndMesh;

	private static Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 50f);

	private static int[] _endSectionIndexMap;

	private static int[] _midSectionIndexMap;

	private int[] _currentEdgeVerts;

	private int[] _newEdgeVerts;

	private Vector3[] _vertArray;

	private Vector3[] _normalArray;

	private Vector2[] _uvArray;

	private int[] _triArray;

	private Mesh _unityMesh;

	private int _nextSampleToRender;

	private bool _hasEndPiece;

	private bool _wasUsingFakeFirstPoint;

	public GougeSpline Spline { get; set; }

	private static GougeRenderMesh Mesh { get; set; }

	private void Awake()
	{
		AddMeshComponent();
		CreatePerGougeData();
	}

	private void CreatePerGougeData()
	{
		_currentEdgeVerts = new int[_numEdgeVerts];
		_newEdgeVerts = new int[_numEdgeVerts];
		_vertArray = new Vector3[376];
		_normalArray = new Vector3[376];
		_uvArray = new Vector2[376];
		_triArray = new int[1926];
	}

	private void AddMeshComponent()
	{
		MeshRenderer meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load(_gougeMaterialName) as Material;
		MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
		_unityMesh = meshFilter.mesh;
	}

	private void ClearMesh()
	{
		_hasEndPiece = false;
		_nextSampleToRender = 0;
		Mesh.Clear();
	}

	private void RemoveEndPieceFromMesh()
	{
		int num = _vertsPerEndMesh - _numEdgeVerts;
		Mesh.Verts.RemoveRange(Mesh.Verts.Count - num, num);
		Mesh.Normals.RemoveRange(Mesh.Normals.Count - num, num);
		Mesh.UVs.RemoveRange(Mesh.UVs.Count - num, num);
		Mesh.Triangles.RemoveRange(Mesh.Triangles.Count - _triangleIndiciesPerEndMesh, _triangleIndiciesPerEndMesh);
	}

	private void ApplyMesh()
	{
		if (Mesh.Verts.Count > _vertArray.Length)
		{
			Debug.LogWarning("Incomplete Gouge due to not enough verts");
			int num = Mesh.Verts.Count - _vertArray.Length;
			Mesh.Verts.RemoveRange(Mesh.Verts.Count - num, num);
			Mesh.Normals.RemoveRange(Mesh.Verts.Count - num, num);
			Mesh.UVs.RemoveRange(Mesh.Verts.Count - num, num);
		}
		if (Mesh.Triangles.Count > _triArray.Length)
		{
			Debug.LogWarning("Incomplete Gouge due to not enough tris");
			int num2 = Mesh.Triangles.Count - _triArray.Length;
			Mesh.Triangles.RemoveRange(Mesh.Triangles.Count - num2, num2);
		}
		Mesh.Verts.CopyTo(_vertArray);
		Mesh.Normals.CopyTo(_normalArray);
		Mesh.UVs.CopyTo(_uvArray);
		Mesh.Triangles.CopyTo(_triArray);
		_unityMesh.vertices = _vertArray;
		_unityMesh.normals = _normalArray;
		_unityMesh.uv = _uvArray;
		_unityMesh.triangles = _triArray;
		_unityMesh.bounds = _bounds;
	}

	private void AddFirstEndMesh(Vector3 offset, Quaternion rotation, Vector3 scale, bool flat)
	{
		int count = Mesh.Verts.Count;
		for (int i = 0; i < _vertsPerEndMesh; i++)
		{
			Vector3 a = _gougeMeshEnd.Verts[i];
			a = Vector3.Scale(a, scale);
			a = rotation * a;
			a += offset;
			Mesh.Verts.Add(a);
			Vector3 item = Vector3.up;
			if (!flat)
			{
				item = _gougeMeshEnd.Normals[i];
				item = rotation * item;
			}
			Mesh.Normals.Add(item);
			Mesh.UVs.Add(_gougeMeshEnd.UVs[i]);
		}
		for (int j = 0; j < _triangleIndiciesPerEndMesh; j++)
		{
			Mesh.Triangles.Add(count + _gougeMeshEnd.Triangles[j]);
		}
		for (int k = 0; k < _numEdgeVerts; k++)
		{
			_currentEdgeVerts[k] = count + _edgeVertsEnd[k];
		}
	}

	private void AddLastEndMesh(Vector3 offset, Quaternion rotation, Vector3 scale, bool flat)
	{
		for (int i = 0; i < _vertsPerEndMesh; i++)
		{
			int num = -1;
			for (int j = 0; j < _numEdgeVerts; j++)
			{
				if (i == _edgeVertsEnd[j])
				{
					num = j;
				}
			}
			if (num >= 0)
			{
				int num2 = _currentEdgeVerts[num];
				_endSectionIndexMap[i] = num2;
				Vector3 vector = Mesh.Verts[num2];
				Vector3 a = _gougeMeshEnd.Verts[i];
				a = Vector3.Scale(a, scale);
				a = rotation * a;
				a += offset;
				Vector3 value = (vector + a) * 0.5f;
				Mesh.Verts[num2] = value;
				Vector3 vector2 = Mesh.Normals[num2];
				Vector3 vector3 = Vector3.up;
				if (!flat)
				{
					vector3 = _gougeMeshEnd.Normals[i];
					vector3 = rotation * vector3;
				}
				Vector3 value2 = vector2 + vector3;
				value2 *= 0.5f;
				Mesh.Normals[num2] = value2;
			}
			else
			{
				_endSectionIndexMap[i] = Mesh.Verts.Count;
				Vector3 a2 = _gougeMeshEnd.Verts[i];
				a2.z *= -1f;
				a2 = Vector3.Scale(a2, scale);
				a2 = rotation * a2;
				a2 += offset;
				Mesh.Verts.Add(a2);
				Vector3 item = Vector3.up;
				if (!flat)
				{
					item = _gougeMeshEnd.Normals[i];
					item = rotation * item;
				}
				Mesh.Normals.Add(item);
				Mesh.UVs.Add(_gougeMeshEnd.UVs[i]);
			}
		}
		int num3 = _triangleIndiciesPerEndMesh / 3;
		for (int k = 0; k < num3; k++)
		{
			int num4 = _gougeMeshEnd.Triangles[k * 3];
			int num5 = _gougeMeshEnd.Triangles[k * 3 + 1];
			int num6 = _gougeMeshEnd.Triangles[k * 3 + 2];
			num4 = _endSectionIndexMap[num4];
			num5 = _endSectionIndexMap[num5];
			num6 = _endSectionIndexMap[num6];
			int num7 = num6;
			num6 = num4;
			num4 = num7;
			Mesh.Triangles.Add(num4);
			Mesh.Triangles.Add(num5);
			Mesh.Triangles.Add(num6);
		}
	}

	private void StitchInNewMidPoint(Vector3 offset, Quaternion rotation, Vector3 scale, bool flat)
	{
		for (int i = 0; i < _vertsPerMidMesh; i++)
		{
			int num = -1;
			for (int j = 0; j < _numEdgeVerts; j++)
			{
				if (i == _edgeVertsMidBottom[j])
				{
					num = j;
				}
			}
			if (num >= 0)
			{
				int num2 = _currentEdgeVerts[num];
				_midSectionIndexMap[i] = num2;
				Vector3 vector = Mesh.Verts[num2];
				Vector3 a = _gougeMeshMid.Verts[i];
				a = Vector3.Scale(a, scale);
				a = rotation * a;
				a += offset;
				Vector3 value = (vector + a) * 0.5f;
				Mesh.Verts[num2] = value;
				Vector3 vector2 = Mesh.Normals[num2];
				Vector3 vector3 = Vector3.up;
				if (!flat)
				{
					vector3 = _gougeMeshMid.Normals[i];
					vector3 = rotation * vector3;
				}
				Vector3 value2 = vector2 + vector3;
				value2 *= 0.5f;
				Mesh.Normals[num2] = value2;
				continue;
			}
			int count = Mesh.Verts.Count;
			_midSectionIndexMap[i] = count;
			for (int k = 0; k < _numEdgeVerts; k++)
			{
				if (i == _edgeVertsMidTop[k])
				{
					_newEdgeVerts[k] = count;
				}
			}
			Vector3 a2 = _gougeMeshMid.Verts[i];
			a2 = Vector3.Scale(a2, scale);
			a2 = rotation * a2;
			a2 += offset;
			Mesh.Verts.Add(a2);
			Vector3 item = Vector3.up;
			if (!flat)
			{
				item = _gougeMeshMid.Normals[i];
				item = rotation * item;
			}
			Mesh.Normals.Add(item);
			Mesh.UVs.Add(_gougeMeshMid.UVs[i]);
		}
		for (int l = 0; l < _triangleIndiciesPerMidMesh; l++)
		{
			int num3 = _gougeMeshMid.Triangles[l];
			num3 = _midSectionIndexMap[num3];
			Mesh.Triangles.Add(num3);
		}
		for (int m = 0; m < _numEdgeVerts; m++)
		{
			_currentEdgeVerts[m] = _newEdgeVerts[m];
		}
	}

	private void CreateMeshFromSpline()
	{
		if (_hasEndPiece)
		{
			RemoveEndPieceFromMesh();
		}
		GougeSplineSample nextSampleData;
		while (_nextSampleToRender < Spline.NumSamples)
		{
			nextSampleData = Spline.GetNextSampleData(_nextSampleToRender);
			if (_nextSampleToRender == 0)
			{
				AddFirstEndMesh(nextSampleData.Position, nextSampleData.Rotation, nextSampleData.Scale, nextSampleData.Flatten);
			}
			else
			{
				StitchInNewMidPoint(nextSampleData.Position, nextSampleData.Rotation, nextSampleData.Scale, nextSampleData.Flatten);
			}
			_nextSampleToRender++;
		}
		nextSampleData = Spline.EndSample;
		AddLastEndMesh(nextSampleData.Position, nextSampleData.Rotation, nextSampleData.Scale, nextSampleData.Flatten);
		_hasEndPiece = true;
		ApplyMesh();
	}

	private void Update()
	{
		if (Spline.SamplesReady && Spline.NumSamples > _nextSampleToRender)
		{
			bool flag = _wasUsingFakeFirstPoint && !Spline.UsingFakeFirstPoint;
			_wasUsingFakeFirstPoint = Spline.UsingFakeFirstPoint;
			if (_nextSampleToRender == 0 || flag)
			{
				ClearMesh();
			}
			CreateMeshFromSpline();
		}
	}

	public static void LoadMeshes()
	{
		if (_gougeMeshEnd != null)
		{
			Debug.LogError("GougeRenderer meshes loaded twice");
			return;
		}
		LoadMidMesh();
		LoadEndMesh();
		CreateHelperArrays();
	}

	private static void CreateHelperArrays()
	{
		Mesh = new GougeRenderMesh();
		_endSectionIndexMap = new int[_vertsPerEndMesh];
		_midSectionIndexMap = new int[_vertsPerMidMesh];
	}

	private static void LoadMidMesh()
	{
		GameObject gameObject = Resources.Load(_gougeMidResourceName) as GameObject;
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		_gougeMeshMid = new GougeRenderMesh(component.mesh);
		_vertsPerMidMesh = _gougeMeshMid.VertexCount;
		_triangleIndiciesPerMidMesh = _gougeMeshMid.TriangleCount;
		float num = float.MinValue;
		float num2 = float.MaxValue;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < _vertsPerMidMesh; i++)
		{
			float z = _gougeMeshMid.Verts[i].z;
			if (z > num)
			{
				num = z;
				num3 = 1;
			}
			else if (z == num)
			{
				num3++;
			}
			if (z < num2)
			{
				num2 = z;
				num4 = 1;
			}
			else if (z == num2)
			{
				num4++;
			}
		}
		if (num4 != num3)
		{
			Debug.LogError("Bad mid section gouge mesh. Must have matching vertex counts along edges to be stitched");
		}
		_numEdgeVerts = num4;
		_edgeVertsMidBottom = new int[_numEdgeVerts];
		_edgeVertsMidTop = new int[_numEdgeVerts];
		float[] array = new float[_numEdgeVerts];
		float[] array2 = new float[_numEdgeVerts];
		int num5 = 0;
		int num6 = 0;
		for (int j = 0; j < _vertsPerMidMesh; j++)
		{
			if (_gougeMeshMid.Verts[j].z == num2)
			{
				_edgeVertsMidBottom[num5] = j;
				array[num5] = _gougeMeshMid.Verts[j].x;
				num5++;
			}
			if (_gougeMeshMid.Verts[j].z == num)
			{
				_edgeVertsMidTop[num6] = j;
				array2[num6] = _gougeMeshMid.Verts[j].x;
				num6++;
			}
		}
		Array.Sort(_edgeVertsMidBottom, CompareMidMeshVertByX);
		Array.Sort(_edgeVertsMidTop, CompareMidMeshVertByX);
	}

	private static void LoadEndMesh()
	{
		GameObject gameObject = Resources.Load(_gougeEndResourceName) as GameObject;
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		_gougeMeshEnd = new GougeRenderMesh(component.mesh);
		_vertsPerEndMesh = _gougeMeshEnd.VertexCount;
		_triangleIndiciesPerEndMesh = _gougeMeshEnd.TriangleCount;
		float num = float.MinValue;
		int num2 = 0;
		for (int i = 0; i < _vertsPerEndMesh; i++)
		{
			float z = _gougeMeshEnd.Verts[i].z;
			if (z > num)
			{
				num = z;
				num2 = 1;
			}
			else if (z == num)
			{
				num2++;
			}
		}
		if (num2 != _numEdgeVerts)
		{
			Debug.LogError(string.Format("Bad end section gouge mesh. Must have matching edge vertex count with mid section (end:{0} != mid:{1})", num2, _numEdgeVerts));
		}
		_edgeVertsEnd = new int[num2];
		float[] array = new float[num2];
		int num3 = 0;
		for (int j = 0; j < _vertsPerEndMesh; j++)
		{
			if (_gougeMeshEnd.Verts[j].z == num)
			{
				_edgeVertsEnd[num3] = j;
				array[num3] = _gougeMeshEnd.Verts[j].x;
				num3++;
			}
		}
		Array.Sort(_edgeVertsEnd, CompareEndMeshVertByX);
	}

	private static int CompareMidMeshVertByX(int v1, int v2)
	{
		float x = _gougeMeshMid.Verts[v1].x;
		float x2 = _gougeMeshMid.Verts[v2].x;
		return Comparer<float>.Default.Compare(x, x2);
	}

	private static int CompareEndMeshVertByX(int v1, int v2)
	{
		float x = _gougeMeshEnd.Verts[v1].x;
		float x2 = _gougeMeshEnd.Verts[v2].x;
		return Comparer<float>.Default.Compare(x, x2);
	}

	public void Fade(float _normalisedDieTime)
	{
		float num = _normalisedDieTime * _normalisedDieTime;
		float num2 = 1f - num;
		for (int i = 0; i < _normalArray.Length; i++)
		{
			_normalArray[i] = Vector3.up * num + _normalArray[i] * num2;
		}
		_unityMesh.normals = _normalArray;
	}
}
