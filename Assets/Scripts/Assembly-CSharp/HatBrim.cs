using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fabric;
using UnityEngine;

public class HatBrim : MonoBehaviour
{
	private const string MonsterIconResourcePath = "BigJigger/MonsterIcons";

	private const string HillIconResourcePath = "BigJigger/HillIcons/";

	private const string HillIcon = "HillUpgrade";

	private const string PowerPlayIconResourcePath = "BigJigger/PowerPlayIcons/";

	private const string PowerPlayNameStub = "PowerPlayJVP";

	private const int NumItemSlots = 14;

	private const int HalfNumSlots = 7;

	private const string ItemSlotNameStub = "Character_";

	private const string SpinObjectName = "HatBrimModel";

	private const float TouchTotalMoveToStartSPinNotTap = 0.05f;

	private const float SnapRotationTime = 1f;

	private const float SpinTime = 2f;

	private const float MinSpinSpeed = 5f;

	private const float MaxSpinSpeed = 720f;

	private const float SpinTouchSpeedForMaxSpin = 4f;

	private Texture2D[] MonsterIcons;

	private HatBrimItemObject[] _itemSlotObjects = new HatBrimItemObject[14];

	private int[] _itemSlotMap = new int[14];

	private int _highlightItemSlot;

	private int _farthestForwardItemSlot;

	private int _farthestBackItemSlot;

	private List<HatBrimItem> Items;

	private List<HatBrimItem> DisplayItems;

	private static Material _shadowMaterial;

	private static string _shadowMaterialResourcePath = "Obstacles/Materials/ObstacleShadowMaterial";

	private float[] SlotRotations;

	private bool _playSounds;

	public bool ReadyToChoose
	{
		get
		{
			return !SpinTouchOn;
		}
	}

	public HatBrimItem CurrentHighlightItem
	{
		get
		{
			return ItemAtSlot(_highlightItemSlot);
		}
	}

	private Transform SpinObjectTransform { get; set; }

	private float CurrentTouchPos { get; set; }

	private float TotalTouchDist { get; set; }

	private float TouchTotalMove { get; set; }

	private bool SpinTouchOn { get; set; }

	private float SpinStart { get; set; }

	private float TargetRotation { get; set; }

	private float FinishRotateTime { get; set; }

	public bool Spinning { get; private set; }

	private float StartSpinSpeed { get; set; }

	private float SpinSpeed { get; set; }

	private float SpinDirection { get; set; }

	private float SpinStartTime { get; set; }

	private float SpinTouchStartRotation { get; set; }

	private float SpinTouchRotation { get; set; }

	private Vector3 SpinStartWorldDir { get; set; }

	private bool SpinnerFrozen { get; set; }

	public bool MouseOverSpinner { get; private set; }

	private float SpinTouchSpeed { get; set; }

	[method: MethodImpl(32)]
	public static event Action<HatBrimItem> NewHighlightEvent;

	[method: MethodImpl(32)]
	public static event Action NewHighlightChosenEvent;

	private int ItemIndexAtSlot(int slot)
	{
		if (slot < _itemSlotMap.Length)
		{
			return _itemSlotMap[slot];
		}
		return 0;
	}

	private HatBrimItem ItemAtSlot(int slot)
	{
		int num = ItemIndexAtSlot(slot);
		if (num < DisplayItems.Count)
		{
			return DisplayItems[num];
		}
		return DisplayItems[0];
	}

	private void Awake()
	{
		JVPController.StateChangeEvent += OnJVPStateChange;
		SaveData.SaveEvent += OnSaveDataChange;
	}

	private void OnDestroy()
	{
		JVPController.StateChangeEvent -= OnJVPStateChange;
		SaveData.SaveEvent -= OnSaveDataChange;
	}

	private void Start()
	{
		LoadMaterials();
		GetSpinObjectTransform();
		InitialiseSlots();
		CalculateSlotRotations();
		LoadItems();
		InitialiseWheel();
		UnFreezeSpinner();
		DisplayHat(false);
		InitialiseInput();
	}

	private void Update()
	{
		SetRotation();
	}

	public void RefreshItems()
	{
		FillDisplayList();
		SortItems();
		PadDisplayList();
	}

