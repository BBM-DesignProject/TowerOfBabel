using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 30f;
    [HideInInspector] public float currentHealth;
    [Tooltip("Movement speed of the enemy.")]
    public float moveSpeed = 3f; // Hareket hızı buraya eklendi

    [Header("Combat Stats & Ranges")]
    public int damageOnTouch = 10;
    [Tooltip("Maximum distance at which this enemy can initially detect a target.")]
    public float detectionRadius = 10f;
    [Tooltip("The range within which this enemy will attempt to perform its action (e.g., attack).")]
    public float actionRange = 2f;
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
    public string takeDamageAnimatorTrigger = "GotHit";

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private Animator animator;
    private HealthBar healthBar;
    private EnemySpawner spawnerReference; // EnemySpawner'a referans

    // FSMC_Executer referansı (opsiyonel)
    // private FSMC.Runtime.FSMC_Executer fsmcExecuter;

    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        // Can barını alt objelerden bulmaya çalış
        healthBar = GetComponentInChildren<HealthBar>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name}: Awake - SpriteRenderer component not found. Flash effect will not work.");
        }

        if (healthBar != null)
        {
            healthBar.SetMaxHealth((int)maxHealth); // Can barını başlangıçta ayarla
        }
        else
        {
            // Bu bir hata değil, her düşmanın can barı olmak zorunda değil.
        }

        // EnemySpawner'ı sahnede bul. Genellikle sahnede tek bir Spawner olur.
        spawnerReference = FindObjectOfType<EnemySpawner>();
        // if (spawnerReference == null)
        // {
        //     Debug.LogWarning($"Enemy {gameObject.name}: Could not find EnemySpawner in the scene.");
        // }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return; // Zaten ölmüşse tekrar hasar almasın

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth); // Canın 0'ın altına düşmesini engelle

        // Debug.Log($"Enemy {gameObject.name}: Took {amount} damage. Current Health: {currentHealth}");

        // Can barını güncelle
        if (healthBar != null)
        {
            healthBar.SetHealth((int)currentHealth);
        }

        // Flash efekti
        if (spriteRenderer != null && gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashEffectCoroutine());
        }
        
        // Hasar alma animasyonunu tetikle
        if (animator != null && !string.IsNullOrEmpty(takeDamageAnimatorTrigger))
        {
            animator.SetTrigger(takeDamageAnimatorTrigger);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died.");
        
        // Spawner'a ölümünü bildir
        if (spawnerReference != null)
        {
            spawnerReference.ReportEnemyDeath(gameObject);
        }

        // Ölüm efektleri, skor vb.
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageOnTouch <= 0) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnTouch);
                // Debug.Log(gameObject.name + " damaged " + collision.gameObject.name + " on touch for " + damageOnTouch + " damage.");
            }
        }
    }
    
    public void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector2 directionToTarget = (target.position - transform.position);
        Vector3 localScale = transform.localScale;

        if (directionToTarget.x > 0.01f) 
        {
            if (localScale.x < 0) 
            {
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
        else if (directionToTarget.x < -0.01f) 
        {
            if (localScale.x > 0) 
            {
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
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
        
        if (spriteRenderer != null) 
        {
            spriteRenderer.color = originalColor;
            // Debug.Log($"Enemy {gameObject.name}: FlashEffectCoroutine - Finished. Color reset to: {originalColor}.");
        }
        // else
        // {
            // Debug.LogWarning($"Enemy {gameObject.name}: FlashEffectCoroutine - SpriteRenderer became null during wait. Cannot reset color.");
        // }
        flashCoroutine = null; 
    }
}