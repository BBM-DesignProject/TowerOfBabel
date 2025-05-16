using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float maxHealth = 30f;
    public int damageOnTouch = 10;

    private float currentHealth;
    private Transform playerTransform;
    private bool isPlayerFound = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Oyuncuyu etiketiyle bul. Start içinde çağırarak diğer objelerin Awake'inin bitmesini bekleyebiliriz.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            isPlayerFound = true;
        }
        else
        {
            Debug.LogError("Enemy could not find GameObject with tag 'Player'. Make sure your player has the 'Player' tag.");
            // Oyuncu bulunamazsa bu düşmanı devre dışı bırakabilir veya bir uyarı durumu ayarlayabiliriz.
            // enabled = false; 
        }
    }

    void Update()
    {
        if (isPlayerFound && playerTransform != null)
        {
            // Oyuncuya doğru hareket et
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            // Rigidbody2D kullanmak daha iyi çarpışma tespiti ve fizik etkileşimleri sağlar.
            // Eğer Rigidbody2D varsa:
            // Rigidbody2D rb = GetComponent<Rigidbody2D>();
            // if (rb != null)
            // {
            //     rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
            // }
            // else
            // {
                   transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            // }

            // (Opsiyonel) Düşmanın oyuncuya doğru dönmesi (sprite'a göre ayarlanmalı)
            // if (direction != Vector2.zero)
            // {
            //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Sprite'ın üstü ileri bakıyorsa
            //     transform.rotation = Quaternion.Euler(0, 0, angle);
            // }
        }
    }

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

    // Oyuncuya temas ettiğinde hasar verme
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnTouch);
                Debug.Log("Enemy damaged player (" + collision.gameObject.name + ") on touch for " + damageOnTouch + " damage.");
            }
            // İsteğe bağlı: Temas sonrası düşmanın kendini yok etmesi veya geri sekmesi
            // Die(); 
        }
    }
    
    // Eğer trigger collider kullanılıyorsa (örn: büyü mermisi için):
    // void OnTriggerEnter2D(Collider2D other)
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
}