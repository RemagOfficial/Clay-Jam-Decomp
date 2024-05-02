using System;
using System.Collections;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class BurstlyInstallTracker : MonoBehaviour
{
	private const string ReturnCodeSuccess = "DNL_OK";

	public string _BundleID = "com.zynga.fatpebble.clayjam";

	[method: MethodImpl(32)]
	public static event Action<string> BurstlyEvent;

	private void Awake()
	{
		SaveData.LoadEvent += OnSaveDataReady;
	}

	private void Done()
	{
		SaveData.LoadEvent -= OnSaveDataReady;
	}

	private void OnSaveDataReady()
	{
		if (SaveData.Instance.Progress._HasBeenInstallTracked.Set)
		{
			Done();
			return;
		}
		string hashedAndroidIMEI = GetHashedAndroidIMEI();
		if (hashedAndroidIMEI == "none")
		{
			if (BurstlyInstallTracker.BurstlyEvent != null)
			{
				BurstlyInstallTracker.BurstlyEvent("BadUDID");
			}
		}
		else
		{
			SendBurstlyDownloadTrackingEndPoint(hashedAndroidIMEI);
			Done();
		}
	}

	private static string GetMacAddress()
	{
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		NetworkInterface[] array = allNetworkInterfaces;
		foreach (NetworkInterface networkInterface in array)
		{
			PhysicalAddress physicalAddress = networkInterface.GetPhysicalAddress();
			if (!string.IsNullOrEmpty(physicalAddress.ToString()))
			{
				return physicalAddress.ToString();
			}
		}
		return "none";
	}

	public static string GetHashedMacAddressForBurstlyIOS()
	{
		string macAddress = GetMacAddress();
		if (macAddress == "none")
		{
			return macAddress;
		}
		macAddress = macAddress.ToUpper();
		if (!macAddress.Contains(":"))
		{
			char[] array = macAddress.ToCharArray();
			macAddress = string.Empty;
			int num = 0;
			for (int i = 0; i < 6; i++)
			{
				if (num >= array.Length - 1)
				{
					break;
				}
				macAddress += array[num++];
				macAddress += array[num++];
				if (i < 5)
				{
					macAddress += ":";
				}
			}
		}
		return Hashed_SHA1(macAddress);
	}

	private static string Hashed_SHA1(string unHashed)
	{
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		byte[] bytes = aSCIIEncoding.GetBytes(unHashed);
		SHA1Managed sHA1Managed = new SHA1Managed();
		string text = string.Empty;
		byte[] array = sHA1Managed.ComputeHash(bytes);
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			text += string.Format("{0:x2}", b);
		}
		return text;
	}

	public static string GetHashedAndroidIMEI()
	{
		string deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
		return Hashed_SHA1(deviceUniqueIdentifier);
	}

	private void SendBurstlyDownloadTrackingEndPoint(string UDID)
	{
		string url = string.Format("http://req.appads.com/scripts/ConfirmDownload.aspx?bundleId={0}&mac={1}&distribution=true", _BundleID, UDID);
		PostBurstly(url);
	}

	private void BurstlyReturned(string result)
	{
		if (result.CompareTo("DNL_OK") == 0)
		{
			SaveData.Instance.Progress._HasBeenInstallTracked.Set = true;
		}
		else
		{
			SaveData.Instance.Progress._HasBeenInstallTracked.Set = true;
		}
		if (BurstlyInstallTracker.BurstlyEvent != null)
		{
			BurstlyInstallTracker.BurstlyEvent(result);
		}
	}

	private void PostBurstly(string url)
	{
		StartCoroutine(PostURL(url));
	}

	private IEnumerator PostURL(string url)
	{
		WWW www = new WWW(url);
		yield return www;
		if (www.error != null)
		{
			BurstlyReturned(www.error);
		}
		else
		{
			BurstlyReturned(www.text);
		}
	}
}
