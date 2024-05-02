public static class CurrentGameMode
{
	public static GameModeType Type
	{
		get
		{
			return SaveData.Instance.GameMode.Current;
		}
	}

	public static int TypeInt
	{
		get
		{
			return (int)SaveData.Instance.GameMode.Current;
		}
	}

	public static GameModeState GameModeState
	{
		get
		{
			return GameModeController.Instance.GameMode;
		}
	}

	public static bool HasBottom
	{
		get
		{
			switch (Type)
			{
			case GameModeType.Quest:
				return true;
			case GameModeType.MonsterLove:
				return false;
			default:
				return true;
			}
		}
	}

	public static bool ShowClayAmountParticles
	{
		get
		{
			switch (Type)
			{
			case GameModeType.Quest:
				return true;
			case GameModeType.MonsterLove:
				return false;
			default:
				return true;
			}
		}
	}
}
