public interface IStatModifier
{
    float Apply(float baseValue);
    int Order { get; }
}
