using UnityEngine;

public class FlightData
{
	private float _flightDistance;

	private float _totalTime;

	private float _maxHeight;

	private float _startDistance;

	public float LandDistance
	{
		get
		{
			return _startDistance + _flightDistance;
		}
	}

	public float StartDistance
	{
		get
		{
			return _startDistance;
		}
	}

	public float DistanceToFly
	{
		get
		{
			return _flightDistance;
		}
	}

	public void CalculateForGameOver()
	{
		_flightDistance = 1f;
		_totalTime = CurrentHill.Instance.Definition._PebbleHandlingParams._FlyTime;
		_maxHeight = CurrentHill.Instance.Definition._PebbleHandlingParams._FlyHeight;
		_startDistance = BossMonster.ProgressToStartFlying;
	}

	public void CalculateForClayAndFlick(float clayCollected, float normalisedFlick)
	{
		PebbleHandlingParams pebbleHandlingParams = CurrentHill.Instance.Definition._PebbleHandlingParams;
		float num = pebbleHandlingParams._FlyDistanceMax - pebbleHandlingParams._FlyDistanceMin;
		float num2 = pebbleHandlingParams._FlyFactor_Clay + pebbleHandlingParams._FlyFactor_Gouge;
		float num3 = pebbleHandlingParams._FlyFactor_Clay / num2;
		float num4 = pebbleHandlingParams._FlyFactor_Gouge / num2;
		float value = clayCollected / pebbleHandlingParams._ClayNeededForMaxFlight;
		value = Mathf.Clamp(value, 0f, 1f);
		float num5 = value * num3;
		float num6 = pebbleHandlingParams._FlyDistanceMin + num * num5;
		float num7 = pebbleHandlingParams._FlyDistanceMin + num * num3;
		float num8 = num7;
		float num9 = num6 * normalisedFlick;
		float value2 = num9 / num8;
		value2 = Mathf.Clamp(value2, 0f, 1f);
		float num10 = value2 * num4;
		float num11 = num5 + num10;
		float num12 = 1f;
		float value3 = 0f;
		if (num12 == 0f)
		{
			Debug.LogWarning("You need to set up fly factors in the pebble handling params");
		}
		else
		{
			value3 = num11 / num12;
		}
		value3 = Mathf.Clamp(value3, 0f, 1f);
		_flightDistance = pebbleHandlingParams._FlyDistanceMin + num * value3;
		_totalTime = pebbleHandlingParams._FlyTime;
		_maxHeight = pebbleHandlingParams._FlyHeight;
		_startDistance = BossMonster.ProgressToStartFlying;
	}

	public float GetFlightProgressAtTime(float flyTime)
	{
		return flyTime / _totalTime;
	}

	public float GetHeightATTime(float flyTime)
	{
		float num = flyTime / _totalTime;
		num *= 2f;
		if (num > 1f)
		{
			num = 2f - num;
		}
		num = 1f - num;
		num *= num;
		num = 1f - num;
		return _maxHeight * num;
	}

	public float GetDistAtTime(float flyTime)
	{
		float num = flyTime / _totalTime;
		if (num >= 1f)
		{
			return _flightDistance + _startDistance;
		}
		return num * _flightDistance + _startDistance;
	}

	public Vector3 GetPointAtDistance(float distance)
	{
		float num = distance - _startDistance;
		float flyTime = num / _flightDistance * _totalTime;
		float heightATTime = GetHeightATTime(flyTime);
		float distAtTime = GetDistAtTime(flyTime);
		return new Vector3(0f, heightATTime, distAtTime);
	}
}
