using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    private void Awake()
    {
        // Auto-find Slider if not assigned in Inspector
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        // Auto-find Fill Image inside the Slider if not assigned
        if (fill == null && slider != null)
            fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;

        if (slider == null)
            Debug.LogError("[HealthBar] Slider is not assigned and could not be found automatically! Please assign it in the Inspector.", this);
    }

    public void SetMaxHealth(int health)
    {
        if (slider == null) { Debug.LogError("[HealthBar] slider is null in SetMaxHealth!", this); return; }
        slider.maxValue = health;
        slider.value = health;
        if (fill != null && gradient != null)
            fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        if (slider == null) { Debug.LogError("[HealthBar] slider is null in SetHealth!", this); return; }
        slider.value = health;
        if (fill != null && gradient != null)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
