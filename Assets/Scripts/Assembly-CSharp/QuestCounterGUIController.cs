using UnityEngine;

public class QuestCounterGUIController : MonoBehaviour
{
	public UILabel _counterComponent;

	public LocalisableText _textComponent;

	public GameObject _IconParent;

	private bool animatingOut;

	private static int FailedIconIndex = 24;

	public UISpriteAnimationControlled Icon { get; set; }

	private void Update()
	{
		if (!base.gameObject.active)
		{
			return;
		}
		if (animatingOut)
		{
			if (!base.animation.isPlaying)
			{
				base.gameObject.SetActiveRecursively(false);
			}
		}
		else if (CurrentQuest.Instance.HasQuest && Icon.CurrentFrame != FailedIconIndex)
		{
			if (CurrentQuest.Instance.CannotComplete)
			{
				MarkFailed();
			}
			else if (CurrentQuest.Instance.HasQuestWithProgressCounter)
			{
				UpdateAsCounter();
			}
			else
			{
				UpdateAsText();
			}
		}
	}

	public void TurnOn(bool on)
	{
		if (on && CurrentQuest.Instance.HasQuest)
		{
			if (!base.gameObject.active)
			{
				base.gameObject.SetActiveRecursively(true);
				if (CurrentQuest.Instance.HasQuestWithProgressCounter)
				{
					_textComponent.Activate(false);
				}
				else
				{
					_counterComponent.gameObject.active = false;
				}
				Icon.GotoFrame(CurrentQuest.Instance.IconIndex);
				animatingOut = false;
				base.animation.Play("QuestCounterIn");
			}
		}
		else if (!animatingOut && base.gameObject.active)
		{
			base.animation.Play("QuestCounterOut");
			animatingOut = true;
		}
	}

	private void UpdateAsCounter()
	{
		int target = CurrentQuest.Instance.Target;
		int num = target;
		if (!CurrentQuest.Instance.QuestComplete)
		{
			num = CurrentQuest.Instance.Progress;
		}
		string text = string.Format("{0}/{1}", num, target);
		_counterComponent.text = text;
	}

	private void UpdateAsText()
	{
		_textComponent.text = CurrentQuest.Instance.CounterText;
	}

	private void MarkFailed()
	{
		Icon.GotoFrame(FailedIconIndex);
		InGameAudio.PostFabricEvent("QuestFailed");
		_textComponent.text = Localization.instance.Get("QUESTS_Failed");
		_textComponent.Activate(true);
		_counterComponent.gameObject.active = false;
	}
}
