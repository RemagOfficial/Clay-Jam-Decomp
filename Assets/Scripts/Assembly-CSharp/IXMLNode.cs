using System.Collections.Generic;

public interface IXMLNode
{
	string value { get; set; }

	XMLNodeType type { get; }

	IXMLNode Parent { get; }

	List<IXMLNode> Children { get; set; }

	List<XMLAttribute> Attributes { get; set; }
}
