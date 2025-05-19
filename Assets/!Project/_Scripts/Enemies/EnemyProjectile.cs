using FSMC.Runtime;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f; // Varsayılan hız, Initialize ile override edilebilir
    public float lifetime = 5f; // Saniye cinsinden merminin ömrü

    [Header("Damage")]
    public float damageAmount = 10f; // Varsayılan hasar, Initialize ile override edilebilir
    public string playerTag = "Player"; // Hasar verilecek oyuncunun etiketi

    [Header("Effects")]
    public GameObject hitEffectPrefab; // Çarpışma efekti prefab'ı (opsiyonel)

    private Vector2 direction;
    private Rigidbody2D rb;
    private FSMC_Executer ownerExecuter; // Mermiyi fırlatan düşmanın FSMC_Executer'ı

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning($"EnemyProjectile on {gameObject.name}: Rigidbody2D not found. Adding one.");
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0; // Yerçekimi olmasın
    }

    public void Initialize(Vector2 flyDirection, float initialSpeed, float initialDamage)
    {
        this.direction = flyDirection.normalized;
        this.speed = initialSpeed;
        this.damageAmount = initialDamage;

        Destroy(gameObject, lifetime);

        if (this.direction != Vector2.zero)
        {
            // Sprite'ın sağı (right) varsayılan olarak ileri yönü olduğu için -90f çıkarılmaz.
            float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg; 
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void SetOwner(FSMC_Executer owner)
    {
        this.ownerExecuter = owner;
    }

    void FixedUpdate() 
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            transform.Translate(direction * speed * Time.fixedDeltaTime, Space.World);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (ownerExecuter != null && other.gameObject == ownerExecuter.gameObject)
        {
            return; 
        }

        if (other.CompareTag(playerTag))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>(); 
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
            SpawnHitEffectAndDestroy();
        }
        else if (!other.isTrigger) 
        {
            SpawnHitEffectAndDestroy();
        }
    }

    void SpawnHitEffectAndDestroy()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject); 
    }
}
