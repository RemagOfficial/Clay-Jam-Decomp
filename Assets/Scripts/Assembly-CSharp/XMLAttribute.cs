using System;

[Serializable]
public struct XMLAttribute
{
	public string name;

	public string value;

	public XMLAttribute(string name, string value)
	{
		this.name = name;
		this.value = value;
	}

	public XMLAttribute(string name)
	{
		this.name = name;
		value = string.Empty;
	}

	public override string ToString()
	{
		return value;
	}
}
