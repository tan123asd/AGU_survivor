using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour 
{
    [SerializeField] private AnimationCurve expCurve;

    private int currentLevel;
    private int currentExp;
    private int previousLevelExp;
    private int nextLevelExp;

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider expSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddExp(int exp)
    {
        currentExp += exp;
        CheckForLevelUp();
        UpdateInterface();
    }

    private void CheckForLevelUp()
    {
        if (currentExp >= nextLevelExp)
        {
            currentLevel++;
            UpdateLevel();
        }

    }

    private void UpdateLevel()
    {
        previousLevelExp = (int)expCurve.Evaluate(currentLevel);
        nextLevelExp = (int)expCurve.Evaluate(currentLevel + 1);
        UpdateInterface();
    }

    private void UpdateInterface()
    {
        float expFill = 0f;
        levelText.text = currentLevel.ToString();
        if (nextLevelExp > previousLevelExp)
        {
            expFill = (float)(currentExp - previousLevelExp) / (nextLevelExp - previousLevelExp);
        }
        expSlider.value = expFill;
    }
}
