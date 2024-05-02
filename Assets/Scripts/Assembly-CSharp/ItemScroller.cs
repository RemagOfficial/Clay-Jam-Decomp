using System.Collections.Generic;
using UnityEngine;

public class ItemScroller : IAPItemSelector
{
	private const float VIEW_OFFSET = -0.12f;

	private const float VIEW_RANGE = 0.85f;

	private const float MENU_DECELERATION = 0.002f;

	private const float MOUSE_CLICK_TIME = 0.25f;

	private const float MOUSE_MAX_MOVE_FOR_CLICK = 0f;

	private const float MOUSE_SCROLL_MULTIPLIER = 1f;

	private const int MOUSE_SPEED_FRAMES = 4;

	private const float SWIPE_SPEED_MULTIPLIER = 0.05f;

	private const float MENU_SCROLL_DAMPING = 0.25f;

	private const float MENU_SNAP_SPEED = 0.01f;

	private const float MAX_MENU_SPEED = 0.07f;

	public List<GameObject> _ItemPanels;

	public bool IsEnabled;

	public Camera _UIcamera;

	private List<StoreProductVO> _products;

	private bool _isInitialised;

	private int _numMenuItems;

	private float _menuLength;

	private float _menuPos;

	private float _itemStep;

	private float _menuSpeed;

	private int[] _panelToItemMap;

	private float _nearestItemDist;

	private bool _isNextItemCloser;

	private bool _isMouseDown;

	private float _mousePrevPos;

	private float _mouseDownElapsed;

	private float _mouseSwipeSpeed;

	private float _mouseTotalDistance;

	private float[] _mousePrevSpeeds;

	private int _mouseSpeedIndex;

	private bool _isAwake;

	public override bool IsAwake
	{
		get
		{
			return _isAwake;
		}
	}

	private void Awake()
	{
		Debug.Log("ItemScroller.Awake(): Start");
		HidePanels();
		_mousePrevSpeeds = new float[4];
		_panelToItemMap = new int[_ItemPanels.Count];
		_isAwake = true;
		FireAwakeEvent();
	}

	private void Update()
	{
		if (IsEnabled)
		{
			SetMenuPos();
			PositionItemPanels();
			CheckInput();
		}
	}

	public override void Init(List<StoreProductVO> storeProducts)
	{
		Debug.Log("ItemScroller.Init(): _initialised: " + _isAwake);
		if (_isAwake)
		{
			_numMenuItems = storeProducts.Count;
			_products = storeProducts;
			InitItemPanels();
			IsEnabled = true;
			_isInitialised = true;
		}
	}

	private void OnDisable()
	{
		Debug.Log("ItemScroller.Disable()");
		IsEnabled = false;
		_isInitialised = false;
		_numMenuItems = 0;
		_products = null;
		HidePanels();
	}

	public void Disable()
	{
		IsEnabled = false;
	}

	public void Enable()
	{
		if (_isInitialised)
		{
			IsEnabled = true;
		}
	}

	private void HidePanels()
	{
		foreach (GameObject itemPanel in _ItemPanels)
		{
			itemPanel.SetActiveRecursively(false);
		}
	}

	private void ShowPanels()
	{
		foreach (GameObject itemPanel in _ItemPanels)
		{
			itemPanel.SetActiveRecursively(true);
		}
	}

	private void InitItemPanels()
	{
		_itemStep = 0.85f / (float)_ItemPanels.Count;
		_menuLength = _itemStep * (float)_numMenuItems - _itemStep * (float)(_ItemPanels.Count - 1);
		_menuPos = 0f;
		PositionItemPanels();
		ShowPanels();
	}

	private void CheckInput()
	{
		if (Input.GetMouseButtonDown(0))
		{
			_isMouseDown = true;
			_mouseDownElapsed = 0f;
			_mouseTotalDistance = 0f;
			_mouseSwipeSpeed = 0f;
			_mousePrevPos = Input.mousePosition.y;
			_menuSpeed = 0f;
			_mouseSpeedIndex = 0;
			_nearestItemDist = 0f;
			_mousePrevSpeeds.Initialize();
		}
		if (!_isMouseDown)
		{
			return;
		}
		_mouseDownElapsed += Time.deltaTime;
		float num = _mousePrevPos - Input.mousePosition.y;
		float num2 = num / (float)Screen.height;
		_mouseTotalDistance += Mathf.Abs(num2);
		_mousePrevPos = Input.mousePosition.y;
		float num3 = num2 / Time.deltaTime;
		_mousePrevSpeeds[_mouseSpeedIndex++ % 4] = num3;
		if (_mouseDownElapsed > 0.25f)
		{
			_menuPos -= num2 * 1f;
		}
		if (Input.GetMouseButtonUp(0))
		{
			_isMouseDown = false;
			if (_mouseDownElapsed <= 0.25f && _mouseTotalDistance <= 0f)
			{
				OnMouseClick();
			}
			else
			{
				OnMouseSwipe();
			}
		}
	}

