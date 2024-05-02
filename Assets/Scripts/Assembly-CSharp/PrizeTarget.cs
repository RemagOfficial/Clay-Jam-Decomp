public class PrizeTarget
{
	public int Beans { get; private set; }

	public PrizeTarget(PrizeDefinition prizeDef)
	{
		if (prizeDef == null)
		{
			Beans = -1;
		}
		else
		{
			Beans = prizeDef.Beans;
		}
	}
}