	public void OnJVPStateChange(JVPController.State state)
	{
		switch (state)
		{
		case JVPController.State.MenuAnimating:
			ResetWheel();
			break;
		case JVPController.State.Interactive:
			_playSounds = true;
			break;
		case JVPController.State.ZoomedOut:
			_playSounds = false;
			break;
		case JVPController.State.BuyingHillItem:
			break;
		case JVPController.State.ZoomingIn:
		case JVPController.State.Chewing:
		case JVPController.State.ZoomingOut:
			break;
		}
	}

	public void UpdateSpinInput(bool mouseOverSpinner, Vector3 mouseHitPoint)
	{
		MouseOverSpinner = mouseOverSpinner;
		if (SpinnerFrozen)
		{
			return;
		}
		float num = ClayJamInput.CursorScreenPosition.x - CurrentTouchPos;
		CurrentTouchPos = ClayJamInput.CursorScreenPosition.x;
		TotalTouchDist = CurrentTouchPos - SpinStart;
		if (mouseOverSpinner)
		{
			if (ClayJamInput.HatBrimActionStarted)
			{
				SpinTouchOn = true;
				SpinStart = ClayJamInput.CursorScreenPosition.x;
				SpinTouchStartRotation = SpinObjectTransform.localEulerAngles.z;
				SpinTouchRotation = 0f;
				SpinStartWorldDir = DirectionToSpinCenter(mouseHitPoint);
				SpinTouchSpeed = 0f;
				TouchTotalMove = 0f;
				num = 0f;
			}
			if (SpinTouchOn && ClayJamInput.HatBrimAction)
			{
				Vector3 lhs = DirectionToSpinCenter(mouseHitPoint);
				float value = Vector3.Dot(lhs, SpinStartWorldDir);
				value = Mathf.Clamp(value, -1f, 1f);
				float num2 = Mathf.Acos(value);
				SpinTouchRotation = num2 * 57.29578f;
				if (TotalTouchDist < 0f)
				{
					SpinTouchRotation *= -1f;
				}
				float num3 = num / (float)Screen.width;
				float num4 = num3 / Time.deltaTime;
				SpinTouchSpeed += num4;
				SpinTouchSpeed /= 4f;
			}
		}
		if (!SpinTouchOn)
		{
			return;
		}
		TouchTotalMove += Mathf.Abs(num);
		float num5 = 0.05f * (float)Screen.width;
		bool flag = Mathf.Abs(TouchTotalMove) < num5;
		if (BuildDetails.Instance._UseLeapIfAvailable)
		{
			flag = false;
		}
		if (flag)
		{
			Debug.Log(ToString() + ": UpdateSpinInput(): touchTreatedAsTap:" + flag);
			if (!mouseOverSpinner || ClayJamInput.HatBrimActionEnded)
			{
				Vector3 forward = Vector3.forward;
				float value2 = Vector3.Dot(forward, SpinStartWorldDir);
				value2 = Mathf.Clamp(value2, -1f, 1f);
				float num6 = Mathf.Acos(value2);
				float num7 = num6 * 57.29578f;
				if (SpinStart > (float)(Screen.width / 2))
				{
					num7 *= -1f;
				}
				float rotation = SpinObjectTransform.localEulerAngles.z + num7;
				int newHighlightSlot = NearestSlot(rotation);
				RotateToNewHighlight(newHighlightSlot);
				SpinTouchOn = false;
			}
		}
		else if (!mouseOverSpinner || ClayJamInput.HatBrimActionEnded)
		{
			float num8 = Mathf.Abs(SpinTouchSpeed);
			float t = num8 / 4f;
			float speed = Mathf.Lerp(0f, 720f, t);
			bool clockwise = SpinTouchSpeed > 0f;
			StartSpinning(speed, clockwise);
		}
	}

