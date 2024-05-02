using System;
using Fabric;

[Serializable]
public class StorySFX
{
	private enum State
	{
		NotPlayed = 0,
		Playing = 1,
		Played = 2
	}

	public float _StartTime;

	public float _StopTime = -1f;

	public string _Event;

	private State _currentState;

	public void Reset()
	{
		if (_currentState == State.Playing)
		{
			Stop();
		}
		_currentState = State.NotPlayed;
	}

	public void Update(float timeSinceStoryStarted)
	{
		switch (_currentState)
		{
		case State.Played:
			break;
		case State.Playing:
			if (timeSinceStoryStarted > _StopTime)
			{
				Stop();
			}
			break;
		case State.NotPlayed:
			if (timeSinceStoryStarted > _StartTime)
			{
				Play();
			}
			break;
		}
	}

	private void Play()
	{
		InGameAudio.PostFabricEvent(_Event, EventAction.PlaySound);
		if (_StopTime < _StartTime)
		{
			_currentState = State.Played;
		}
		else
		{
			_currentState = State.Playing;
		}
	}

	private void Stop()
	{
		InGameAudio.PostFabricEvent(_Event, EventAction.StopSound);
	}
}
