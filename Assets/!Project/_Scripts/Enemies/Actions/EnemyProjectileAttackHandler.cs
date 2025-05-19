using UnityEngine;

public class EnemyProjectileAttackHandler : EnemyActionHandlerBase
{
    [Header("Projectile Attack Settings")]
    [Tooltip("The projectile prefab to be spawned.")]
    public GameObject projectilePrefab;

    [Tooltip("Optional: Transform point from which projectiles are spawned. If null, uses the enemy's position plus an offset.")]
    public Transform projectileSpawnPoint;

    [Tooltip("If projectileSpawnPoint is null, this offset is applied in the forward direction of the enemy to spawn the projectile.")]
    public float projectileSpawnOffsetForward = 1f;

    [Tooltip("Speed of the projectile. This can be used if the projectile itself doesn't define its speed, or to override it via an Initialize method on the projectile.")]
    public float projectileSpeed = 10f; // Örnek değer

    [Tooltip("Damage dealt by the projectile. This can be used if the projectile itself doesn't define its damage, or to override it.")]
    public float projectileDamage = 5f; // Örnek değer

    // Animasyon ve süre için base class'taki değerleri override edebiliriz
    // public override string ActionAnimationName => "RangedAttack";
    // public override float ActionDuration => 1.2f;

    public override void ExecuteAction(Transform target)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"{gameObject.name} - {GetType().Name}: Projectile Prefab is not assigned. Cannot fire.");
            return;
        }

        if (target == null && enemyScript != null && enemyScript.detectedTarget != null)
        {
            // Eğer ExecuteAction'a target null geldiyse ama Enemy script'inde hala bir hedef varsa onu kullan.
            // Bu, aksiyonun etki noktası ile FSM'in hedef güncellemesi arasında bir senkronizasyon farkı olursa diye.
            target = enemyScript.detectedTarget;
        }
        
        // Hedefe doğru tam açılı dönüş kodu kaldırıldı.
        // Yön ayarı (sağa/sola bakma) EnemyAction FSM Behaviour'ının OnStateEnter'ında yapılıyor.
        // Bu Handler, o ayarlanmış yöne göre mermiyi fırlatmalı.
        // if (target != null)
        // {
        //      Vector2 directionToTarget = (target.position - transform.position).normalized;
        //      if (directionToTarget != Vector2.zero)
        //      {
        //          float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        //          if (enemyScript != null)
        //          {
        //             enemyScript.transform.rotation = Quaternion.Euler(0, 0, angle);
        //          }
        //          else
        //          {
        //             transform.rotation = Quaternion.Euler(0, 0, angle);
        //          }
        //      }
        // }

        // Mermi spawn pozisyonu ve rotasyonu
        // Düşmanın (veya spawn noktasının) mevcut yönünü kullanacağız.
        // EnemyAction.OnStateEnter'da düşmanın localScale.x'i ayarlandığı için sprite doğru yöne bakıyor olmalı.
        // Düşmanın transform.rotation'ı (0,0,0) olmalı.
        Transform baseTransform = enemyScript != null ? enemyScript.transform : transform;
        Transform spawnPointToUse = projectileSpawnPoint != null ? projectileSpawnPoint : baseTransform;
        
        Vector3 spawnPosition;
        // Merminin başlangıç rotasyonu, genellikle fırlatılacağı yöne bakar veya mermi sprite'ının varsayılan yönüdür.
        // EnemyProjectile.Initialize içinde merminin kendi sprite'ı ayrıca döndürülecek.
        // Bu yüzden burada spawnRotation'ı spawnPointToUse.rotation veya Quaternion.identity olarak ayarlayabiliriz.
        // Eğer spawnPointToUse bir silahın ucu gibi özel bir rotasyona sahipse, onun rotasyonunu kullanmak mantıklıdır.
        Quaternion spawnRotation = spawnPointToUse.rotation;

        if (projectileSpawnPoint != null)
        {
            spawnPosition = projectileSpawnPoint.position;
        }
        else
        {
            // baseTransform.right, localScale.x ile çevrildiğinde doğru "ön" yönü verir (eğer sprite sağa bakıyorsa).
            // Eğer sprite yukarı bakıyorsa baseTransform.up kullanılabilir.
            // Varsayılan olarak sprite'ın sağa baktığını ve X ekseninde çevrildiğini varsayalım.
            Vector2 forwardDirection = baseTransform.right * baseTransform.localScale.x;
            spawnPosition = baseTransform.position + (Vector3)(forwardDirection.normalized * projectileSpawnOffsetForward);
        }
        // Tekrarlanan spawnPosition bloğu kaldırıldı.

        GameObject projectileGO = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        // Debug.Log($"{gameObject.name} - {GetType().Name}: Fired projectile {projectileGO.name} from {spawnPosition} with rotation {spawnRotation.eulerAngles}");

        EnemyProjectile projectileScript = projectileGO.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            Vector2 fireDirection;
            if (target != null) // Eğer bir hedef varsa (genellikle oyuncu)
            {
               // Mermiyi spawn noktasından hedefe doğru yönlendir
               fireDirection = ((Vector2)target.position - (Vector2)spawnPosition).normalized;
            }
            else
            {
                // Hedef yoksa, düşmanın baktığı yöne doğru fırlat (fallback)
                // Sprite'ınızın varsayılan olarak sağa baktığını ve X ekseninde çevrildiğini varsayıyoruz.
                fireDirection = baseTransform.right * Mathf.Sign(baseTransform.localScale.x);
            }
            
            // Debug.Log($"Fire direction: {fireDirection}, Target: {(target != null ? target.name : "null")}, SpawnPos: {spawnPosition}");
            projectileScript.Initialize(fireDirection, projectileSpeed, projectileDamage); // .normalized zaten yapıldı veya Initialize içinde yapılabilir
            projectileScript.SetOwner(fsmcExecuter);
        }
        else
        {
            // Merminin Rigidbody'si varsa ve hıza ihtiyacı varsa:
            Rigidbody2D rb = projectileGO.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = transform.up * projectileSpeed; // Düşmanın baktığı yönde fırlat
            }
        }
    }

    // OnActionEnter, OnActionUpdate, OnActionExit metodları gerekirse override edilebilir.
    // Örneğin, OnActionEnter'da spesifik bir mermili saldırı animasyonunu tetikleyebiliriz.
    public override void OnActionEnter(Transform target)
    {
        base.OnActionEnter(target); // Base'in animasyon oynatma gibi işlevlerini çağırır (eğer ActionAnimationName set edilmişse)
        // Projectile'a özel ek OnEnter mantığı...
        // Örneğin, silahı hedefe doğrultma animasyonu vs.
    }
}