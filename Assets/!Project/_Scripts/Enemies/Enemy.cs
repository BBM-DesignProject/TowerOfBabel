using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 30f;
    [HideInInspector] public float currentHealth; // FSM veya UI tarafından okunabilir ama Inspector'da direkt değiştirilmesin

    [Header("Combat")]
    public int damageOnTouch = 10; // Temas hasarı
    
    [Header("FSM Interaction")]
    [Tooltip("This will be set by FSM Behaviours (e.g., EnemyIdle) when a target is detected.")]
    public Transform detectedTarget; // FSM Behaviours (EnemyIdle) bu alanı set edecek

    // FSMC_Executer referansı (opsiyonel, eğer FSM parametrelerini buradan güncellemek isterseniz)
    // private FSMC.Runtime.FSMC_Executer fsmcExecuter;

    void Awake()
    {
        currentHealth = maxHealth;
        // fsmcExecuter = GetComponent<FSMC.Runtime.FSMC_Executer>();
    }

    // Start ve Update metodları artık FSM tarafından yönetileceği için kaldırıldı.
    // Hareket ve hedefleme mantığı FSM Behaviour'larında (EnemyIdle, EnemyFollow, EnemyAction).

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died.");
        // Burada ölüm efekti (Assets/Sprites/Death.png kullanılabilir), skor artışı vb. eklenebilir.
        Destroy(gameObject); 
    }

    // Oyuncuya temas ettiğinde hasar verme (Bu mantık kalabilir)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageOnTouch <= 0) return; // Temas hasarı yoksa bir şey yapma

        if (collision.gameObject.CompareTag("Player"))
        {
            // PlayerHealth script'inin oyuncuda olduğunu varsayıyoruz.
            // Projenizdeki oyuncu can script'inin adıyla değiştirin.
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnTouch);
                Debug.Log(gameObject.name + " damaged " + collision.gameObject.name + " on touch for " + damageOnTouch + " damage.");
            }
        }
    }
    
    // Büyü mermisiyle hasar alma gibi şeyler için OnTriggerEnter2D kalabilir,
    // ancak merminin kendisi Enemy'deki TakeDamage'ı çağırabilir.
    // Bu yüzden bu kısım şimdilik yoruma alınabilir veya mermi tasarımına göre düzenlenebilir.
    /*
    void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("PlayerSpell")) // Büyü mermilerinin etiketi "PlayerSpell" ise
    //     {
    //         // SpellDamage spellDamage = other.GetComponent<SpellDamage>();
    //         // if (spellDamage != null)
    //         // {
    //         //     TakeDamage(spellDamage.damageAmount);
    //         // }
    //         // Destroy(other.gameObject); // Büyü mermisini yok et
    //     }
    // }
    */
}