	private void SetRotation()
	{
		Vector3 localEulerAngles = SpinObjectTransform.localEulerAngles;
		if (SpinTouchOn)
		{
			localEulerAngles.z = SpinTouchStartRotation + SpinTouchRotation;
			int toNewHighlight = NearestSlot(localEulerAngles.z);
			SetToNewHighlight(toNewHighlight);
		}
		else if (Spinning)
		{
			localEulerAngles.z += SpinSpeed * Time.deltaTime * SpinDirection;
			float num = Time.time - SpinStartTime;
			float t = num / 2f;
			SpinSpeed = Mathf.Lerp(StartSpinSpeed, 0f, t);
			int num2 = NearestSlot(localEulerAngles.z);
			if (SpinSpeed <= 5f)
			{
				Spinning = false;
				RotateToNewHighlight(num2);
			}
			else
			{
				SetToNewHighlight(num2);
			}
		}
		else
		{
			float num3 = FinishRotateTime - Time.time;
			float t2 = 1f - num3 / 1f;
			localEulerAngles.z = Mathf.LerpAngle(localEulerAngles.z, TargetRotation, t2);
		}
		SpinObjectTransform.localEulerAngles = localEulerAngles;
	}

	public void DisplayHat(bool displayed)
	{
		base.gameObject.SetActiveRecursively(displayed);
	}

	private void InitialiseInput()
	{
		MouseOverSpinner = false;
	}

	public void InitialiseWheel()
	{
		_farthestForwardItemSlot = 6;
		_farthestBackItemSlot = 7;
		_highlightItemSlot = 0;
		ResetWheel();
	}

	public void ResetWheel()
	{
		RefreshItems();
		FillSlotMap(CurrentHill.Instance.ProgressData._LastJVPItem);
		HatBrim.NewHighlightEvent(ItemAtSlot(_highlightItemSlot));
		TargetRotation = SlotRotations[_highlightItemSlot];
		SetIcons();
	}

	private void FillSlotMap(int startIndex)
	{
		int num = startIndex;
		int index = _highlightItemSlot;
		for (int i = 0; i < 7; i++)
		{
			_itemSlotMap[index] = num;
			IncramentSlotIndex(ref index);
			num++;
			if (num >= DisplayItems.Count)
			{
				num = 0;
			}
		}
		num = startIndex;
		index = _highlightItemSlot;
		for (int j = 0; j < 7; j++)
		{
			DecramentSlotIndex(ref index);
			num--;
			if (num < 0)
			{
				num = DisplayItems.Count - 1;
			}
			_itemSlotMap[index] = num;
		}
	}

	private void SetIcons()
	{
		for (int i = 0; i < 14; i++)
		{
			HatBrimItem item = ItemAtSlot(i);
			_itemSlotObjects[i].SetupForItem(item);
		}
	}

