using UnityEngine;

public class GougeSectionCollider : MonoBehaviour
{
	public GougeSpline Spline { get; set; }

	public int StartSample { get; set; }

	public int EndSample { get; set; }

	public GougeSectionCollider Previous { get; set; }

	public GougeSectionCollider Next { get; set; }

	public Vector3 Direction
	{
		get
		{
			return (!(StartPos.z < EndPos.z)) ? (base.transform.forward * -1f) : base.transform.forward;
		}
	}

	public Vector3 UpHillNormal
	{
		get
		{
			if (base.transform.right.z < 0f)
			{
				return base.transform.right;
			}
			return base.transform.right * -1f;
		}
	}

	public Vector3 DownHillNormal
	{
		get
		{
			if (base.transform.right.z >= 0f)
			{
				return base.transform.right;
			}
			return base.transform.right * -1f;
		}
	}

	public Vector3 SpinAxis
	{
		get
		{
			return Vector3.Cross(Direction, Vector3.down);
		}
	}

	public float Radius
	{
		get
		{
			return ((CapsuleCollider)base.collider).radius;
		}
	}

	public float MaxDepth
	{
		get
		{
			return 0.1f;
		}
	}

	public float Length
	{
		get
		{
			return ((CapsuleCollider)base.collider).height;
		}
		set
		{
			float height = value + 0.05f;
			((CapsuleCollider)base.collider).height = height;
		}
	}

	public Vector3 EndPos { get; private set; }

	public Vector3 StartPos { get; private set; }

	private void OnTriggerEnter(Collider other)
	{
		Pebble component = other.gameObject.GetComponent<Pebble>();
		if (component != null)
		{
			component.EnterGougeSection(this);
		}
	}

	public bool ToCenterLine(Vector3 pos, out Vector3 vector, out float depth)
	{
		Plane plane = new Plane(SpinAxis, base.transform.position);
		Vector3 downHillNormal = DownHillNormal;
		Ray ray = new Ray(pos, downHillNormal);
		float enter;
		if (plane.Raycast(ray, out enter))
		{
			vector = downHillNormal * enter;
			depth = (1f - enter / Radius) * MaxDepth;
			depth = Mathf.Max(0f, depth);
			return true;
		}
		vector = Vector3.zero;
		depth = 0f;
		return false;
	}

	private void OrientateAndSize()
	{
		Vector3 position = (StartPos + EndPos) * 0.5f;
		base.transform.position = position;
		base.transform.LookAt(EndPos);
		Length = (StartPos - EndPos).magnitude;
	}

	internal void Initialise()
	{
		CapsuleCollider capsuleCollider = base.gameObject.AddComponent<CapsuleCollider>();
		capsuleCollider.direction = 2;
		capsuleCollider.radius = CameraDirector.Instance.FingerPrintScale * 0.5f;
		capsuleCollider.isTrigger = true;
	}

	public void SetEndPoints(Vector3 startPoint, Vector3 endPoint, int startSample, int endSample)
	{
		StartPos = startPoint;
		EndPos = endPoint;
		StartSample = startSample;
		EndSample = endSample;
		OrientateAndSize();
	}

	public bool Touches(GougeSectionCollider gougeSection)
	{
		if (gougeSection == this)
		{
			return true;
		}
		if (Previous != null && Previous == gougeSection)
		{
			return true;
		}
		if (Next != null && Next == gougeSection)
		{
			return true;
		}
		return false;
	}
}
