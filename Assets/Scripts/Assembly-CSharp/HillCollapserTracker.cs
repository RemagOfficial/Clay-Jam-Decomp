using UnityEngine;

public class HillCollapserTracker : MonoBehaviour
{
	private void FixedUpdate()
	{
		if (!(HillCollapser.Instance == null))
		{
			base.transform.position = new Vector3(0f, 0f, HillCollapser.Instance.PointOfCollapse + HillCollapser.Instance.BackOffAmount);
		}
	}
}
