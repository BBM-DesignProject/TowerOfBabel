using UnityEngine;

public class EnemyMeleeAttackHandler : EnemyActionHandlerBase
{
    [Header("Melee Specific Settings")]
    [Tooltip("Damage dealt by this melee attack.")]
    public float meleeDamage = 10f;
    [Tooltip("Offset from this GameObject's pivot to the center of the melee attack area, relative to this transform's forward (up).")]
    public Vector2 attackOffset = Vector2.up;
    [Tooltip("Size of the OverlapBox for melee hit detection.")]
    public Vector2 attackBoxSize = new Vector2(1f, 1f);
    [Tooltip("Layer mask to specify which layers contain player(s) to hit.")]
    public LayerMask playerLayerMask;

    // ActionAnimationName ve ActionDuration'ı override edebiliriz veya base'dekini kullanabiliriz.
    // Örneğin, her melee saldırısının animasyon adı farklı olabilir:
    // public override string ActionAnimationName => "MeleeSwing";
    // public override float ActionDuration => 0.8f;

    // currentTarget artık ExecuteAttack metoduna parametre olarak gelecek.
    // public Transform currentTarget;

    // Awake metodu base sınıfta zaten animator ve enemyScript'i alıyor.
    // Ekstra bir şey gerekmiyorsa override etmeye gerek yok.
    // protected override void Awake()
    // {
    //     base.Awake();
    // }
    
    public override void ExecuteAction(Transform target)
    {
        Debug.Log($"[{Time.frameCount}] {gameObject.name} - {GetType().Name}: Executing melee attack.");
        if (target == null)
        {
            // Debug.LogWarning($"{gameObject.name} - {GetType().Name}: Target is null. Cannot perform melee attack.");
            return;
        }

        // Hedefe doğru dönme mantığı FSM Behaviour (EnemyAction) veya burada yapılabilir.
        // Şimdilik FSM Behaviour'ın (EnemyAction.OnStateEnter) hedefe döndürdüğünü varsayalım.
        // Eğer bu Handler'ın kendisi de dönmeli ise:
        // Vector2 directionToTarget = (target.position - transform.position).normalized;
        // if (directionToTarget != Vector2.zero)
        // {
        //     float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        //     transform.rotation = Quaternion.Euler(0, 0, angle); // Ya da enemyScript.transform.rotation
        // }

        // Debug.Log($"{gameObject.name} - {GetType().Name}: Executing Melee Attack towards {target.name}");

        // transform.up, bu GameObject'in (veya parent Enemy objesinin) baktığı yönü temsil eder.
        Vector2 attackDirection = transform.up;
        Vector2 localAttackCenter = new Vector2(attackOffset.x, attackOffset.y);

        Vector2 worldAttackCenter = (Vector2)transform.position + (Vector2)(transform.rotation * localAttackCenter);
        
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(worldAttackCenter, attackBoxSize, transform.eulerAngles.z, playerLayerMask);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(meleeDamage);
                // Debug.Log($"{gameObject.name} meleed {playerCollider.name} for {meleeDamage} damage via {GetType().Name}.");
            }
        }
    }

    // OnActionEnter, OnActionUpdate, OnActionExit metodları gerekirse override edilebilir.
    // Örneğin, OnActionEnter'da spesifik bir melee animasyonunu tetikleyebiliriz eğer base.ActionAnimationName yeterli değilse.
    // public override void OnActionEnter(Transform target)
    // {
    //     base.OnActionEnter(target); // Base'in animasyon oynatma gibi işlevlerini çağırır
    //     // Melee'ye özel ek OnEnter mantığı...
    // }

    // Editörde melee attack alanını çizmek için (opsiyonel)
    void OnDrawGizmosSelected()
    {
        if (!enabled) return; // Script devre dışıysa çizme

        Gizmos.color = Color.red;
        Vector2 localAttackCenter = new Vector2(attackOffset.x, attackOffset.y);
        Vector2 worldAttackCenter = (Vector2)transform.position + (Vector2)(transform.rotation * localAttackCenter);
        
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(worldAttackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize); // Lokal pozisyonda çiz
        Gizmos.matrix = oldMatrix;
    }
}