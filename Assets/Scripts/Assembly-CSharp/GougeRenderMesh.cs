using System.Collections.Generic;
using UnityEngine;

public class GougeRenderMesh
{
	public const int MaxVerts = 376;

	public const int MaxTrinagles = 1926;

	public List<Vector3> Verts { get; set; }

	public List<Vector3> Normals { get; set; }

	public List<Vector2> UVs { get; set; }

	public List<int> Triangles { get; set; }

	public int VertexCount
	{
		get
		{
			return Verts.Count;
		}
	}

	public int TriangleCount
	{
		get
		{
			return Triangles.Count;
		}
	}

	public GougeRenderMesh()
	{
		Verts = new List<Vector3>(376);
		Normals = new List<Vector3>(376);
		UVs = new List<Vector2>(376);
		Triangles = new List<int>(1926);
	}

	public GougeRenderMesh(Mesh mesh)
	{
		Verts = new List<Vector3>(mesh.vertexCount);
		Normals = new List<Vector3>(mesh.vertexCount);
		UVs = new List<Vector2>(mesh.vertexCount);
		Triangles = new List<int>(mesh.triangles.Length);
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			Verts.Add(mesh.vertices[i]);
			UVs.Add(mesh.uv[i]);
			Normals.Add(mesh.normals[i]);
		}
		for (int j = 0; j < mesh.triangles.Length; j++)
		{
			Triangles.Add(mesh.triangles[j]);
		}
	}

	public void Clear()
	{
		Verts.Clear();
		Normals.Clear();
		UVs.Clear();
		Triangles.Clear();
	}
}
