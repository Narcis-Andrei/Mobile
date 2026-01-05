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
        // Convert rarity into a scaling multiplier
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
                    int amount = Mathf.CeilToInt(baseHp * rarityMult);
                    health.AddMaxHP(amount);
                    break;
                }

            case RewardType.Heal:
                {
                    int baseHeal = 8;
                    int amount = Mathf.CeilToInt(baseHeal * rarityMult);
                    health.Heal(amount);
                    break;
                }

            case RewardType.FireRate:
                {
                    // Fire rate scales multiplicatively so upgrades stack corectly
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
                    break;
                }

            case RewardType.Projectiles:
                {
                    // Projectile count increases more at higher rarities
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
                    break;
                }

            case RewardType.DashRecharge:
                {
                    float baseReduction = 0.10f;
                    dashRechargeTime *= (1f - baseReduction * rarityMult);
                    break;
                }

            case RewardType.CritChance:
                {
                    // Crit chance adds are small and clamped
                    float add = rarity switch
                    {
                        Rarity.Common => 0.005f,
                        Rarity.Uncommon => 0.010f,
                        Rarity.Rare => 0.02f,
                        Rarity.Epic => 0.035f,
                        Rarity.Legendary => 0.04f,
                        _ => 0.005f
                    };

                    critChance = Mathf.Clamp01(critChance + add);
                    break;
                }

            case RewardType.CritDamage:
                {
                    float add = 0.10f * rarityMult;
                    critDamage += add;
                    break;
                }
        }
    }
}

