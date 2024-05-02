using UnityEngine;

public class UIModeSelect : MonoBehaviour
{
	public GameModeType _Mode;

	private void OnClick()
	{
		SaveData.Instance.GameMode.Current = _Mode;
	}
}
