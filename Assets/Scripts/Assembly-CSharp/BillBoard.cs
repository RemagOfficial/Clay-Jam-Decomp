using UnityEngine;

public class BillBoard : MonoBehaviour
{
	public bool _FaceCameraAlongYAxis;

	public bool _FixedCameraBillBoard = true;

	private void Update()
	{
		FaceCamera();
	}

	public void FaceCamera()
	{
		if (_FixedCameraBillBoard)
		{
			base.transform.rotation = CameraDirector.Instance.FixedBillboardRotation;
			return;
		}
		Vector3 position = Camera.main.transform.position;
		base.transform.LookAt(position, Vector3.up);
		if (_FaceCameraAlongYAxis)
		{
			base.transform.Rotate(Vector3.right, 90f);
		}
	}
}
