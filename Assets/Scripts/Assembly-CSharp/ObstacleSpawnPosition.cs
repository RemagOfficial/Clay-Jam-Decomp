using UnityEngine;

public class ObstacleSpawnPosition : MonoBehaviour
{
	public void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, 0.25f);
	}
}
