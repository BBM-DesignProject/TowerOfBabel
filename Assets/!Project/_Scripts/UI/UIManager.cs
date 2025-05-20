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
    // public GameObject gameOverPanel; // Ölüm durumu için de benzer bir panel eklenebilir.

    public string playerTag = "Player"; 
    public string mainMenuSceneName = "MainMenu"; 

    private bool isPaused = false;

    // Level Up UI için yardımcı class
    [System.Serializable]
    public class SpellChoiceSlotUI
    {
        public GameObject slotRoot; 
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public Button selectButton;
    }

    // Level Up seçenekleri için veri yapısı
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
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        EnemySpawner.OnAllWavesCompletedAndCleared += HandleAllWavesCleared;
    }

    void OnDisable()
    {
        EnemySpawner.OnAllWavesCompletedAndCleared -= HandleAllWavesCleared;
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
    }

    void Update()
    {
        if (Instance != this) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if ((levelUpPanel != null && levelUpPanel.activeSelf) || 
                (victoryPanel != null && victoryPanel.activeSelf))
            {
                return;
            }
            TogglePauseMenu();
        }
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

        levelUpPanel.SetActive(true);
        Time.timeScale = 0f; 

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
        Time.timeScale = 1f; 
    }

    private void OnSpellUpgradeSelected(int choiceIndex)
    {
        Debug.Log($"Player selected spell/upgrade choice with original index: {choiceIndex}");
        // PlayerManager.Instance.ApplyUpgrade(choiceIndex); 
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
            Time.timeScale = 0f; 
            Debug.Log("Game Paused");
        }
        else
        {
            Time.timeScale = 1f; 
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

    public void ReturnToMainMenu() // Bu metod PausePanel'deki butona bağlı
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
            Debug.Log("All waves cleared! Showing Victory Panel.");
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; 
            
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("UIManager: VictoryPanel is not assigned in the Inspector!");
        }
    }

    // ReturnToMainMenuFromVictory metodu kaldırıldı.
    // Sadece QuitGameFromVictory kalacak.

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
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.onHealthChanged.RemoveListener(UpdatePlayerHealthUI);
                }
            }
        }
    }
}