using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f; // Saniye cinsinden merminin ömrü
    public float damageAmount = 15f; // Merminin vereceği hasar
    public string enemyTag = "Enemy"; // Düşmanların etiketi

    private Vector2 direction;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        // Merminin ömrünü ayarla
        Destroy(gameObject, lifetime);

        // Mermiyi doğru yöne döndür (opsiyonel, sprite'a bağlı)
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Sprite'ın üstü ileri bakıyorsa
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void Update()
    {
        // Mermiyi hareket ettir
        transform.Translate(Vector2.up * speed * Time.deltaTime); // Sprite'ın "yukarısı" ileri yöndeyse
        // Alternatif: transform.Translate(direction * speed * Time.deltaTime, Space.World); // Eğer direction'ı dünya koordinatlarında kullanmak isterseniz
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Düşmana çarpıp çarpmadığını kontrol et
        if (other.CompareTag(enemyTag))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log("Spell projectile hit " + other.name + " for " + damageAmount + " damage.");
            }
            Destroy(gameObject); // Çarpışmadan sonra mermiyi yok et
        }
        else if (other.CompareTag("Wall")) // Duvarların etiketi "Wall" ise
        {
            Destroy(gameObject);
        }
    }
}