	private void SetMenuPos()
	{
		_menuPos -= _menuSpeed;
		if (_menuSpeed < 0f)
		{
			_menuSpeed += 0.002f;
			if (_menuSpeed >= 0f)
			{
				_menuSpeed = 0f;
				snapToNearestItem();
			}
		}
		else if (_menuSpeed > 0f)
		{
			_menuSpeed -= 0.002f;
			if (_menuSpeed <= 0f)
			{
				_menuSpeed = 0f;
				snapToNearestItem();
			}
		}
		if (_menuPos > _menuLength)
		{
			_menuSpeed = 0f - _menuSpeed;
			_menuSpeed *= 0.25f;
			_menuPos = _menuLength;
		}
		else if (_menuPos < 0f)
		{
			_menuSpeed = 0f - _menuSpeed;
			_menuSpeed *= 0.25f;
			_menuPos = 0f;
		}
		if (_nearestItemDist > 0f)
		{
			if (_isNextItemCloser)
			{
				_menuPos += Mathf.Min(0.01f, _nearestItemDist);
			}
			else
			{
				_menuPos -= Mathf.Min(0.01f, _nearestItemDist);
			}
			_nearestItemDist -= 0.01f;
		}
	}

	private void PositionItemPanels()
	{
		for (int i = 0; i < _ItemPanels.Count; i++)
		{
			float num = ((float)i * _itemStep + _itemStep - _menuPos) % 0.85f;
			if (num < 0f)
			{
				num += 0.85f;
			}
			float num2 = _menuPos + num - _itemStep;
			int value = Mathf.RoundToInt(num2 / _itemStep);
			value = Mathf.Clamp(value, 0, _numMenuItems - 1);
			_panelToItemMap[i] = value;
			float num3 = 0.85f - _itemStep;
			if (num < _itemStep)
			{
				float num4 = Mathf.Clamp(num / _itemStep, 0.01f, 1f);
				num += _itemStep * (1f - num4) * 0.5f;
				_ItemPanels[i].transform.localScale = new Vector3(num4, num4, 1f);
			}
			else if (num > num3)
			{
				float num5 = 1f - (num - num3) / _itemStep;
				num -= _itemStep * (1f - num5) * 0.5f;
				_ItemPanels[i].transform.localScale = new Vector3(num5, num5, 1f);
			}
			else
			{
				_ItemPanels[i].transform.localScale = Vector3.one;
			}
			_ItemPanels[i].GetComponent<UIAnchor>().relativeOffset.y = -0.12f - num;
			_ItemPanels[i].transform.FindChild("Amount").GetComponent<UILabel>().text = _products[value].description;
			_ItemPanels[i].transform.FindChild("Description").GetComponent<UILabel>().text = _products[value].title;
			_ItemPanels[i].transform.FindChild("Cost").GetComponent<UILabel>().text = _products[value].formattedPrice;
		}
	}

	private void snapToNearestItem()
	{
		float num = _menuPos % _itemStep;
		if (num > _itemStep / 2f)
		{
			_nearestItemDist = _itemStep - num;
			_isNextItemCloser = true;
		}
		else
		{
			_nearestItemDist = num;
			_isNextItemCloser = false;
		}
	}

	private void OnMouseClick()
	{
		if (_UIcamera == null)
		{
			return;
		}
		Ray ray = _UIcamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		if (!Physics.Raycast(ray, out hitInfo))
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < _ItemPanels.Count; i++)
		{
			if (_ItemPanels[i] == hitInfo.collider.gameObject)
			{
				num = _panelToItemMap[i];
				break;
			}
		}
		if (num > -1)
		{
			FireItemSelectedEvent(_products[num].productIdentifier);
		}
	}

	private void OnMouseSwipe()
	{
		_mouseSwipeSpeed = 0f;
		float[] mousePrevSpeeds = _mousePrevSpeeds;
		foreach (float num in mousePrevSpeeds)
		{
			_mouseSwipeSpeed += num;
		}
		_mouseSwipeSpeed /= 4f;
		_menuSpeed = Mathf.Clamp(_mouseSwipeSpeed * 0.05f, -0.07f, 0.07f);
		if (_menuSpeed == 0f)
		{
			snapToNearestItem();
		}
	}
}
