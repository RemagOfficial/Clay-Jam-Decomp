using System;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class PaintVertices : MonoBehaviour
{
	public float radius;

	public float pull;

	private MeshFilter unappliedMesh;

	public FallOff fallOff;

	public PaintVertices()
	{
		radius = 1f;
		pull = 10f;
		fallOff = FallOff.Gauss;
	}

	public static float LinearFalloff(float distance, float inRadius)
	{
		return Mathf.Clamp01(1f - distance / inRadius);
	}

	public static float GaussFalloff(float distance, float inRadius)
	{
		return Mathf.Clamp01(Mathf.Pow(360f, 0f - Mathf.Pow(distance / inRadius, 2.5f) - 0.01f));
	}

	public virtual float NeedleFalloff(float dist, float inRadius)
	{
		return (0f - dist * dist) / (inRadius * inRadius) + 1f;
	}

	public virtual void DeformMesh(Mesh mesh, Vector3 position, float power, float inRadius)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		float num = inRadius * inRadius;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < Extensions.get_length((Array)vertices); i++)
		{
			float sqrMagnitude = (vertices[i] - position).sqrMagnitude;
			if (sqrMagnitude <= num)
			{
				float distance = Mathf.Sqrt(sqrMagnitude);
				float num2 = LinearFalloff(distance, inRadius);
				zero += num2 * normals[i];
			}
		}
		zero = zero.normalized;
		for (int i = 0; i < Extensions.get_length((Array)vertices); i++)
		{
			float sqrMagnitude = (vertices[i] - position).sqrMagnitude;
			if (sqrMagnitude <= num)
			{
				float distance = Mathf.Sqrt(sqrMagnitude);
				float num2;
				switch (fallOff)
				{
				case FallOff.Gauss:
					num2 = GaussFalloff(distance, inRadius);
					break;
				case FallOff.Needle:
					num2 = NeedleFalloff(distance, inRadius);
					break;
				default:
					num2 = LinearFalloff(distance, inRadius);
					break;
				}
				vertices[i] -= zero * num2 * power;
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	public virtual void Update()
	{
		if (!Input.GetMouseButton(0))
		{
			ApplyMeshCollider();
			return;
		}
		RaycastHit hitInfo = default(RaycastHit);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (!Physics.Raycast(ray, out hitInfo))
		{
			return;
		}
		MeshFilter meshFilter = (MeshFilter)hitInfo.collider.GetComponent(typeof(MeshFilter));
		if ((bool)meshFilter)
		{
			if (meshFilter != unappliedMesh)
			{
				ApplyMeshCollider();
				unappliedMesh = meshFilter;
				MonoBehaviour.print("NEW FILTER");
			}
			Vector3 position = meshFilter.transform.InverseTransformPoint(hitInfo.point);
			DeformMesh(meshFilter.mesh, position, pull * Time.deltaTime, radius);
		}
	}

	public virtual void ApplyMeshCollider()
	{
	}

	public virtual void Main()
	{
	}
}
