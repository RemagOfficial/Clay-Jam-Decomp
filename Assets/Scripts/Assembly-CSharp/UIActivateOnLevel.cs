using UnityEngine;

[AddComponentMenu("NGUI/ClayJam/Activate On Level")]
public class UIActivateOnLevel : MonoBehaviour
{
	public int _HillID;

	private void OnEnable()
	{
		if (CurrentHill.Instance.ID != _HillID)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}
}