	private void InitialiseSlots()
	{
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < 14; i++)
		{
			string slotName = string.Format("{0}{1:00}", "Character_", i + 1);
			Transform transform = Array.Find(componentsInChildren, (Transform t) => t.name == slotName);
			if ((bool)transform)
			{
				_itemSlotObjects[i] = new HatBrimItemObject();
				_itemSlotObjects[i].Initialise(transform.gameObject);
			}
			else
			{
				Debug.LogError(string.Format("Missing {0} object in HatBrim", slotName), base.gameObject);
			}
		}
	}

	public void ScaleHighlighted(int index, bool up = true)
	{
		if (index >= 0)
		{
			_itemSlotObjects[index].Scale(up);
		}
	}

	private void CalculateSlotRotations()
	{
		SlotRotations = new float[14]
		{
			78f, 51f, 25f, 359f, 334.5f, 310f, 286f, 262f, 235.5f, 209.5f,
			184f, 159.5f, 132f, 105.5f
		};
	}

	private void LoadItems()
	{
		Items = new List<HatBrimItem>();
		LoadCreatureItems();
		LoadHillItems();
		LoadPowerPlaytems();
		DisplayItems = new List<HatBrimItem>(Items.Count);
	}

	private void LoadCreatureIcons()
	{
		UnityEngine.Object[] array = Resources.LoadAll("BigJigger/MonsterIcons");
		MonsterIcons = new Texture2D[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			MonsterIcons[i] = array[i] as Texture2D;
		}
	}

	private void LoadCreatureItems()
	{
		LoadCreatureIcons();
		List<CastData> purchasableList = SaveData.Instance.Casts.PurchasableList;
		foreach (CastData item in purchasableList)
		{
			HatBrimItem hatBrimItem = new HatBrimItem(HatBrimItem.ItemType.Cast);
			string iconName = item.IconName;
			if (iconName == string.Empty)
			{
				iconName = item.MouldName;
			}
			Texture2D texture2D = Array.Find(MonsterIcons, (Texture2D t) => t.name == iconName);
			if (texture2D == null)
			{
				Debug.LogError(string.Format("Couldn't find Monster Icon {0} for {1}", iconName, item.MouldName));
			}
			hatBrimItem.AddCastData(item);
			string key = "MONSTER_" + item.MouldName;
			hatBrimItem.Name = Localization.instance.GetFor3DText(key);
			hatBrimItem.Texture = texture2D;
			Items.Add(hatBrimItem);
		}
	}

	private void LoadHillItems()
	{
		int maxUpgradeLevel = HillDatabase.Instance.GetDefintionFromIndex(0).MaxUpgradeLevel;
		for (int i = 0; i <= maxUpgradeLevel; i++)
		{
			LoadHillItem(i);
		}
	}

	private void LoadHillItem(int upgradeLevel)
	{
		string text = string.Format("{0}{1}{2}", "BigJigger/HillIcons/", "HillUpgrade", upgradeLevel);
		Texture2D texture2D = Resources.Load(text) as Texture2D;
		if (texture2D == null)
		{
			Debug.LogError(string.Format("Couldn't find Hill Icon {0}", text));
		}
		HatBrimItem hatBrimItem = new HatBrimItem(HatBrimItem.ItemType.Hill);
		hatBrimItem.Texture = texture2D;
		hatBrimItem.AddHillData(upgradeLevel);
		hatBrimItem.Name = Localization.instance.GetFor3DText((upgradeLevel >= 1) ? "JVP_ExpandHill" : "JVP_UnlockHill");
		Items.Add(hatBrimItem);
	}

	private void LoadPowerPlaytems()
	{
		string text = string.Format("{0}{1}", "BigJigger/PowerPlayIcons/", "PowerPlayJVP");
		Texture2D texture2D = Resources.Load(text) as Texture2D;
		if (texture2D == null)
		{
			Debug.LogError(string.Format("Couldn't find Powerplay Icon for {0}", text));
		}
		for (int i = 0; i < PowerupDatabase.Instance.NumPowerPlayPacks(CurrentHill.Instance.ID); i++)
		{
			HatBrimItem hatBrimItem = new HatBrimItem(HatBrimItem.ItemType.PowerPlay);
			hatBrimItem.Texture = texture2D;
			hatBrimItem.AddPowerPlayData(i);
			hatBrimItem.Name = Localization.instance.GetFor3DText("POWERUP_Title");
			Items.Add(hatBrimItem);
		}
	}

	private void GetSpinObjectTransform()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == "HatBrimModel")
			{
				SpinObjectTransform = transform;
				return;
			}
		}
		Debug.LogError(string.Format("{0} object not found in children of HatBrim", "HatBrimModel"), base.gameObject);
	}

	private void DecramentSlotIndex(ref int index)
	{
		index--;
		if (index < 0)
		{
			index = 13;
		}
	}

	private void IncramentSlotIndex(ref int index)
	{
		index++;
		if (index >= 14)
		{
			index = 0;
		}
	}

	private void LoadMaterials()
	{
		if (_shadowMaterial == null)
		{
			_shadowMaterial = Resources.Load(_shadowMaterialResourcePath) as Material;
			if (_shadowMaterial == null)
			{
				Debug.LogError(string.Format("Material path not found for Brim item shadow :{0}", _shadowMaterialResourcePath));
			}
		}
	}

	private void StartSpinning(float speed, bool clockwise)
	{
		SpinTouchOn = false;
		Spinning = true;
		SpinStartTime = Time.time;
		float startSpinSpeed = (SpinSpeed = speed);
		StartSpinSpeed = startSpinSpeed;
		SpinDirection = ((!clockwise) ? (-1f) : 1f);
	}

	public void StopSpinning()
	{
		SpinStartTime = Time.time - 2f;
	}

	private void RotateToNewHighlight(int newHighlightSlot)
	{
		SpinTouchOn = false;
		Spinning = false;
		FinishRotateTime = Time.time + 1f;
		TargetRotation = SlotRotations[newHighlightSlot];
		SetToNewHighlight(newHighlightSlot);
		CurrentHill.Instance.ProgressData._LastJVPItem = ItemIndexAtSlot(newHighlightSlot);
		if (HatBrim.NewHighlightChosenEvent != null)
		{
			HatBrim.NewHighlightChosenEvent();
		}
	}

	private void SetToNewHighlight(int newHighlightSlot)
	{
		if (_highlightItemSlot != newHighlightSlot)
		{
			if (newHighlightSlot != 0)
			{
				SaveData.Instance.Progress._HasSpunJVP.Set = true;
			}
			int i = newHighlightSlot - _highlightItemSlot;
			if (i > 7)
			{
				i -= 14;
			}
			else if (i < -7)
			{
				i += 14;
			}
			while (i > 0)
			{
				ShiftItemsForward();
				i--;
			}
			for (; i < 0; i++)
			{
				ShiftItemsBack();
			}
			ScaleHighlighted(_highlightItemSlot, false);
			ScaleHighlighted(newHighlightSlot);
			_highlightItemSlot = newHighlightSlot;
			HatBrim.NewHighlightEvent(CurrentHighlightItem);
			if (_playSounds)
			{
				InGameAudio.PostFabricEvent("JVPSpin", EventAction.PlaySound);
			}
			SetIcons();
		}
	}

	private int NearestSlot(float rotation)
	{
		float num = float.MaxValue;
		int result = 0;
		for (int i = 0; i < 14; i++)
		{
			float f = Mathf.DeltaAngle(rotation, SlotRotations[i]);
			f = Mathf.Abs(f);
			if (f < num)
			{
				num = f;
				result = i;
			}
		}
		return result;
	}

	private void ShiftItemsBack()
	{
		DecramentSlotIndex(ref _farthestForwardItemSlot);
		int num = _itemSlotMap[_farthestBackItemSlot];
		num--;
		if (num < 0)
		{
			num = DisplayItems.Count - 1;
		}
		DecramentSlotIndex(ref _farthestBackItemSlot);
		_itemSlotMap[_farthestBackItemSlot] = num;
	}

	private void ShiftItemsForward()
	{
		IncramentSlotIndex(ref _farthestBackItemSlot);
		int num = _itemSlotMap[_farthestForwardItemSlot];
		num++;
		if (num >= DisplayItems.Count)
		{
			num = 0;
		}
		IncramentSlotIndex(ref _farthestForwardItemSlot);
		_itemSlotMap[_farthestForwardItemSlot] = num;
	}

	private Vector3 DirectionToSpinCenter(Vector3 point)
	{
		return (point - SpinObjectTransform.position).normalized;
	}

	private void SortItems()
	{
		DisplayItems.Sort();
	}

	private void FillDisplayList()
	{
		DisplayItems.Clear();
		foreach (HatBrimItem item in Items)
		{
			if (item.ShouldBeDisplayed())
			{
				DisplayItems.Add(item);
			}
		}
	}

	private void PadDisplayList()
	{
		int count = DisplayItems.Count;
		while (DisplayItems.Count < 14)
		{
			for (int i = 0; i < count; i++)
			{
				DisplayItems.Add(DisplayItems[i]);
			}
		}
	}

	public int GetNumAffordableItems()
	{
		int num = 0;
		if (!SaveData.Instance.Progress._FinishedOneLevel.Set)
		{
			return 0;
		}
		foreach (HatBrimItem item in Items)
		{
			if (item.ShouldBeDisplayed() && item.CanAfford && item.LockState == LockState.Unlocked)
			{
				num++;
			}
		}
		return num;
	}

	public void FreezeSpinner()
	{
		StopSpinning();
		SpinnerFrozen = true;
	}

	public void UnFreezeSpinner()
	{
		SpinnerFrozen = false;
	}

	private void OnSaveDataChange()
	{
		SetIcons();
	}
}
