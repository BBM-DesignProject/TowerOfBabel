using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 30f;
    [HideInInspector] public float currentHealth;

    [Header("Combat Stats & Ranges")]
    public int damageOnTouch = 10;
    [Tooltip("Maximum distance at which this enemy can initially detect a target.")]
    public float detectionRadius = 10f;
    [Tooltip("The range within which this enemy will attempt to perform its action (e.g., attack).")]
    public float actionRange = 2f; // Bu, hem Idle'daki attackRange hem de Follow'daki stoppingDistance yerine geçecek
    [Tooltip("If a target moves beyond this distance while being followed, the enemy will lose the target.")]
    public float loseTargetDistance = 12f;

    [Header("FSM Interaction")]
    [Tooltip("This will be set by FSM Behaviours (e.g., EnemyIdle) when a target is detected.")]
    public Transform detectedTarget;

    // FSMC_Executer referansı (opsiyonel)
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

    public void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector2 directionToTarget = (target.position - transform.position);
        Vector3 localScale = transform.localScale;

        if (directionToTarget.x > 0.01f) // Hedef sağda
        {
            // Varsayılan olarak sprite'ın sağa baktığını ve localScale.x = 1 olduğunu varsayıyoruz.
            // Eğer sprite'ınız varsayılan olarak sola bakıyorsa, bu koşulları ve çarpmaları tersine çevirin.
            if (localScale.x < 0)
            {
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
        else if (directionToTarget.x < -0.01f) // Hedef solda
        {
            if (localScale.x > 0)
            {
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
        // Eğer directionToTarget.x çok küçükse (tam üstünde/altında), mevcut yönde kalır.
    }
}