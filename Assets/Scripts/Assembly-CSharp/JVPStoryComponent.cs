using System.Collections.Generic;

public class JVPStoryComponent : StoryComponent
{
	protected override List<AnimatedSprite> SpriteAnimations
	{
		get
		{
			return JVPController.Instance.SpriteAnimations;
		}
	}
}
