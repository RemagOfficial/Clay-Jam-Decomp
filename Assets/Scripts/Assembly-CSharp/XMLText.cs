using System;
using System.Collections.Generic;

[Serializable]
public class XMLText : IXMLNode
{
	protected string valueString;

	protected IXMLNode parentNode;

	public string value
	{
		get
		{
			return valueString;
		}
		set
		{
			valueString = value;
		}
	}

	public XMLNodeType type
	{
		get
		{
			return XMLNodeType.Text;
		}
	}

	public IXMLNode Parent
	{
		get
		{
			return parentNode;
		}
	}

	public List<IXMLNode> Children
	{
		get
		{
			return new List<IXMLNode>();
		}
		set
		{
		}
	}

	public List<XMLAttribute> Attributes
	{
		get
		{
			return new List<XMLAttribute>();
		}
		set
		{
		}
	}

	public XMLText(string text, IXMLNode parent)
	{
		valueString = text;
		parentNode = parent;
		if (parent != null)
		{
			parent.Children.Add(this);
		}
	}

	public override string ToString()
	{
		return valueString;
	}
}
