using UnityEngine;
using UnityEngine.UI; // Slider ve Image gibi standart UI elemanları için
using TMPro; // TextMeshPro kullanıyorsak
using System.Collections.Generic; // List<T> için
using UnityEngine.SceneManagement; // Sahne yönetimi için

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player UI Elements")]
    public TextMeshProUGUI playerHealthText;
    public Slider playerHealthSlider;
    
    [Header("Spell Cooldown UI Elements")]
    [Tooltip("Image component to show cooldown progress (Image Type should be Filled).")]
    public Image spellCooldownImage; 
    [Tooltip("Text component to show remaining cooldown time (optional).")]
    public TextMeshProUGUI spellCooldownText; 

    [Header("Level Up UI Elements")]
    [Tooltip("The main panel GameObject for the level up choices.")]
    public GameObject levelUpPanel; 
    [Tooltip("Array of UI slot configurations for displaying spell/upgrade choices.")]
    public SpellChoiceSlotUI[] spellChoiceSlots; 

    [Header("Pause Menu UI Elements")]
    [Tooltip("The main panel GameObject for the pause menu.")]
    public GameObject pauseMenuPanel;

    [Header("Game Over/Victory UI Elements")]
    [Tooltip("The panel GameObject to show when all waves are cleared.")]
    public GameObject victoryPanel;
    [Tooltip("Text component on Victory Panel to display elapsed time.")]
    public TextMeshProUGUI victoryPanelTimeText; 
    [Tooltip("The panel GameObject to show when the player dies.")]
    public GameObject gameOverPanel; 
    [Tooltip("Text component on Game Over Panel to display elapsed time.")]
    public TextMeshProUGUI gameOverPanelTimeText; 

    [Header("In-Game Timer UI (Opsiyonel)")]
    [Tooltip("Text component to display the elapsed time during gameplay (optional).")]
    public TextMeshProUGUI inGameTimerText;

    public string playerTag = "Player"; 
    public string mainMenuSceneName = "MainMenu"; 

    private bool isPaused = false;
    private float sessionElapsedTime = 0f;
    private bool timerIsRunning = false;

    [System.Serializable]
    public class SpellChoiceSlotUI
    {
        public GameObject slotRoot; 
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public Button selectButton;
    }

    public struct SpellUpgradeChoice 
    {
        public string spellName;
        public string description;
        public Sprite icon;
        public int choiceIndex; 
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        EnemySpawner.OnAllWavesCompletedAndCleared += HandleAllWavesCleared;
        PlayerHealth.onPlayerDied += HandlePlayerDied; 
    }

    void OnDisable()
    {
        EnemySpawner.OnAllWavesCompletedAndCleared -= HandleAllWavesCleared;
        PlayerHealth.onPlayerDied -= HandlePlayerDied; 
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                UpdatePlayerHealthUI(playerHealth.CurrentHealth, playerHealth.maxHealth); 
                playerHealth.onHealthChanged.AddListener(UpdatePlayerHealthUI);
            }
            else
            {
                Debug.LogError("UIManager: Player object does not have a PlayerHealth component.");
            }
        }
        else
        {
            Debug.LogError($"UIManager: Could not find GameObject with tag '{playerTag}'. Player health UI might not work.");
        }

        UpdateSpellCooldownUI(0, 1); 
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        StartSessionTimer(); 
    }

    void Update()
    {
        if (Instance != this) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if ((levelUpPanel != null && levelUpPanel.activeSelf) || 
                (victoryPanel != null && victoryPanel.activeSelf) || 
                (gameOverPanel != null && gameOverPanel.activeSelf))
            {
                return;
            }
            TogglePauseMenu();
        }

        if (timerIsRunning)
        {
            sessionElapsedTime += Time.deltaTime; 
            if (inGameTimerText != null && inGameTimerText.gameObject.activeInHierarchy)
            {
                inGameTimerText.text = FormatTime(sessionElapsedTime);
            }
        }
    }

    public void StartSessionTimer()
    {
        sessionElapsedTime = 0f;
        timerIsRunning = true;
        Debug.Log("Session Timer Started.");
        if (inGameTimerText != null) inGameTimerText.gameObject.SetActive(true); 
    }

    public void StopSessionTimer()
    {
        timerIsRunning = false;
        Debug.Log($"Session Timer Stopped. Elapsed Time: {GetFormattedElapsedTime()}");
        if (inGameTimerText != null) inGameTimerText.gameObject.SetActive(false); 
    }

    public string GetFormattedElapsedTime()
    {
        return FormatTime(sessionElapsedTime);
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdatePlayerHealthUI(float currentHealth, float maxHealth)
    {
        if (playerHealthText != null)
        {
            playerHealthText.text = $"HP: {Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }

        if (playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = maxHealth;
            playerHealthSlider.value = currentHealth;
        }
    }

    public void UpdateSpellCooldownUI(float currentCooldown, float maxCooldown, int spellIndex = 0)
    {
        if (spellCooldownImage != null)
        {
            if (maxCooldown <= 0.001f) 
            {
                spellCooldownImage.fillAmount = 1f;
            }
            else
            {
                spellCooldownImage.fillAmount = 1.0f - (Mathf.Clamp(currentCooldown, 0, maxCooldown) / maxCooldown);
            }
        }

        if (spellCooldownText != null)
        {
            if (currentCooldown > 0.01f) 
            {
                spellCooldownText.text = Mathf.CeilToInt(currentCooldown).ToString();
                spellCooldownText.enabled = true;
            }
            else
            {
                spellCooldownText.text = ""; 
                spellCooldownText.enabled = false; 
            }
        }
    }
    
    public void ShowLevelUpPanel(List<SpellUpgradeChoice> choices)
    {
        if (levelUpPanel == null || spellChoiceSlots == null)
        {
            Debug.LogError("UIManager: LevelUpPanel or SpellChoiceSlots are not assigned in the Inspector!");
            return;
        }
        if (timerIsRunning) Time.timeScale = 0f; 
        levelUpPanel.SetActive(true);
        
        for (int i = 0; i < spellChoiceSlots.Length; i++)
        {
            if (i < choices.Count && spellChoiceSlots[i].slotRoot != null)
            {
                spellChoiceSlots[i].slotRoot.SetActive(true);
                if(spellChoiceSlots[i].nameText != null) spellChoiceSlots[i].nameText.text = choices[i].spellName;
                if(spellChoiceSlots[i].descriptionText != null) spellChoiceSlots[i].descriptionText.text = choices[i].description;
                
                if (spellChoiceSlots[i].iconImage != null)
                {
                    if (choices[i].icon != null)
                    {
                        spellChoiceSlots[i].iconImage.sprite = choices[i].icon;
                        spellChoiceSlots[i].iconImage.enabled = true;
                    }
                    else
                    {
                        spellChoiceSlots[i].iconImage.enabled = false;
                    }
                }

                if(spellChoiceSlots[i].selectButton != null)
                {
                    int capturedChoiceIndex = choices[i].choiceIndex; 
                    spellChoiceSlots[i].selectButton.onClick.RemoveAllListeners();
                    spellChoiceSlots[i].selectButton.onClick.AddListener(() => OnSpellUpgradeSelected(capturedChoiceIndex));
                }
            }
            else if (spellChoiceSlots[i].slotRoot != null)
            {
                spellChoiceSlots[i].slotRoot.SetActive(false); 
            }
        }
    }

    public void HideLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        if (timerIsRunning) Time.timeScale = 1f; 
    }

    private void OnSpellUpgradeSelected(int choiceIndex)
    {
        Debug.Log($"Player selected spell/upgrade choice with original index: {choiceIndex}");
        HideLevelUpPanel();
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuPanel == null)
        {
            Debug.LogError("UIManager: PauseMenuPanel is not assigned in the Inspector!");
            return;
        }

        isPaused = !isPaused;
        pauseMenuPanel.SetActive(isPaused);

        if (isPaused)
        {
            if (timerIsRunning) Time.timeScale = 0f;
            Debug.Log("Game Paused");
        }
        else
        {
            if (timerIsRunning) Time.timeScale = 1f;
            Debug.Log("Game Resumed");
        }
    }

    public void ResumeGame() 
    {
        if (isPaused)
        {
            TogglePauseMenu(); 
        }
    }

    public void ReturnToMainMenu() 
    {
        Time.timeScale = 1f; 
        Debug.Log($"Returning to Main Menu: {mainMenuSceneName}");
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("MainMenuSceneName is not set in UIManager!");
        }
    }

    private void HandleAllWavesCleared() 
    {
        ShowVictoryPanel();
    }

    public void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            if (gameOverPanel != null && gameOverPanel.activeSelf)
            {
                Debug.Log("Game Over panel is active, not showing Victory panel.");
                return;
            }
            Debug.Log("All waves cleared! Showing Victory Panel.");
            StopSessionTimer(); 
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; 
            
            if (victoryPanelTimeText != null)
            {
                victoryPanelTimeText.text = "Süre: " + GetFormattedElapsedTime();
            }
            
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("UIManager: VictoryPanel is not assigned in the Inspector!");
        }
    }

    private void HandlePlayerDied() 
    {
        ShowGameOverPanel();
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            if (victoryPanel != null && victoryPanel.activeSelf)
            {
                Debug.Log("Victory panel is active, not showing Game Over panel.");
                return;
            }
            Debug.Log("Player died! Showing Game Over Panel.");
            StopSessionTimer(); 
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; 
            
            if (gameOverPanelTimeText != null)
            {
                gameOverPanelTimeText.text = "Hayatta Kalma Süresi: " + GetFormattedElapsedTime();
            }
            
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("UIManager: GameOverPanel is not assigned in the Inspector!");
        }
    }

    public void RestartGame() // Artık bu da ana menüye dönecek
    {
        Time.timeScale = 1f;
        Debug.Log("Restarting game by returning to Main Menu...");
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("MainMenuSceneName is not set in UIManager! Cannot restart via main menu.");
            // Fallback olarak mevcut sahneyi yeniden yükleyebilir veya hata verebilir.
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Eski davranış
        }
    }

    public void ReturnToMainMenuFromGameOver() 
    {
        Time.timeScale = 1f; 
        Debug.Log($"Game Over - Returning to Main Menu: {mainMenuSceneName}");
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("MainMenuSceneName is not set in UIManager!");
        }
    }

    public void QuitGameFromVictory() 
    {
        Time.timeScale = 1f; 
        Debug.Log("Victory - Quitting game...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    void OnDestroy()
    {
        if (Instance == this) 
        {
            // Statik event abonelikleri OnDisable'da kaldırılıyor.
        }
    }
}