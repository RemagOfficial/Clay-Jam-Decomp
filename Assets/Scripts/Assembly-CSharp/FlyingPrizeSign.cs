using UnityEngine;

public class FlyingPrizeSign : MonoBehaviour
{
	private GameObject newFlyingPrize;

	public GameObject FlyingPrize;

	private float prizeFlightTime;

	public Camera Uicam;

	public GameObject Panel;

	private float width;

	private float height;

	private bool isActive;

	public static FlyingPrizeSign Instance { get; private set; }

	public bool GivingPrize { get; set; }

	private void Start()
	{
	}

	public void Active()
	{
		width = Screen.width;
		height = Screen.height;
		isActive = true;
		FlyingPrize.GetComponent<ParticleEmitter>().emit = true;
	}

	private void Awake()
	{
		Uicam = NGUITools.FindCameraForLayer(LayerMask.NameToLayer("NGUI"));
		newFlyingPrize = NGUITools.AddChild(GameObject.Find("PopupPanel"), FlyingPrize);
		if (Instance != null)
		{
			Debug.LogError("More than one FlyingPrizeSign", base.gameObject);
		}
		Instance = this;
		FlyingPrize.SetActiveRecursively(true);
		Debug.Log("Flying Prize init complete ");
	}

	private void Update()
	{
		if (isActive)
		{
			Vector3 vector = Camera.mainCamera.WorldToScreenPoint(Instance.transform.position);
			float x = vector.x;
			float y = vector.y;
			x = 1f / width * x;
			y = 1f / height * y;
			if (x < -1f)
			{
				x = -1f;
			}
			if (x > 2f)
			{
				x = 2f;
			}
			if (y < -1f)
			{
				y = -1f;
			}
			if (y > 2f)
			{
				y = 2f;
			}
			newFlyingPrize.transform.localPosition = new Vector3(x, y, 0f);
		}
	}
}
