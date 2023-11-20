using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    public int currentHealth { private set; get; }
    public event EventHandler OnDead;
    public event EventHandler OnHealthChanged;
    public event EventHandler OnRecieveDamage;

    private void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(int damage)
    {
        Mathf.Clamp(currentHealth -= damage, 0, maxHealth);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
        OnRecieveDamage?.Invoke(this, EventArgs.Empty);
        if (IsDead())
        {
            OnDead?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ApplyHealing(int healing)
    {
        Mathf.Clamp(currentHealth += healing, 0, maxHealth);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }
}
