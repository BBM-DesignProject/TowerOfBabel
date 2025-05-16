using TMPro;
using UnityEngine;
using UnityEngine.UI; // Unity UI elemanları için (Text, Slider, Image)

public class UIManager : MonoBehaviour
{
    [Header("Player Health UI")]
    public Slider playerHealthSlider; // Oyuncu can barı için Slider
    public TextMeshProUGUI playerHealthText; // Opsiyonel: Canı metin olarak göstermek için (örn: 100/100)

    [Header("Tutorial UI")]
    public TextMeshProUGUI tutorialTextElement; // Tutorial metnini gösterecek UI Text elemanı
    public string spellTutorialMessage = "Sağ Fare Tuşu basılıyken Sol Fare Tuşu ile daire çizerek büyü yap!";

    [Header("Spell Cooldown UI")]
    public Image spellCooldownImage; // Büyü bekleme süresi için (fillAmount ile)
    public TextMeshProUGUI spellCooldownText; // Opsiyonel: Kalan süreyi göstermek için

    private PlayerHealth playerHealth;

    void Start()
    {
        // Oyuncuyu bul ve can event'ine abone ol
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onHealthChanged.AddListener(UpdatePlayerHealthUI);
                // Başlangıç değerlerini ayarla
                UpdatePlayerHealthUI(playerHealth.maxHealth, playerHealth.maxHealth); // Mevcut canı almak için bir yol gerekebilir veya PlayerHealth Start'ta invoke etmeli
            }
            else
            {
                Debug.LogError("UIManager: Player object does not have a PlayerHealth component.");
            }
        }
        else
        {
            Debug.LogError("UIManager: Could not find GameObject with tag 'Player'.");
        }

        // Tutorial metnini ayarla
        if (tutorialTextElement != null)
        {
            tutorialTextElement.text = spellTutorialMessage;
        }
        else
        {
            Debug.LogWarning("UIManager: TutorialTextElement is not assigned.");
        }

        // Cooldown UI başlangıç ayarları
        if (spellCooldownImage != null)
        {
            spellCooldownImage.fillAmount = 0; // Başlangıçta cooldown yok
        }
        if (spellCooldownText != null)
        {
            spellCooldownText.text = "";
        }
    }

    public void UpdatePlayerHealthUI(float currentHealth, float maxHealth)
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = maxHealth;
            playerHealthSlider.value = currentHealth;
        }
        if (playerHealthText != null)
        {
            playerHealthText.text = $"Can: {Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    public void UpdateSpellCooldownUI(float currentCooldown, float maxCooldown)
    {
        if (spellCooldownImage != null)
        {
            if (maxCooldown <= 0)
            {
                spellCooldownImage.fillAmount = 0;
            }
            else
            {
                spellCooldownImage.fillAmount = currentCooldown / maxCooldown;
            }
        }
        if (spellCooldownText != null)
        {
            if (currentCooldown > 0)
            {
                spellCooldownText.text = Mathf.CeilToInt(currentCooldown).ToString();
            }
            else
            {
                spellCooldownText.text = "";
            }
        }
    }

    // Tutorial metnini gizlemek/göstermek için bir metod (opsiyonel)
    public void SetTutorialTextActive(bool isActive)
    {
        if (tutorialTextElement != null)
        {
            tutorialTextElement.gameObject.SetActive(isActive);
        }
    }
}