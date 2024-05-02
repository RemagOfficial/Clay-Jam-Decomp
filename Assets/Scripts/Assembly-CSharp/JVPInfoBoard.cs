using System;

[Serializable]
public class JVPInfoBoard
{
	public LocalisableText _Text;

	public void SetInfo(JVPItemData itemData)
	{
		if (_Text != null)
		{
			TurnOnTitle(itemData);
		}
	}

	private void TurnOnTitle(JVPItemData itemData)
	{
		if (itemData.Item != null)
		{
			_Text.text = itemData.Item.Info;
		}
		else
		{
			_Text.text = string.Empty;
		}
	}
}
