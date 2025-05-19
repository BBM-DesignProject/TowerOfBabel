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

    [Header("Feedback Effects")]
    [Tooltip("Color to flash to when taking damage.")]
    public Color flashColor = Color.white;
    [Tooltip("Duration of the flash effect in seconds.")]
    public float flashDuration = 0.1f;
    [Tooltip("Animator trigger name for the 'take damage' animation.")]
    public string takeDamageAnimatorTrigger = "GotHit"; // Animator'de bu isimde bir trigger olmalı

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private Animator animator;

    // FSMC_Executer referansı (opsiyonel)
    // private FSMC.Runtime.FSMC_Executer fsmcExecuter;

    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            // Debug.Log($"Enemy {gameObject.name}: Awake - Original Sprite Color set to {originalColor}"); // Bu log kalabilir veya kaldırılabilir.
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name}: Awake - SpriteRenderer component not found. Flash effect will not work.");
        }
        // fsmcExecuter = GetComponent<FSMC.Runtime.FSMC_Executer>();
    }

    // Start ve Update metodları artık FSM tarafından yönetileceği için kaldırıldı.
    // Hareket ve hedefleme mantığı FSM Behaviour'larında (EnemyIdle, EnemyFollow, EnemyAction).

    public void TakeDamage(float amount)
    {
        // Debug.Log($"Enemy {gameObject.name}: TakeDamage called with amount {amount}. Current Health before: {currentHealth}");
        currentHealth -= amount;
        // Debug.Log($"Enemy {gameObject.name}: Current Health after: {currentHealth}");

        // Flash efekti
        if (spriteRenderer != null && gameObject.activeInHierarchy) // gameObject.activeInHierarchy kontrolü önemli
        {
            // Debug.Log($"Enemy {gameObject.name}: TakeDamage - Attempting to start FlashEffectCoroutine."); // Bu log kalabilir.
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                // Debug.Log($"Enemy {gameObject.name}: TakeDamage - Stopped existing flash coroutine.");
            }
            flashCoroutine = StartCoroutine(FlashEffectCoroutine());
        }
        else
        {
            if(spriteRenderer == null) Debug.LogWarning($"Enemy {gameObject.name}: TakeDamage - SpriteRenderer is null. Cannot flash.");
            if(!gameObject.activeInHierarchy) Debug.LogWarning($"Enemy {gameObject.name}: TakeDamage - GameObject not active in hierarchy. Cannot flash.");
        }

        // Hasar alma animasyonunu tetikle
        if (animator != null && !string.IsNullOrEmpty(takeDamageAnimatorTrigger))
        {
            // // Debug.Log($"Enemy {gameObject.name}: TakeDamage - Setting animator trigger '{takeDamageAnimatorTrigger}'.");
            animator.SetTrigger(takeDamageAnimatorTrigger);
        }
        // else if (animator == null) // Bu loglar birleştirilebilir veya daha az detaylı olabilir.
        // {
        //    // Debug.LogWarning($"Enemy {gameObject.name}: TakeDamage - Animator is null. Cannot play take damage animation.");
        // }
        // else if (string.IsNullOrEmpty(takeDamageAnimatorTrigger))
        // {
        //    // Debug.LogWarning($"Enemy {gameObject.name}: TakeDamage - takeDamageAnimatorTrigger string is null or empty.");
        // }


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

    private System.Collections.IEnumerator FlashEffectCoroutine()
    {
        if (spriteRenderer == null)
        {
            // Debug.LogError($"Enemy {gameObject.name}: FlashEffectCoroutine - SpriteRenderer is null at start. Aborting flash.");
            yield break;
        }

        // Debug.Log($"Enemy {gameObject.name}: FlashEffectCoroutine - Started. Setting color to: {flashColor} for {flashDuration}s.");
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        
        if (spriteRenderer != null) // Obje hala var mı diye kontrol et
        {
            spriteRenderer.color = originalColor;
            // Debug.Log($"Enemy {gameObject.name}: FlashEffectCoroutine - Finished. Color reset to: {originalColor}.");
        }
        else
        {
            // Debug.LogWarning($"Enemy {gameObject.name}: FlashEffectCoroutine - SpriteRenderer became null during wait. Cannot reset color.");
        }
        flashCoroutine = null; // Coroutine bittiğinde referansı temizle
    }
}