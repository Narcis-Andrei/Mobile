using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("health")]
    public int MaxHP = 20;
    public int CurrentHP;

    [Header("UI")]
    public Image HpFill;

    [Header("DamageCooldown")]
    public float IFrameTime = 0.5f;

    float _lastHit = -999f;

    private void Awake()
    {
        CurrentHP = MaxHP;
        UpdateUI();
    }
    public bool CanTakeDamage => Time.time - _lastHit >= IFrameTime;

    public void TakeDamage(int amount)
    {
        if (!CanTakeDamage) return;

        _lastHit = Time.time;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        UpdateUI();

        if (CurrentHP <= 0)
        {
            Debug.Log("Player died");
        }
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (HpFill) HpFill.fillAmount = Mathf.Clamp01(CurrentHP / (float)MaxHP);
    }

    public void AddMaxHP(int amount, bool alsoHealByAmount = true)
    {
        if (amount <= 0) return;

        MaxHP += amount;

        if (alsoHealByAmount)
            CurrentHP += amount;

        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        UpdateUI();
    }

}
