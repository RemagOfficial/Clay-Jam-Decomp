using UnityEngine;

public static class MeshCreation
{
	public static void CreateSingleQuad(Mesh mesh, float width, float length)
	{
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] array3 = new int[6];
		int num = 0;
		float num2 = width * 0.5f;
		float num3 = length * -0.5f;
		float num4 = width / 1f;
		float num5 = length / 1f;
		for (float num6 = 0f; num6 < 2f; num6 += 1f)
		{
			for (float num7 = 0f; num7 < 2f; num7 += 1f)
			{
				array[num] = new Vector3(num2 - num6 * num4, num3 + num7 * num5, 0f);
				array2[num++] = new Vector2(num6 * 1f, num7 * 1f);
			}
		}
		num = 0;
		for (int i = 0; i < 1; i++)
		{
			for (int j = 0; j < 1; j++)
			{
				array3[num] = j * 2 + i;
				array3[num + 1] = j * 2 + i + 1;
				array3[num + 2] = (j + 1) * 2 + i;
				array3[num + 3] = (j + 1) * 2 + i;
				array3[num + 4] = j * 2 + i + 1;
				array3[num + 5] = (j + 1) * 2 + i + 1;
				num += 6;
			}
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}
