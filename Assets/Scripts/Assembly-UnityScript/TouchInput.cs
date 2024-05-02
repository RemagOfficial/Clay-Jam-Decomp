using System;
using UnityEngine;

[Serializable]
public class TouchInput : MonoBehaviour
{
	public virtual void DeformMesh(RaycastHit hit_info)
	{
		MeshFilter meshFilter = null;
		Mesh mesh = null;
		Vector3[] array = null;
		Vector3 vector = default(Vector3);
		int[] array2 = null;
		Transform transform = null;
		Vector3 vector2 = default(Vector3);
		Vector3 vector3 = default(Vector3);
		Vector3 vector4 = default(Vector3);
		meshFilter = (MeshFilter)hit_info.collider.GetComponent(typeof(MeshFilter));
		mesh = meshFilter.mesh;
		array = mesh.vertices;
		vector = hit_info.normal;
		array2 = mesh.triangles;
		vector2 = array[array2[hit_info.triangleIndex * 3 + 0]];
		vector3 = array[array2[hit_info.triangleIndex * 3 + 1]];
		vector4 = array[array2[hit_info.triangleIndex * 3 + 2]];
		transform = hit_info.collider.transform;
		vector2 = transform.TransformPoint(vector2);
		vector3 = transform.TransformPoint(vector3);
		vector4 = transform.TransformPoint(vector4);
		vector2 -= vector * 0.02f;
		vector3 -= vector * 0.02f;
		vector4 -= vector * 0.02f;
		vector2 = transform.InverseTransformPoint(vector2);
		vector3 = transform.InverseTransformPoint(vector3);
		vector4 = transform.InverseTransformPoint(vector4);
		array[array2[hit_info.triangleIndex * 3 + 0]] = vector2;
		array[array2[hit_info.triangleIndex * 3 + 1]] = vector3;
		array[array2[hit_info.triangleIndex * 3 + 2]] = vector4;
		mesh.vertices = array;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	public virtual void Update()
	{
		Ray ray = default(Ray);
		RaycastHit hitInfo = default(RaycastHit);
		if (Input.multiTouchEnabled)
		{
			int i = 0;
			Touch[] touches = Input.touches;
			for (int length = touches.Length; i < length; i++)
			{
				ray = Camera.main.ScreenPointToRay(touches[i].position);
				if (Physics.Raycast(ray, out hitInfo) && hitInfo.collider.gameObject.name == "Ground")
				{
					DeformMesh(hitInfo);
				}
			}
		}
		else if (Input.GetMouseButton(0))
		{
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hitInfo) && hitInfo.collider.gameObject.name == "Ground")
			{
				DeformMesh(hitInfo);
			}
		}
	}

	public virtual void Main()
	{
	}
}
