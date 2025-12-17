using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Multipliers")]
    public float moveSpeedMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float dashCooldownMultiplier = 1f;

    [Header("Stats")]
    public int bonusMaxHP = 0;
    [Min(1)] public int projectileCount = 1;
    [Range(1, 4)] public int maxDashCharges = 4;
    [Min(0.05f)] public float dashRechargeTime = 1.25f;
    [Range(0f, 1f)] public float critChance = 0f;
    [Min(1f)] public float critDamage = 1.5f;

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
            case RewardType.Projectiles:
                {
                    int add = rarity switch
                    {
                        Rarity.Common => 1,
                        Rarity.Uncommon => 2,
                        Rarity.Rare => 3,
                        Rarity.Epic => 4,
                        Rarity.Legendary => 5,
                        _ => 1
                    };

                    projectileCount += add;
                    projectileCount = Mathf.Clamp(projectileCount, 1, 20);
                    break;
                }

            case RewardType.DashRecharge:
                {
                    float baseReduction = 0.10f;
                    dashRechargeTime *= (1f - baseReduction * rarityMult);
                    dashRechargeTime = Mathf.Clamp(dashRechargeTime, 0.25f, 5f);
                    break;
                }

            case RewardType.CritChance:
                {
                    float add = 0.02f * rarityMult;
                    critChance = Mathf.Clamp01(critChance + add);
                    break;
                }

            case RewardType.CritDamage:
                {
                    float add = 0.10f * rarityMult;
                    critDamage = Mathf.Clamp(critDamage + add, 1f, 5f);
                    break;
                }
        }
    }
}

