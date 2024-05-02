using System;
using System.Collections.Generic;

[Serializable]
public class XMLElement : IXMLNode
{
	protected string valueString;

	protected IXMLNode parentNode;

	protected List<IXMLNode> childList;

	protected List<XMLAttribute> attributeList;

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
			return XMLNodeType.Element;
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
			return childList;
		}
		set
		{
			childList = value;
		}
	}

	public List<XMLAttribute> Attributes
	{
		get
		{
			return attributeList;
		}
		set
		{
			attributeList = value;
		}
	}

	public XMLElement(string name, IXMLNode parent, List<IXMLNode> children, List<XMLAttribute> attributes)
	{
		valueString = name;
		parentNode = parent;
		childList = children;
		attributeList = attributes;
		if (parent != null)
		{
			parent.Children.Add(this);
		}
	}

	public XMLElement(string name, IXMLNode parent, List<IXMLNode> children)
	{
		valueString = name;
		parentNode = parent;
		childList = children;
		attributeList = new List<XMLAttribute>();
		if (parent != null)
		{
			parent.Children.Add(this);
		}
	}

	public XMLElement(string name, IXMLNode parent)
	{
		valueString = name;
		parentNode = parent;
		childList = new List<IXMLNode>();
		attributeList = new List<XMLAttribute>();
		if (parent != null)
		{
			parent.Children.Add(this);
		}
	}
}
