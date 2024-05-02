using UnityEngine;

public class Gouge : MonoBehaviour
{
	private const float TimeToDie = 1f;

	private GougeRender _renderer;

	private GougeCollider _collider;

	private float _normalisedDieTime;

	public bool Finished { get; set; }

	public bool Dieing { get; set; }

	public GougeSpline Spline { get; set; }

	public void Initialise(bool newInput)
	{
		Spline = new GougeSpline(base.transform.position, newInput);
		AddComponents();
	}

	private void AddComponents()
	{
		_renderer = base.gameObject.AddComponent<GougeRender>();
		_renderer.Spline = Spline;
		_collider = base.gameObject.AddComponent<GougeCollider>();
		_collider.Spline = Spline;
		_collider.Update();
	}

	public void AddPoint(Vector3 point)
	{
		Finished = Spline.AddPoint(point);
	}

	public void Stop()
	{
		Finished = true;
	}

	public void Die()
	{
		_normalisedDieTime = 0f;
		Dieing = true;
	}

	private void Update()
	{
		if (Dieing)
		{
			_normalisedDieTime += Time.deltaTime / 1f;
			if (_normalisedDieTime > 0.5f && _collider != null)
			{
				_collider.Die();
				_collider = null;
				Pebble.Instance.LeaveGougeSpline(Spline);
			}
			if (_normalisedDieTime > 1f)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				_renderer.Fade(_normalisedDieTime);
			}
		}
	}

	public GougeSectionCollider GetFirstCollider()
	{
		return _collider.GetFirstCollider();
	}
}
