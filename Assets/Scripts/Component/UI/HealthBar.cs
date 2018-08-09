using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    private IHealth iHealth;
    private float originalWidth;

    private void Start()
    {
        originalWidth = healthBar.sizeDelta.x;
        iHealth = GetComponentInParent<IHealth>();
        if (iHealth == null)
        {
            Destroy(gameObject);
            return;
        }
        iHealth.OnHealthChanged += OnHealthChanged;
    }

    /**<summary> Called when health of the target is changed </summary>*/
    public void OnHealthChanged(int health, int maxHealth)
    {
        DOTween.To(() => healthBar.sizeDelta, x => healthBar.sizeDelta = x, new Vector2(health / (float)maxHealth * originalWidth, healthBar.sizeDelta.y), 0.5f).SetEase(Ease.OutSine);
        healthText.text = health + "/" + maxHealth;
    }
}
