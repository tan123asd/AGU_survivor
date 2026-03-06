using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "NewUpgrade")]
public class UpgradeData : ScriptableObject
{
    public string displayName;
    public string description;
    public Sprite icon;
    public BuffEffect[] buffEffects;
    [System.Serializable]
    public struct BuffEffect
    {
        public string effectName;
        public float effectValue;
        public bool isPercentage;
    }
    public IStatModifier[] GetStatModifiers()
    {
        IStatModifier[] mods = new IStatModifier[buffEffects.Length];
        for (int i = 0; i < buffEffects.Length; i++)
        {
            var effect = buffEffects[i];
            if (effect.isPercentage)
            {
                mods[i] = new PercentageMultiplyModifier { Multiplier = 1 + effect.effectValue };
            }
            else
            {
                mods[i] = new FlatAddModifier { Amount = effect.effectValue };
            }
        }

        return mods;
    }
}
