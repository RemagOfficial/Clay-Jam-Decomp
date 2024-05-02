using UnityEngine;

public class Printf : MonoBehaviour
{
	private const int numMessages = 5;

	private static string _text;

	private static bool _doClear;

	private static string[] _tempMessages = new string[5];

	private static float[] _tempMessageTimer = new float[5];

	private void Start()
	{
		Debug.LogError("DEBUG CLASS ONLY");
		_text = string.Empty;
	}

	private void Update()
	{
		if (_doClear)
		{
			_text = string.Empty;
			_doClear = false;
		}
		UpdateMessages();
	}

	private void OnGUI()
	{
		float num = 32f;
		float num2 = 72f;
		GUI.color = Color.black;
		GUI.Label(new Rect(num + 1f, num2 + 1f, 320f, 480f), _text);
		GUI.color = Color.white;
		GUI.Label(new Rect(num, num2, 320f, 480f), _text);
		_doClear = true;
		ShowMessages();
	}

	public static void PostMessage(string msg)
	{
		for (int i = 0; i < 5; i++)
		{
			if (_tempMessageTimer[i] <= 0f)
			{
				_tempMessageTimer[i] = 5f;
				_tempMessages[i] = msg;
				break;
			}
		}
	}

	private void UpdateMessages()
	{
		for (int i = 0; i < 5; i++)
		{
			if (!(_tempMessageTimer[i] > 0f))
			{
				continue;
			}
			_tempMessageTimer[i] -= Time.deltaTime;
			if (!(_tempMessageTimer[i] <= 0f))
			{
				continue;
			}
			for (int j = 0; j < 4; j++)
			{
				if (_tempMessageTimer[j] <= 0f)
				{
					_tempMessages[j] = _tempMessages[j + 1];
					_tempMessageTimer[j] = _tempMessageTimer[j + 1];
				}
				_tempMessages[j + 1] = string.Empty;
				_tempMessageTimer[j + 1] = 0f;
			}
		}
	}

	private void ShowMessages()
	{
		float num = 32f;
		float num2 = 8f;
		float width = (float)Screen.width - num * 2f;
		float num3 = 20f;
		GUI.skin.label.alignment = TextAnchor.UpperRight;
		for (int i = 0; i < 5; i++)
		{
			if (_tempMessageTimer[i] > 0f)
			{
				GUI.color = Color.black;
				GUI.Label(new Rect(num + 1f, num2 + 1f + num3 * (float)i, width, 64f), _tempMessages[i]);
				GUI.color = Color.white;
				GUI.Label(new Rect(num, num2 + num3 * (float)i, width, 64f), _tempMessages[i]);
			}
		}
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
	}

	public static void Print(string t)
	{
		if (_doClear)
		{
			_text = string.Empty;
			_doClear = false;
		}
		_text += t;
	}
}
