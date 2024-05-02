using UnityEngine;

public class GougeSection
{
	public Vector3 Start { get; set; }

	public Vector3 End { get; set; }

	public Texture2D BumpTexture { get; set; }

	public GougeSection(Vector3 start, Vector3 end, Texture2D bumpTexture)
	{
		Start = start;
		End = end;
		BumpTexture = bumpTexture;
	}
}
