public class FlatAddModifier : IStatModifier
{
    public float Amount;
    public int Order => 10;
    public float Apply(float baseValue)
    {
        return baseValue + Amount;
    }
}
