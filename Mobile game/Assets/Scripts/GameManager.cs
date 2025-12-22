using UnityEngine;
using UnityEngine.SceneManagement;
using CandyCoded.HapticFeedback;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Menu References")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject statsMenu;

    [Header("Player health UI")]
    [SerializeField] private PlayerHealth player;

    [Header("Collectables")]
    [SerializeField] private GameObject collectablesMenu;
    [SerializeField] private bool pauseOnCollect = true;

    private enum GameState { Playing, Paused, Dead }
    private GameState _state = GameState.Playing;
    private bool _collectablesOpen = false;
    private bool _rerollUsed = false;
    private bool _pauseBySettings = false;
    private StatsTracker _runStats;

    public bool RerollUsed => _rerollUsed;
    public void ResetReroll() => _rerollUsed = false;
    public void MarkRerollUsed() => _rerollUsed = true;
    public bool IsCollectablesOpen => _collectablesOpen;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _runStats = FindFirstObjectByType<StatsTracker>();
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();
        Time.timeScale = 1f;
        if (statsMenu) statsMenu.SetActive(true);

        ShowOnlyPlayerUI();
        if (collectablesMenu) collectablesMenu.SetActive(false);
    }

    private void Update()
    {
        if (_state == GameState.Playing && player != null && player.CurrentHP <= 0)
            HandlePlayerDied();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_collectablesOpen)
            {
                HideCollectableMenu(); return;
            }
            if (_state == GameState.Playing) Pause();
            else if (_state == GameState.Paused && settingsMenu != null && settingsMenu.activeSelf) CloseSettings();
            else if (_state == GameState.Paused) Resume();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RetryRun();
        }
    }

    private void HandlePlayerDied()
    {
        if (_state == GameState.Dead) return;
        _state = GameState.Dead;

        _runStats?.OnRunEnded();

        HapticFeedback.HeavyFeedback();

        Time.timeScale = 0f;

        SetMenus(
            playerUiOn: false,
            pauseOn: false,
            deathOn: true,
            settingsOn: false
        );

        if (statsMenu) statsMenu.SetActive(false);

        if (collectablesMenu) collectablesMenu.SetActive(false);
        _collectablesOpen = false;
        _pauseBySettings = false;
    }

    public void RetryRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Pause()
    {
        if (_state != GameState.Playing) return;
        _state = GameState.Paused;

        Time.timeScale = 0f;
        _pauseBySettings = false;
        SetMenus(false, true, false, false);
    }

    public void Resume()
    {
        if (_state != GameState.Paused) return;
        _state = GameState.Playing;

        Time.timeScale = 1f;
        _pauseBySettings = false;
        ShowOnlyPlayerUI();
    }

    public void OpenSettings()
    {
        if (_state == GameState.Dead) return;

        if (_state == GameState.Playing)
        {
            Time.timeScale = 0f;
            _state = GameState.Paused;
            _pauseBySettings = true;
        }
        else
        {
            _pauseBySettings = false;
        }

        SetMenus(false, false, false, true);
    }

    public void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(false);

        if (_pauseBySettings)
        {
            _pauseBySettings = false;
            _state = GameState.Playing;
            Time.timeScale = 1f;
            ShowOnlyPlayerUI();
        }
        else
        {
            if (_state == GameState.Paused)
                SetMenus(false, true, false, false);
        }
    }

    private void ShowOnlyPlayerUI()
    {
        SetMenus(true, false, false, false);

        if (statsMenu) statsMenu.SetActive(true);

        if (collectablesMenu) collectablesMenu.SetActive(false);
        _collectablesOpen = false;
    }

    private void SetMenus(bool playerUiOn, bool pauseOn, bool deathOn, bool settingsOn)
    {
        if (playerUI != null) playerUI.SetActive(playerUiOn);
        if (pauseMenu != null) pauseMenu.SetActive(pauseOn);
        if (deathScreen != null) deathScreen.SetActive(deathOn);
        if (settingsMenu != null) settingsMenu.SetActive(settingsOn);
    }

    public void ShowCollectablesMenu()
    {
        if (_state == GameState.Dead) return;

        SetMenus(false, false, false, false);
        if (collectablesMenu) collectablesMenu.SetActive(true);
        _collectablesOpen = true;

        ResetReroll();
        collectablesMenu?.GetComponentInChildren<ChestRewardMenu>(true)?.OnMenuOpened();

        if (pauseOnCollect) Time.timeScale = 0f;
    }

    public void HideCollectableMenu()
    {
        if (!_collectablesOpen) return;

        if (collectablesMenu) collectablesMenu.SetActive(false);
        _collectablesOpen = false;
        ResetReroll();

        if (pauseOnCollect && _state == GameState.Playing)
            Time.timeScale = 1f;

        SetMenus(true, false, false, false);
    }

    public void OnChestOptionChosen(RewardType reward, Rarity rarity)
    {
        var stats = player ? player.GetComponent<PlayerStats>() : null;
        if (stats && player)
            stats.ApplyReward(reward, rarity, player);

        HideCollectableMenu();
    }

    public void TryOpenChestByFlick(FlickDir dir)
    {
        if (!_collectablesOpen) return;
        if (dir != FlickDir.Up) return;

        collectablesMenu
            ?.GetComponentInChildren<ChestRewardMenu>(true)
            ?.OpenChests();
    }
}