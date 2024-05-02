public class JVPItemData
{
	private HatBrimItem _item;

	public HatBrimItem Item
	{
		get
		{
			return _item;
		}
		set
		{
			_item = value;
			SetData();
		}
	}

	public HatBrimItem.ItemType Type
	{
		get
		{
			return Item.Type;
		}
	}

	public CastData CastData { get; private set; }

	public string CostString { get; set; }

	public HSVColour Colour
	{
		get
		{
			if (Item == null)
			{
				return HSVColour.NoShift;
			}
			if (Item.Type == HatBrimItem.ItemType.Hill)
			{
				return CurrentHill.Instance.Definition._ColourFromOrange;
			}
			if (Item.Type == HatBrimItem.ItemType.Cast)
			{
				return CastData.Colour;
			}
			if (Item.Type == HatBrimItem.ItemType.PowerPlay)
			{
				return ColourDatabase.Instance._PowerPlayColour;
			}
			return HSVColour.NoShift;
		}
	}

	public bool CanPurchase
	{
		get
		{
			return Item.LockState == LockState.Unlocked && Item.CanAfford;
		}
	}

	public JVPItemData()
	{
		ClearData();
	}

	public void ForceReCheck()
	{
		SetData();
	}

	public void ClearData()
	{
		_item = null;
	}

	private void SetData()
	{
		if (Item != null)
		{
			switch (Item.Type)
			{
			case HatBrimItem.ItemType.Cast:
				CastData = Item.CastData;
				break;
			case HatBrimItem.ItemType.Hill:
				CastData = null;
				break;
			case HatBrimItem.ItemType.PowerPlay:
				CastData = null;
				break;
			}
		}
	}
}
