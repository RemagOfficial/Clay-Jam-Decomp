public static class GameModeTypeNames
{
	private static string[] _names;

	public static string GetName(GameModeType type)
	{
		return GetName((int)type);
	}

	public static string GetName(int typeInt)
	{
		if (_names == null)
		{
			GenerateNames();
		}
		return _names[typeInt];
	}

	private static void GenerateNames()
	{
		_names = new string[2];
		for (int i = 0; i < 2; i++)
		{
			GameModeType gameModeType = (GameModeType)i;
			string text = gameModeType.ToString();
			_names[i] = text;
		}
	}
}
