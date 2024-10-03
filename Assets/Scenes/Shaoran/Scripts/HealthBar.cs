using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Healthbar : MonoBehaviour
{
    [SerializeField] private Image _healthbarSprite;
    [SerializeField] private TMP_Text _healthbarText;

    private void Awake()
    {
        Enemy.onEnemyHurt  += OnHurtEvent;
        Character.onCharacterHurt += OnHurtEvent;
    }

    private void OnDestroy()
    {
        Enemy.onEnemyHurt -= OnHurtEvent;
        Character.onCharacterHurt -= OnHurtEvent;
    }

    private void OnHurtEvent(float maxHealth, float currentHealth, GameObject hurtObject)
    {
        if (hurtObject == this.gameObject) { UpdateHealthBar(maxHealth, currentHealth); }
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth)
    {
        Debug.Log("UpdatedHealthBar");
        _healthbarSprite.fillAmount = currentHealth / maxHealth;
        _healthbarText.SetText(currentHealth.ToString());
    }
}