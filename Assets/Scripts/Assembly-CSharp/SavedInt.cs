using System;
using System.Collections;

public class SavedInt
{
	public int ValueWhenLoadingFromOldVersion { get; set; }

	public int Value { get; set; }

	public string KeyName { get; set; }

	public Version IntroducedInVersion { get; set; }

	public SavedInt(string keyName, int val, Version introducedInVersion)
	{
		KeyName = keyName;
		Value = val;
		IntroducedInVersion = introducedInVersion;
		ValueWhenLoadingFromOldVersion = val;
	}

	public void AddToHashTable(Hashtable hash)
	{
		hash.Add(KeyName, Value.ToString());
	}

	public void SetFromSaveData(Hashtable hash, Version version)
	{
		if (version < IntroducedInVersion)
		{
			Value = ValueWhenLoadingFromOldVersion;
			return;
		}
		string s = (string)hash[KeyName];
		Value = int.Parse(s);
	}
}
