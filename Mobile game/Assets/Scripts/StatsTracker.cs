using UnityEngine;
using TMPro;

public class StatsTracker : MonoBehaviour
{
    [Header("Run UI")]
    public TMP_Text timeText;
    public TMP_Text killsText;
    public TMP_Text attemptsText;
    public TMP_Text bestText;

    [Header("Top HUD")]
    public TMP_Text hpText;

    [Header("Player Stat UI")]
    public TMP_Text moveSpeedText;
    public TMP_Text fireRateText;
    public TMP_Text dashCooldownText;

    [Header("Stat UI")]
    public TMP_Text dashesText;
    public TMP_Text dashRechargeText;
    public TMP_Text projectilesText;
    public TMP_Text critChanceText;
    public TMP_Text critDamageText;

    [Header("References")]
    public PlayerStats playerStats;
    public PlayerHealth playerHealth;
    public DashController dashController;

    public int Kills { get; private set; }
    public float TimeSurvived { get; private set; }

    const string AttemptsKey = "Attempts";
    const string BestTimeKey = "BestTime";
    const string BestKillsKey = "BestKills";

    int attempts;
    float bestTime;
    int bestKills;

    void Awake()
    {
        attempts = PlayerPrefs.GetInt(AttemptsKey, 0) + 1;
        PlayerPrefs.SetInt(AttemptsKey, attempts);

        bestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
        bestKills = PlayerPrefs.GetInt(BestKillsKey, 0);

        PlayerPrefs.Save();

        if (!playerStats) playerStats = FindFirstObjectByType<PlayerStats>();
        if (!playerHealth) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (!dashController) dashController = FindFirstObjectByType<DashController>();

        RefreshUI();
    }

    void Update()
    {
        if (Time.timeScale <= 0f) { RefreshUI(); return; }

        if (playerHealth && playerHealth.CurrentHP <= 0)
        {
            RefreshUI();
            return;
        }

        TimeSurvived += Time.deltaTime;
        RefreshUI();
    }

    public void AddKill(int amount = 1)
    {
        Kills += Mathf.Max(1, amount);
        RefreshUI();
    }

    public void OnRunEnded()
    {
        bool newBest = false;

        if (TimeSurvived > bestTime)
        {
            bestTime = TimeSurvived;
            PlayerPrefs.SetFloat(BestTimeKey, bestTime);
            newBest = true;
        }

        if (Kills > bestKills)
        {
            bestKills = Kills;
            PlayerPrefs.SetInt(BestKillsKey, bestKills);
            newBest = true;
        }

        if (newBest) PlayerPrefs.Save();

        RefreshUI();
    }

    void RefreshUI()
    {
        if (timeText) timeText.text = $"Time: {FormatTime(TimeSurvived)}";
        if (killsText) killsText.text = $"Kills: {Kills}";
        if (attemptsText) attemptsText.text = $"Attempts: {attempts}";
        if (bestText) bestText.text = $"Best: {FormatTime(bestTime)}  |  Best Kills: {bestKills}";

        if (playerHealth && hpText)
            hpText.text = $"HP: {playerHealth.CurrentHP} / {playerHealth.MaxHP}";

        if (playerStats)
        {
            if (moveSpeedText) moveSpeedText.text = $"Move: x{playerStats.moveSpeedMultiplier:0.00}";
            if (fireRateText) fireRateText.text = $"Fire: x{playerStats.fireRateMultiplier:0.00}";
            if (dashCooldownText) dashCooldownText.text = $"Dash CD: x{playerStats.dashCooldownMultiplier:0.00}";

            if (projectilesText) projectilesText.text = $"Projectiles: {Mathf.Max(1, playerStats.projectileCount)}";
            if (dashRechargeText) dashRechargeText.text = $"DashRl: {Mathf.Max(0.1f, playerStats.dashRechargeTime):0.00}s";
            if (critChanceText) critChanceText.text = $"CritChance: {(Mathf.Clamp01(playerStats.critChance) * 100f):0.00}%";
            if (critDamageText) critDamageText.text = $"CritDmg: x{Mathf.Max(1f, playerStats.critDamage):0.00}";
        }

        if (dashesText)
        {
            int current = dashController ? dashController.CurrentCharges : 0;
            int max = playerStats ? Mathf.Max(1, playerStats.maxDashCharges) : 1;
            dashesText.text = $"Dashes: {current}/{max}";
        }
    }

    static string FormatTime(float t)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(t));
        int m = total / 60;
        int s = total % 60;
        return $"{m:00}:{s:00}";
    }
}
