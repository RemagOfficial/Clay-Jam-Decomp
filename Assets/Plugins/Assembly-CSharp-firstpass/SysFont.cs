using UnityEngine;

public class SysFont : MonoBehaviour
{
	public enum Alignment
	{
		Left = 0,
		Center = 1,
		Right = 2
	}

	private static AndroidJavaObject _unitySysFontInstance;

	private static AndroidJavaObject UnitySysFontInstance
	{
		get
		{
			if (_unitySysFontInstance == null)
			{
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.github.imkira.unitysysfont.UnitySysFont");
				_unitySysFontInstance = androidJavaClass.CallStatic<AndroidJavaObject>("getInstance", new object[0]);
			}
			return _unitySysFontInstance;
		}
	}

	private static void _SysFontQueueTexture(string text, string fontName, int fontSize, bool isBold, bool isItalic, Alignment alignment, int maxWidthPixels, int maxHeightPixels, int textureID)
	{
		UnitySysFontInstance.Call("queueTexture", text, fontName, fontSize, isBold, isItalic, (int)alignment, maxWidthPixels, maxHeightPixels, textureID);
	}

	private static void _SysFontUpdateQueuedTexture(int textureID)
	{
		UnitySysFontInstance.Call("updateQueuedTexture", textureID);
	}

	private static void _SysFontDequeueTexture(int textureID)
	{
		UnitySysFontInstance.Call("dequeueTexture", textureID);
	}

	private static int _SysFontGetTextureWidth(int textureID)
	{
		return UnitySysFontInstance.Call<int>("getTextureWidth", new object[1] { textureID });
	}

	private static int _SysFontGetTextureHeight(int textureID)
	{
		return UnitySysFontInstance.Call<int>("getTextureHeight", new object[1] { textureID });
	}

	private static int _SysFontGetTextWidth(int textureID)
	{
		return UnitySysFontInstance.Call<int>("getTextWidth", new object[1] { textureID });
	}

	private static int _SysFontGetTextHeight(int textureID)
	{
		return UnitySysFontInstance.Call<int>("getTextHeight", new object[1] { textureID });
	}

	private static void _SysFontRender()
	{
		UnitySysFontInstance.Call("processQueue");
	}

	public static int GetTextureWidth(int textureID)
	{
		return Mathf.Max(_SysFontGetTextureWidth(textureID), 1);
	}

	public static int GetTextureHeight(int textureID)
	{
		return Mathf.Max(_SysFontGetTextureHeight(textureID), 1);
	}

	public static int GetTextWidth(int textureID)
	{
		return Mathf.Max(_SysFontGetTextWidth(textureID), 1);
	}

	public static int GetTextHeight(int textureID)
	{
		return Mathf.Max(_SysFontGetTextHeight(textureID), 1);
	}

	public static void QueueTexture(string text, string fontName, int fontSize, bool isBold, bool isItalic, Alignment alignment, bool isMultiLine, int maxWidthPixels, int maxHeightPixels, int textureID)
	{
		if (!isMultiLine)
		{
			text = text.Replace("\r\n", string.Empty).Replace("\n", string.Empty);
		}
		_SysFontQueueTexture(text, fontName, fontSize, isBold, isItalic, alignment, maxWidthPixels, maxHeightPixels, textureID);
	}

	public static void UpdateQueuedTexture(int textureID)
	{
		_SysFontUpdateQueuedTexture(textureID);
		_SysFontRender();
	}

	public static void DequeueTexture(int textureID)
	{
		_SysFontDequeueTexture(textureID);
	}

	public static void SafeDestroy(Object obj)
	{
		if (obj != null)
		{
			if (Application.isEditor)
			{
				Object.DestroyImmediate(obj);
			}
			else
			{
				Object.Destroy(obj);
			}
		}
	}
}
