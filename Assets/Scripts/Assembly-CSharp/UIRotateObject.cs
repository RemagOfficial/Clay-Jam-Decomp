using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Rotate Object")]
public class UIRotateObject : MonoBehaviour
{
	public Transform tweenTarget;

	public Vector3 rotation = Vector3.zero;

	public float duration = 0.2f;

	private bool mInitDone;

	private float mTimer;

	private bool mCanRotate = true;

	private bool mIsHeldDown;

	private void Start()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Init()
	{
		mInitDone = true;
		if (tweenTarget == null)
		{
			tweenTarget = base.transform;
		}
	}

	private void Update()
	{
		if (mTimer > 0f)
		{
			mTimer -= Time.deltaTime;
			if (mTimer <= 0f)
			{
				mTimer = 0f;
				mCanRotate = true;
			}
		}
		if (mIsHeldDown && base.enabled && mCanRotate)
		{
			Quaternion rot = tweenTarget.rotation;
			rot *= Quaternion.Euler(rotation);
			if (!mInitDone)
			{
				Init();
			}
			TweenRotation.Begin(tweenTarget.gameObject, duration, rot).method = UITweener.Method.EaseInOut;
			mTimer = duration;
			mCanRotate = false;
		}
	}

	private void OnClick()
	{
		if (base.enabled && mCanRotate)
		{
			Quaternion rot = tweenTarget.rotation;
			rot *= Quaternion.Euler(rotation);
			if (!mInitDone)
			{
				Init();
			}
			TweenRotation.Begin(tweenTarget.gameObject, duration, rot).method = UITweener.Method.EaseInOut;
			mTimer = duration;
			mCanRotate = false;
		}
	}

	private void OnPress(bool isPressed)
	{
		if (isPressed)
		{
			mIsHeldDown = true;
		}
		else
		{
			mIsHeldDown = false;
		}
	}
}
