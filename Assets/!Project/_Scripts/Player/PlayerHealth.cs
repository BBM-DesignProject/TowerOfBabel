using FSMC.Runtime;
using UnityEngine;
using UnityEngine.Events; // UnityEvent için

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public float CurrentHealth => currentHealth; // Dışarıdan okunabilir property

    // Oyuncu canı değiştiğinde tetiklenecek event (UI güncellemesi için)
    // Parametreler: currentHealth, maxHealth
    [System.Serializable]
    public class HealthChangedEvent : UnityEvent<float, float> { } 
    public HealthChangedEvent onHealthChanged;

    // Oyuncu öldüğünde tetiklenecek statik event
    public static event System.Action onPlayerDied;

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
        // else // Bu log çok sık çıkabilir, UI Manager'ın varlığına bağlı
        // {
        //     Debug.LogWarning("PlayerHealth: onHealthChanged event has no listeners. UI might not update initially.");
        // }
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

    // Birleştirilmiş Die metodu
    void Die()
    {
        Debug.Log("Player Died! (" + gameObject.name + ")");
        
        // Statik event'i tetikle
        onPlayerDied?.Invoke();

        // Oyuncu objesini deaktif et veya yok et (oyun tasarımına göre)
        // Örneğin, oyuncuyu deaktif edip Game Over ekranının kontrolü ele almasını sağlayabiliriz.
        gameObject.SetActive(false);
        
        // Veya hemen yok et:
        // Destroy(gameObject);

        // Diğer ölümle ilgili işlemler (animasyon, ses, skor vb.) buraya eklenebilir.
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0) return;
        if (currentHealth >= maxHealth) return; // Canı tam veya daha fazlaysa iyileşme

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Debug.Log(gameObject.name + " healed " + amount + ". Current health: " + currentHealth);
        
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // İkinci Die() metodu kaldırıldı.

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