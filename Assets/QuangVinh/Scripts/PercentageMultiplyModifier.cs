using UnityEngine;

public class PercentageMultiplyModifier : IStatModifier
{
    public float Multiplier; // e.g., 1.2 for +20%
    public int Order => 20;
    public float Apply(float baseValue)
    {
        return baseValue * Multiplier;
    }
}
