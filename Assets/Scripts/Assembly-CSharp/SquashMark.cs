using UnityEngine;

public class SquashMark : MonoBehaviour
{
	private const float MaxScale = 2f;

	private const float MovesToMakeTargetRotation = 5f;

	private Material HSVMaterial { get; set; }

	public Quaternion Rotation { get; private set; }

	public HSVColour Colour { get; private set; }

	public Vector2 Offset { get; private set; }

	public Vector2 Scale { get; private set; }

	public Quaternion TargetRotation { get; private set; }

	public int MovesSinceAppeared { get; private set; }

	private void Awake()
	{
		Renderer componentInChildren = base.gameObject.GetComponentInChildren<Renderer>();
		HSVMaterial = componentInChildren.material;
	}

	public void SetTexture(Texture texture)
	{
		HSVMaterial.mainTexture = texture;
	}

	public void SetFrom(SquashMark other)
	{
		if (other.gameObject.active)
		{
			MovesSinceAppeared = other.MovesSinceAppeared + 1;
			float num = (float)MovesSinceAppeared / 5f;
			num = Mathf.Clamp(MovesSinceAppeared, 0f, 1f);
			TargetRotation = other.TargetRotation;
			Rotation = Quaternion.Lerp(other.Rotation, TargetRotation, num);
			Scale = other.Scale;
			Offset = other.Offset;
			Colour = other.Colour;
			Activate();
		}
	}

	private void Activate()
	{
		if (!base.gameObject.active)
		{
			base.gameObject.SetActiveRecursively(true);
		}
		base.transform.localRotation = Rotation;
		HSVMaterial.mainTextureOffset = Offset;
		HSVMaterial.mainTextureScale = Scale;
		Colour.UseOnHSVMaterial(HSVMaterial);
	}

	public void Appear(Quaternion rotation, ObstacleMould obstacle, Quaternion targetRotation)
	{
		Rotation = Quaternion.Inverse(rotation);
		TargetRotation = targetRotation;
		float size = obstacle.Size;
		float radiusScaleForSize = CurrentHill.Instance.Definition._PebbleHandlingParams.GetRadiusScaleForSize(size);
		float radiusScale = Pebble.Instance.RadiusScale;
		float value = radiusScaleForSize / radiusScale;
		value = Mathf.Clamp(value, 0.75f, 1f);
		value = 1f / value;
		float num = 0f - (value - 1f) * 0.5f;
		Offset = new Vector2(num, num);
		Scale = new Vector2(value, value);
		if (obstacle.Type == ObstacleType.Trap)
		{
			Colour = ColourDatabase.Instance._TrapSplatColour;
		}
		else
		{
			Colour = obstacle.Colour;
		}
		MovesSinceAppeared = 0;
		Activate();
	}

	public void TurnOff()
	{
		base.gameObject.SetActiveRecursively(false);
	}
}
