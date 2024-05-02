using System;
using System.Collections;

[Serializable]
public class ClayData
{
	public int _ColourIndex;

	public float _Amount;

	public ClayData()
	{
	}

	public ClayData(int index, float amount)
	{
		_ColourIndex = index;
		_Amount = amount;
	}

	public ClayData Clone()
	{
		return new ClayData(_ColourIndex, _Amount);
	}

	public string Encode()
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("Index", _ColourIndex);
		hashtable.Add("Amount", _Amount);
		return MiniJSON.jsonEncode(hashtable);
	}

	public void Decode(string json)
	{
		Hashtable hashtable = (Hashtable)MiniJSON.jsonDecode(json);
		double num = (double)hashtable["Index"];
		_ColourIndex = (int)num;
		double num2 = (double)hashtable["Amount"];
		_Amount = (float)num2;
	}
}
