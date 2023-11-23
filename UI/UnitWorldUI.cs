using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UnitWorldUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private Unit unit;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private HealthSystem healthSystem;

    private void OnEnable()
    {
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged;
        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;
        //if (unit.IsEnemy())
        //{
        //    actionPointsText.gameObject.SetActive(false);
        //}
        //else
        //{
        //    actionPointsText.gameObject.SetActive(true);
        //}
    }

    private void OnDisable()
    {
        Unit.OnAnyActionPointsChanged -= Unit_OnAnyActionPointsChanged;
        healthSystem.OnHealthChanged -= HealthSystem_OnHealthChanged;
    }

    private void Start()
    {
        UpdateActionPointsText();
        UpdateHealthBarImage();
    }

    private void UpdateActionPointsText()
    {
        int ap = unit.GetActionPoints();
        actionPointsText.text = ap.ToString();
        if (ap <= 0)
        {
            actionPointsText.color = Color.red;
        }
        else
        {
            actionPointsText.color = Color.white;
        }
    }

    private void UpdateHealthBarImage()
    {
        float healthPercent = healthSystem.GetHealthPercent();
        healthBarImage.fillAmount = healthPercent;
        if (healthPercent >= 1) // 100% health
        {
            healthBarImage.color = Color.green;
        }
        else if (healthPercent > 0.5) // Between 50% and 100% health
        {
            healthBarImage.color = Color.yellow;
        }
        else // 50% or less
        {
            healthBarImage.color = Color.red;
        }
    }

    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        UpdateActionPointsText();
    }

    private void HealthSystem_OnHealthChanged(object sender, EventArgs e)
    {
        UpdateHealthBarImage();
    }
}
