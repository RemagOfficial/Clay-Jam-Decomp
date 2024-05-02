using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class Google : MonoBehaviour
{
	private const string _baseURL = "http://www.google-analytics.com/__utm.gif?";

	private const string _archivedURLSeperator = "||~||";

	private const string _unfinishedRequestsPrefsKey = "_GAUnfinishedRequests";

	private const string _persistancePrefsKey = "_GAPlayerMemory";

	private const string _visitCountPrefsKey = "_GAVisitCount";

	public static Google analytics;

	public bool rememberPlayerBetweenSessions = true;

	public bool debug;

	public string trackingCodeMobile;

	public string domainMobile;

	public string trackingCodeLeap;

	public string domainLeap;

	public float wwwTimeout = 15f;

	private IGALogger _logger;

	private Queue<string> _requestQueue = new Queue<string>();

	private bool _queueIsProcessing;

	private int _domainHash;

	private string TrackingCode
	{
		get
		{
			if (BuildDetails.Instance._OnlyUseLeap)
			{
				return trackingCodeLeap;
			}
			return trackingCodeMobile;
		}
	}

	private string Domain
	{
		get
		{
			if (BuildDetails.Instance._OnlyUseLeap)
			{
				return domainLeap;
			}
			return domainMobile;
		}
	}

	private double _epochTime
	{
		get
		{
			return (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		}
	}

	private void Awake()
	{
		if (analytics != null)
		{
			Debug.Log("Found existing Google.  Commiting suicide to avoid multiple instances.");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (TrackingCode.Length == 0 || Domain.Length == 0)
		{
			Debug.LogError("Please enter your tracking code and domain in the prefab!");
			return;
		}
		if (debug && !Application.isWebPlayer)
		{
			_logger = new GADebugLogger();
		}
		else
		{
			_logger = new GAEmptyLogger();
		}
		analytics = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		calculateDomainHash();
		resurrectQueue();
	}

	private void OnApplicationPause(bool didPause)
	{
		if (didPause)
		{
			persistQueue();
		}
		else
		{
			resurrectQueue();
		}
	}

	public void OnApplicationExit()
	{
		persistQueue();
	}

	public void logPageView()
	{
		logPageView(Application.loadedLevelName);
	}

	public void logPageView(string page)
	{
		logPageView(page, page);
	}

	public void logPageView(string page, string pageTitle)
	{
		sendRequest(page, pageTitle, null, null, null, null);
	}

	public void logEvent(string page, string category, string action)
	{
		logEvent(page, page, category, action);
	}

	public void logEvent(string page, string pageTitle, string category, string action)
	{
		sendRequest(page, pageTitle, category, action, null, null);
	}

	public void logEvent(string page, string category, string action, string label, int value)
	{
		logEvent(page, page, category, action, label, value);
	}

	public void logEvent(string page, string pageTitle, string category, string action, string label, int value)
	{
		sendRequest(page, pageTitle, category, action, label, value);
	}

	private void sendRequest(string page, string pageTitle, string category, string action, string label, int? value)
	{
		if (!page.StartsWith("/"))
		{
			page = "/" + page;
		}
		System.Random random = new System.Random();
		int num = (int)_epochTime;
		int @int = PlayerPrefs.GetInt("_GAVisitCount", 0);
		@int++;
		PlayerPrefs.SetInt("_GAVisitCount", @int);
		string empty = string.Empty;
		if (rememberPlayerBetweenSessions)
		{
			string text = PlayerPrefs.GetString("_GAPlayerMemory");
			if (text.Length == 0)
			{
				text = string.Format("{0}.{1}.{2}.{3}.{4}.", _domainHash, random.Next(1000000000), num, num, num);
				PlayerPrefs.SetString("_GAPlayerMemory", text);
			}
			empty = text + @int;
		}
		else
		{
			empty = string.Format("{0}.{1}.{2}.{3}.{4}.{5}", _domainHash, random.Next(1000000000), num, num, num, @int);
		}
		string arg = string.Format("{0}.{1}.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none)", _domainHash, num);
		string text2 = WWW.EscapeURL(string.Format("__utma={0};+__utmz={1};", empty, arg));
		text2 = text2.Replace("|", "%7C");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("utmwv", "4.8.8");
		dictionary.Add("utmn", random.Next(1000000000).ToString());
		dictionary.Add("utmhn", WWW.EscapeURL(Domain));
		dictionary.Add("utmcs", "UTF-8");
		dictionary.Add("utmsr", string.Format("{0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height));
		dictionary.Add("utmsc", "24-bit");
		dictionary.Add("utmul", "en-us");
		dictionary.Add("utmje", "0");
		dictionary.Add("utmfl", "-");
		dictionary.Add("utmdt", WWW.EscapeURL(pageTitle));
		dictionary.Add("utmhid", random.Next(1000000000).ToString());
		dictionary.Add("utmr", "-");
		dictionary.Add("utmp", WWW.EscapeURL(page));
		dictionary.Add("utmac", TrackingCode);
		dictionary.Add("utmcc", text2);
		Dictionary<string, string> dictionary2 = dictionary;
		if (category != null && action != null && category.Length > 0 && action.Length > 0)
		{
			string text3 = string.Format("5({0}*{1}", category, action);
			text3 = ((label == null || !value.HasValue || label.Length <= 0) ? (text3 + ")") : (text3 + string.Format("*{0})({1})", label, value.ToString())));
			dictionary2.Add("utme", WWW.EscapeURL(text3));
			dictionary2.Add("utmt", "event");
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string key in dictionary2.Keys)
		{
			stringBuilder.AppendFormat("{0}={1}&", key, dictionary2[key]);
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		string item = "http://www.google-analytics.com/__utm.gif?" + stringBuilder.ToString();
		_requestQueue.Enqueue(item);
		StartCoroutine(processRequestQueue());
	}

	private IEnumerator waitForRequest(WWW www)
	{
		float timeoutCompletion = Time.realtimeSinceStartup + wwwTimeout;
		while (Time.realtimeSinceStartup < timeoutCompletion && !www.isDone)
		{
			yield return null;
		}
	}

	private IEnumerator processRequestQueue()
	{
		if (_queueIsProcessing)
		{
			yield break;
		}
		_queueIsProcessing = true;
		while (_requestQueue.Count > 0)
		{
			string url = _requestQueue.Dequeue();
			_logger.logStartRequest(url);
			if (Application.isWebPlayer)
			{
				string eval = string.Format("var i = new Image(); i.src = '{0}'; document.body.appendChild( i );", url);
				Application.ExternalEval(eval);
				_logger.logSuccessfulRequest(url);
				continue;
			}
			WWW www = new WWW(url);
			yield return StartCoroutine(waitForRequest(www));
			if (www.isDone)
			{
				if (www.error != null)
				{
					_logger.logFailedRequest(url, www.error);
					_requestQueue.Enqueue(url);
					break;
				}
				_logger.logSuccessfulRequest(url);
				continue;
			}
			_requestQueue.Enqueue(url);
			break;
		}
		_queueIsProcessing = false;
	}

	private void persistQueue()
	{
		if (_requestQueue.Count > 0)
		{
			_logger.log(string.Format("[Saving {0} unsent requests]", _requestQueue.Count.ToString()));
			string[] array = new string[_requestQueue.Count];
			_requestQueue.CopyTo(array, 0);
			string value = string.Join("||~||", array);
			PlayerPrefs.SetString("_GAUnfinishedRequests", value);
			_requestQueue.Clear();
		}
	}

	private void resurrectQueue()
	{
		string @string = PlayerPrefs.GetString("_GAUnfinishedRequests");
		if (@string != string.Empty)
		{
			string[] array = @string.Split(new string[1] { "||~||" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in array)
			{
				_requestQueue.Enqueue(item);
			}
			PlayerPrefs.SetString("_GAUnfinishedRequests", string.Empty);
			_logger.log(string.Format("[Resurrected {0} unsent requests]", _requestQueue.Count.ToString()));
			StartCoroutine(processRequestQueue());
		}
	}

	private void calculateDomainHash()
	{
		int num = 1;
		int num2 = 0;
		num = 0;
		for (int num3 = Domain.Length - 1; num3 >= 0; num3--)
		{
			char c = char.Parse(Domain.Substring(num3, 1));
			int num4 = c;
			num = ((num << 6) & 0xFFFFFFF) + num4 + (num4 << 14);
			num2 = num & 0xFE00000;
			num = ((num2 == 0) ? num : (num ^ (num2 >> 21)));
		}
		_domainHash = num;
	}
}
