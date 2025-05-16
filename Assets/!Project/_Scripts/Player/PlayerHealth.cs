using UnityEngine;
using UnityEngine.Events; // UnityEvent için

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    // Oyuncu canı değiştiğinde tetiklenecek event (UI güncellemesi için)
    // Parametreler: currentHealth, maxHealth
    [System.Serializable]
    public class HealthChangedEvent : UnityEvent<float, float> { } 
    public HealthChangedEvent onHealthChanged;

    // Oyuncu öldüğünde tetiklenecek event
    public UnityEvent onPlayerDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Başlangıçta UI'ı güncellemek için event'i tetikle
        if (onHealthChanged != null)
        {
            onHealthChanged.Invoke(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning("PlayerHealth: onHealthChanged event is not assigned. UI might not update.");
        }

        if (onPlayerDied == null)
        {
            onPlayerDied = new UnityEvent(); // Null ise initialize et
            Debug.LogWarning("PlayerHealth: onPlayerDied event was not assigned, initialized to new UnityEvent.");
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return; // Zaten ölmüşse hasar alma

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Canın 0'ın altına düşmesini engelle

        Debug.Log(gameObject.name + " took " + amount + " damage. Current health: " + currentHealth);

        if (onHealthChanged != null)
        {
            onHealthChanged.Invoke(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0) return; // Ölmüşse iyileşemez
        if (currentHealth == maxHealth) return; // Zaten tam can ise iyileşemez

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Canın maksimumun üzerine çıkmasını engelle

        Debug.Log(gameObject.name + " healed " + amount + ". Current health: " + currentHealth);
        
        if (onHealthChanged != null)
        {
            onHealthChanged.Invoke(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died.");
        // Burada ölüm animasyonu (Assets/Sprites/Death.png kullanılabilir), oyun sonu ekranı vb. eklenebilir.
        // Şimdilik sadece objeyi devre dışı bırakabilir veya yok edebiliriz.
        // Örneğin, oyuncu kontrolünü devre dışı bırak:
        // GetComponent<PlayerMovement>().enabled = false;
        // GetComponent<SpellGestureSystem>().enabled = false;
        
        if (onPlayerDied != null)
        {
            onPlayerDied.Invoke();
        }
        
        // Destroy(gameObject, 2f); // 2 saniye sonra yok et (opsiyonel)
        // enabled = false; // Script'i devre dışı bırak
    }

    // Test amaçlı hasar alma fonksiyonu (Editörden veya başka bir scriptten çağrılabilir)
    [ContextMenu("Test Take 10 Damage")]
    public void TestDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Test Heal 10 HP")]
    public void TestHeal()
    {
        Heal(10);
    }
}