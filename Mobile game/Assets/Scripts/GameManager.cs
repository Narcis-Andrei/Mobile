using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Menu References (children of Menus canvas)")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject settingsMenu;

    [Header("Optional")]
    [SerializeField] private PlayerHealth player;

    private enum GameState { Playing, Paused, Dead }
    private GameState _state = GameState.Playing;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerHealth>();
        Time.timeScale = 1f;
        ShowOnlyPlayerUI();
    }

    private void Update()
    {
        if (_state == GameState.Playing && player != null && player.CurrentHP <= 0)
            HandlePlayerDied();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
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

        Time.timeScale = 0f;
        Handheld.Vibrate();

        SetMenus(
            playerUiOn: false,
            pauseOn: false,
            deathOn: true,
            settingsOn: false
        );
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
        SetMenus(false, true, false, false);
    }

    public void Resume()
    {
        if (_state != GameState.Paused) return;
        _state = GameState.Playing;

        Time.timeScale = 1f;
        ShowOnlyPlayerUI();
    }

    public void OpenSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(false);
    }

    private void ShowOnlyPlayerUI()
    {
        SetMenus(true, false, false, false);
    }

    private void SetMenus(bool playerUiOn, bool pauseOn, bool deathOn, bool settingsOn)
    {
        if (playerUI != null) playerUI.SetActive(playerUiOn);
        if (pauseMenu != null) pauseMenu.SetActive(pauseOn);
        if (deathScreen != null) deathScreen.SetActive(deathOn);
        if (settingsMenu != null) settingsMenu.SetActive(settingsOn);
    }
}
