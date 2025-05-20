using UnityEngine;
using FSMC.Runtime;
using System;

[Serializable]
// [CreateAssetMenu(fileName = "EnemyFollowStateBehaviour", menuName = "FSMC/Enemy Behaviours/Follow")]
public class EnemyFollow : FSMC_Behaviour
{
    // [Header("Movement")] // Bu başlık ve altındaki moveSpeed kaldırılıyor.
    // public float moveSpeed = 3f; // Bu alan artık Enemy.cs'den okunacak.
    // stoppingDistance ve loseTargetDistance artık Enemy.cs'den okunacak.

    [Header("Animation")]
    [Tooltip("Name of the Boolean parameter in the Animator Controller that triggers movement animation (e.g., IsMoving).")]
    public string animatorMoveBoolParameterName = "IsMoving";
    // public string moveAnimationName = "Move"; // Bu satır artık kullanılmayacak, SetBool kullanılacak.
    private Animator animator;

    [Tooltip("Boolean FSM parameter indicating if the target is now within attack range.")]
    public string targetInAttackRangeParameter = "IsTargetInAttackRange";
    [Tooltip("Boolean FSM parameter indicating if a valid target is still being followed.")]
    public string targetFoundFSMParameter = "TargetFound"; // EnemyIdle'ın set ettiği parametreyle aynı isimde olmalı

    private Transform currentTargetTransform;
    private FSMC_Executer fsmcExecuterComponent;
    private Rigidbody2D rb;
    private Enemy enemyScript; // Ana düşman script'ine referans

    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.StateInit(stateMachine, executer);
        fsmcExecuterComponent = executer;
        animator = executer.GetComponent<Animator>();
        rb = executer.GetComponent<Rigidbody2D>();
        enemyScript = executer.GetComponent<Enemy>(); // Enemy.cs olduğunu varsayıyoruz

