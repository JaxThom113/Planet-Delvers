using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    [Header("HP/EP")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpValue;
    [SerializeField] private Slider epSlider;
    [SerializeField] private TextMeshProUGUI epValue;

    void Update()
    {
        UpdateHealth();
        UpdateEnergy();
    }

    private void UpdateHealth()
    {
        hpSlider.maxValue = GameSystem.Instance.playerMaxHealth;
        hpSlider.value = GameSystem.Instance.playerCurrentHealth;

        hpValue.text = GameSystem.Instance.playerCurrentHealth.ToString();
    }

    private void UpdateEnergy()
    {
        epSlider.maxValue = GameSystem.Instance.playerMaxEnergy;
        epSlider.value = GameSystem.Instance.playerCurrentEnergy;

        epValue.text = GameSystem.Instance.playerCurrentEnergy.ToString();
    }
}
