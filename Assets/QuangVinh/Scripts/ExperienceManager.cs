using System.Collections;
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
    [SerializeField] private ParticleSystem levelUpEffect;
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
            GameObject player = GameObject.FindWithTag("Player");
            if (levelUpEffect != null && player != null)
            {
                ParticleSystem psInstance = Instantiate(levelUpEffect, player.transform.position + new Vector3(0, -0.5f, 0), Quaternion.identity);
                psInstance.transform.SetParent(player.transform);
                StartCoroutine(StopFollowingAndDestroy(psInstance.gameObject, psInstance.main.duration));
            }
        }
    }

    private IEnumerator StopFollowingAndDestroy(GameObject psObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(psObj != null)
        {
            psObj.transform.SetParent(null);
            ParticleSystem ps = psObj.GetComponent<ParticleSystem>();
            if(ps != null)
            {
                Destroy(ps.gameObject, ps.main.duration);
            }
            
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