        if (animator == null) 
            Debug.LogWarning($"EnemyFollow ({GetType().Name}): Animator not found on '{executer.gameObject.name}'.");
        if (enemyScript == null)
            Debug.LogError($"EnemyFollow ({GetType().Name}): Enemy script not found on '{executer.gameObject.name}'. This behaviour requires it to get the target.");
    }

    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        // Debug.Log("FollowEntered");
        base.OnStateEnter(stateMachine, executer);
        if (animator != null && !string.IsNullOrEmpty(animatorMoveBoolParameterName))
        {
            animator.SetBool(animatorMoveBoolParameterName, true);
        }
        // else if (animator != null && !string.IsNullOrEmpty(moveAnimationName)) // Eski direkt oynatma
        // {
        //    animator.Play(moveAnimationName);
        // }

        if (enemyScript != null)
        {
            currentTargetTransform = enemyScript.detectedTarget;
        }

        if (currentTargetTransform == null)
        {
            // Hedef yoksa, hemen TargetFound'u false yap ve Idle'a dönülmesini sağla
            if (!string.IsNullOrEmpty(targetFoundFSMParameter))
                stateMachine.SetBool(targetFoundFSMParameter, false);
            Debug.LogWarning($"EnemyFollow ({GetType().Name}): No target found on Enemy script. Forcing TargetFound to false.");
        }
        
        // Saldırı menzili parametresini başlangıçta false yap
        if (!string.IsNullOrEmpty(targetInAttackRangeParameter))
            stateMachine.SetBool(targetInAttackRangeParameter, false);
    }

    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {

        base.OnStateUpdate(stateMachine, executer);

        if (enemyScript == null)
        {
            Debug.LogError($"[{Time.frameCount}] EnemyFollow: enemyScript is null in Update. This should not happen if StateInit succeeded.");
            if (!string.IsNullOrEmpty(targetFoundFSMParameter))
                stateMachine.SetBool(targetFoundFSMParameter, false); // Güvenlik önlemi
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }
        
        Transform previousTarget = currentTargetTransform;
        currentTargetTransform = enemyScript.detectedTarget; // Hedefi her frame yeniden oku

        if (previousTarget != currentTargetTransform)
        {
             Debug.Log($"[{Time.frameCount}] EnemyFollow: currentTargetTransform changed. Old: '{(previousTarget == null ? "NULL" : previousTarget.name)}', New: '{(currentTargetTransform == null ? "NULL" : currentTargetTransform.name)}'");
        }

        if (currentTargetTransform == null)
        {
            // Debug.Log($"[{Time.frameCount}] EnemyFollow: currentTargetTransform IS NULL. Setting '{targetFoundFSMParameter}' to false.");
            if (!string.IsNullOrEmpty(targetFoundFSMParameter))
            {
                // Sadece gerçekten değişiyorsa set et ve logla
                if (stateMachine.GetBool(targetFoundFSMParameter) == true)
                {
                    // Debug.Log($"[{Time.frameCount}] EnemyFollow: Changing FSM param '{targetFoundFSMParameter}' from true to false.");
                    stateMachine.SetBool(targetFoundFSMParameter, false);
                }
            }
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }
        // else
        // {
            // Eğer buradaysak ve currentTargetTransform null değilse, TargetFound'un true olması beklenir (Idle tarafından set edilmiş olmalı).        // }


        // }

        if (enemyScript == null)
        {
            // Debug.LogError($"[{Time.frameCount}] EnemyFollow: enemyScript is null, cannot get range values for distance checks.");
            return;
        }

        // Menzilleri Enemy script'inden oku
        float currentActionRange = enemyScript.actionRange; // Bu, stoppingDistance yerine geçecek
        float currentLoseTargetDistance = enemyScript.loseTargetDistance;

        float distanceToTarget = Vector2.Distance(fsmcExecuterComponent.transform.position, currentTargetTransform.position);
        // Debug.Log($"[{Time.frameCount}] EnemyFollow: Distance to '{currentTargetTransform.name}' is {distanceToTarget}. ActionRange (Stopping): {currentActionRange}, LoseTargetDistance: {currentLoseTargetDistance}");

        // Hedefi kaybetme kontrolü (mesafeye göre)
        if (distanceToTarget > currentLoseTargetDistance)
        {
            // Debug.Log($"[{Time.frameCount}] EnemyFollow: Target '{currentTargetTransform.name}' is beyond loseTargetDistance ({distanceToTarget} > {currentLoseTargetDistance}). Setting '{targetFoundFSMParameter}' to false.");
            if (!string.IsNullOrEmpty(targetFoundFSMParameter))
            {
                stateMachine.SetBool(targetFoundFSMParameter, false);
            }
            enemyScript.detectedTarget = null; // Enemy script'indeki hedefi de temizle
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return; // Idle'a dönmeli
        }

        // Saldırı menzili kontrolü
        if (distanceToTarget <= currentActionRange) // currentActionRange kullanıldı
        {
            // Debug.Log($"[{Time.frameCount}] EnemyFollow: Target '{currentTargetTransform.name}' IN ACTION RANGE. Setting '{targetInAttackRangeParameter}' to true.");
            if (!string.IsNullOrEmpty(targetInAttackRangeParameter))
                stateMachine.SetBool(targetInAttackRangeParameter, true);
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }
        else
        {
            // Debug.Log($"[{Time.frameCount}] EnemyFollow: Target '{currentTargetTransform.name}' NOT in action range. Setting '{targetInAttackRangeParameter}' to false.");
            if (!string.IsNullOrEmpty(targetInAttackRangeParameter))
                stateMachine.SetBool(targetInAttackRangeParameter, false);
        }

        // Hedefe doğru hareket
        Vector2 direction = (currentTargetTransform.position - fsmcExecuterComponent.transform.position).normalized;
        
        if (rb != null)
        {
            if (enemyScript != null) // enemyScript null değilse onun hızını kullan
            {
                rb.linearVelocity = direction * enemyScript.moveSpeed;
            }
            // else // enemyScript null ise bir varsayılan hız kullanılabilir veya hata verilebilir. Şimdilik bir şey yapma.
            // {
            //     rb.linearVelocity = direction * 3f; // Fallback speed
            // }
        }
        else
        {
            if (enemyScript != null)
            {
                fsmcExecuterComponent.transform.position = Vector2.MoveTowards(fsmcExecuterComponent.transform.position, currentTargetTransform.position, enemyScript.moveSpeed * Time.deltaTime);
            }
            // else
            // {
            //     fsmcExecuterComponent.transform.position = Vector2.MoveTowards(fsmcExecuterComponent.transform.position, currentTargetTransform.position, 3f * Time.deltaTime); // Fallback speed
            // }
        }

        // Yönü Enemy.cs'deki FaceTarget metodu ile ayarla
        if (enemyScript != null && currentTargetTransform != null)
        {
            enemyScript.FaceTarget(currentTargetTransform);
        }
    }

    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        // Debug.Log("FollowExited");
        base.OnStateExit(stateMachine, executer);
        if (animator != null && !string.IsNullOrEmpty(animatorMoveBoolParameterName))
        {
            animator.SetBool(animatorMoveBoolParameterName, false);
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
