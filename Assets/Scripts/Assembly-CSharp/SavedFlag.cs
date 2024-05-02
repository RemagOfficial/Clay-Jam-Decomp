using System;
using System.Collections;

public class SavedFlag
{
	public bool ValueWhenLoadingFromOldVersion { get; set; }

	public bool Set { get; set; }

	public string KeyName { get; set; }

	public Version IntroducedInVersion { get; set; }

	public SavedFlag(string keyName, bool set, Version introducedInVersion)
	{
		KeyName = keyName;
		Set = set;
		IntroducedInVersion = introducedInVersion;
		ValueWhenLoadingFromOldVersion = Set;
	}

	public void AddToHashTable(Hashtable hash)
	{
		hash.Add(KeyName, (!Set) ? "0" : "1");
	}

	public void SetFromSaveData(Hashtable hash, Version version, bool adding)
	{
		if (!adding || Set == ValueWhenLoadingFromOldVersion)
		{
			if (version < IntroducedInVersion)
			{
				Set = ValueWhenLoadingFromOldVersion;
			}
			else if ((string)hash[KeyName] == "1")
			{
				Set = true;
			}
			else
			{
				Set = false;
			}
		}
	}
}
