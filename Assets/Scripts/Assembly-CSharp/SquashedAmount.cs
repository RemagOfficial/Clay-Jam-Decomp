using UnityEngine;

public class SquashedAmount : MonoBehaviour
{
	public delegate void AddAmountHandler(int amount);

	private const string AnimationName = "SquashedAmount";

	public TextMesh _NumberText;

	public float _NormalisedAnimTimeToUpdateHud = 0.25f;

	private int _amount;

	private bool _collectionDisplayed;

	private AddAmountHandler AddAmountCallback;

	public Material HSVMaterial { get; private set; }

	private void Awake()
	{
		HSVMaterial = base.renderer.material;
	}

	private void Update()
	{
		bool flag = false;
		if (!base.animation.IsPlaying("SquashedAmount"))
		{
			Object.Destroy(base.gameObject);
			flag = true;
		}
		else if (!_collectionDisplayed)
		{
			AnimationState animationState = base.animation["SquashedAmount"];
			if (animationState.normalizedTime > _NormalisedAnimTimeToUpdateHud)
			{
				flag = true;
			}
		}
		if (!_collectionDisplayed && flag)
		{
			AddAmountCallback(_amount);
			_collectionDisplayed = true;
		}
	}

	public void Begin(int amount, HSVColour colour, AddAmountHandler addCallback)
	{
		_collectionDisplayed = false;
		_amount = amount;
		_NumberText.text = amount.ToString();
		colour.UseOnHSVMaterial(HSVMaterial);
		AddAmountCallback = addCallback;
	}
}
