using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChestRewardMenu : MonoBehaviour
{
    [System.Serializable]
    public struct OptionUI
    {
        public Button button;
        public TextMeshProUGUI label;
        public Image panel;
    }

    [Header("UI")]
    public OptionUI[] options = new OptionUI[3];

    [Header("Rarity Colours")]
    public Color common = Color.gray;
    public Color uncommon = Color.green;
    public Color rare = Color.blue;
    public Color epic = new Color(0.7f, 0.2f, 1f);
    public Color legendary = new Color(1f, 0.7f, 0.1f);

    [Header("Rarity Weights")]
    public float wCommon = 60f;
    public float wUncommon = 25f;
    public float wRare = 10f;
    public float wEpic = 4f;
    public float wLegendary = 1f;

    private bool revealed = false;

    public RewardType[] CurrentRewards { get; private set; } = new RewardType[3];
    public Rarity[] CurrentRarities { get; private set; } = new Rarity[3];

    void Awake()
    {
        for (int i = 0; i < options.Length; i++)
        {
            int idx = i;
            if (options[idx].button)
                options[idx].button.onClick.AddListener(() => Choose(idx));
        }
    }

    public void OnMenuOpened()
    {
        revealed = false;
        ClearUI();
        RollThreeOptionsUnique();
        FindFirstObjectByType<RewardedAdsButton>()?.LoadAd();
    }
    public void OpenChests()
    {
        if (revealed) return;
        revealed = true;

        RefreshUI();

        for (int i = 0; i < options.Length; i++)
            if (options[i].button) options[i].button.interactable = true;
    }

    void ClearUI()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].label)
            {
                options[i].label.text = "Reward";
                options[i].label.color = Color.white;
            }

            if (options[i].panel)
                options[i].panel.color = Color.white;

            if (options[i].button)
                options[i].button.interactable = false;
        }
    }

    // Rolls three rewards and prevents duplicates
    void RollThreeOptionsUnique()
    {
        var all = (RewardType[])System.Enum.GetValues(typeof(RewardType));

        for (int i = 0; i < 3; i++)
        {
            RewardType type;
            int guard = 0;
            // Reroll if it matches previously chosen reward
            do
            {
                type = all[Random.Range(0, all.Length)];
                guard++;
            }
            while (guard < 100 && ((i > 0 && type == CurrentRewards[0]) || (i > 1 && type == CurrentRewards[1])));

            CurrentRewards[i] = type;
            CurrentRarities[i] = RollRarity();
        }
    }

    // Weighted rarity roll
    Rarity RollRarity()
    {
        float total = wCommon + wUncommon + wRare + wEpic + wLegendary;
        float r = Random.value * total;

        if ((r -= wCommon) < 0f) return Rarity.Common;
        if ((r -= wUncommon) < 0f) return Rarity.Uncommon;
        if ((r -= wRare) < 0f) return Rarity.Rare;
        if ((r -= wEpic) < 0f) return Rarity.Epic;
        return Rarity.Legendary;
    }

    Color ColourFor(Rarity rarity) => rarity switch
    {
        Rarity.Common => common,
        Rarity.Uncommon => uncommon,
        Rarity.Rare => rare,
        Rarity.Epic => epic,
        Rarity.Legendary => legendary,
        _ => common
    };

    string GetRewardDescription(RewardType reward, Rarity rarity)
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
                return $"+{Mathf.RoundToInt(5 * rarityMult)} Max Health";

            case RewardType.Heal:
                return $"+{Mathf.RoundToInt(8 * rarityMult)} HP";

            case RewardType.FireRate:
                return $"+{Mathf.RoundToInt(10 * rarityMult)}% Fire Rate";

            case RewardType.MoveSpeed:
                return $"+{Mathf.RoundToInt(8 * rarityMult)}% Move Speed";

            case RewardType.DashCooldown:
                return $"-{Mathf.RoundToInt(10 * rarityMult)}% Dash Cooldown";

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

                    return $"+{add} Projectile{(add > 1 ? "s" : "")}";
                }

            case RewardType.DashRecharge:
                return $"-{Mathf.RoundToInt(10 * rarityMult)}% Dash Reload";

            case RewardType.CritChance:
                {
                    float add = rarity switch
                    {
                        Rarity.Common => 0.5f,
                        Rarity.Uncommon => 1f,
                        Rarity.Rare => 2f,
                        Rarity.Epic => 3.5f,
                        Rarity.Legendary => 4f,
                        _ => 0.5f
                    };

                    return $"+{add:0.#}% Crit Chance";
                }

            case RewardType.CritDamage:
                return $"+{Mathf.RoundToInt(15 * rarityMult)}% Crit Damage";
        }

        return "Reward";
    }

    void RefreshUI()
    {
        for (int i = 0; i < 3 && i < options.Length; i++)
        {
            var ColourTxt = ColourFor(CurrentRarities[i]);

            if (options[i].label)
            {
                options[i].label.text = $"{GetRewardDescription(CurrentRewards[i], CurrentRarities[i])}\n{CurrentRarities[i]}";
                options[i].label.color = ColourTxt;
            }

            if (options[i].panel)
                options[i].panel.color = ColourTxt;

            if (options[i].button)
                options[i].button.interactable = true;
        }
    }

    public void Reroll()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.RerollUsed) return;

        GameManager.Instance.MarkRerollUsed();

        revealed = false;
        ClearUI();
        RollThreeOptionsUnique();
    }

    public void Choose(int index)
    {
        if (index < 0 || index >= 3) return;

        for (int i = 0; i < options.Length; i++)
            if (options[i].button) options[i].button.interactable = false;

        GameManager.Instance?.OnChestOptionChosen(CurrentRewards[index], CurrentRarities[index]);
    }
}
