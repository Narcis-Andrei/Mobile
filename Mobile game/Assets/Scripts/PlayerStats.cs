using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Multipliers")]
    public float moveSpeedMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float dashCooldownMultiplier = 1f;

    [Header("Flat bonuses")]
    public int bonusMaxHP = 0;

    public void ApplyReward(RewardType reward, Rarity rarity, PlayerHealth health)
    {
        float rarityMult = rarity switch
        {
            Rarity.Common => 1f,
            Rarity.Uncommon => 1.25f,
            Rarity.Rare => 1.5f,
            Rarity.Epic => 2f,
            Rarity.Legendary => 3f,
            _ => 1f
        };

        switch (reward)
        {
            case RewardType.MaxHealth:
                {
                    int baseHp = 5;
                    int amount = Mathf.RoundToInt(baseHp * rarityMult);
                    health.AddMaxHP(amount);
                    break;
                }
            case RewardType.Heal:
                {
                    int baseHeal = 8;
                    int amount = Mathf.RoundToInt(baseHeal * rarityMult);
                    health.Heal(amount);
                    break;
                }
            case RewardType.FireRate:
                {
                    float baseBonus = 0.10f;
                    fireRateMultiplier *= (1f + baseBonus * rarityMult);
                    break;
                }
            case RewardType.MoveSpeed:
                {
                    float baseBonus = 0.08f;
                    moveSpeedMultiplier *= (1f + baseBonus * rarityMult);
                    break;
                }
            case RewardType.DashCooldown:
                {
                    float baseReduction = 0.10f;
                    dashCooldownMultiplier *= (1f - baseReduction * rarityMult);
                    dashCooldownMultiplier = Mathf.Clamp(dashCooldownMultiplier, 0.4f, 1f);
                    break;
                }
        }
    }
}
