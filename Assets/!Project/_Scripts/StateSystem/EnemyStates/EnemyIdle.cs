using UnityEngine;
using FSMC.Runtime; // FSMC_Behaviour, FSMC_Controller, FSMC_Executer için
using System;

[Serializable]
// Eğer bu bir ScriptableObject olacaksa ve editörden oluşturulacaksa CreateAssetMenu attribute'u ekleyebilirsiniz.
// [CreateAssetMenu(fileName = "EnemyIdleStateBehaviour", menuName = "FSMC/Enemy Behaviours/Idle")]
public class EnemyIdle : FSMC_Behaviour
{
    [Header("Animation")]
    public string idleAnimationName = "Idle"; // Oynatılacak idle animasyonunun adı
    private Animator animator;

    [Header("Target Scanning")]
    public float scanInterval = 1.0f; // Saniye cinsinden tarama aralığı
    public float detectionRadius = 1f; // Hedef tespit yarıçapı
    [Tooltip("Effective range for an attack. If target is within this, IsTargetInAttackRange will be true.")]
    public float attackRange = 2f;
    // chaseRangeThreshold detectionRadius'tan büyük olamaz, genellikle detectionRadius'a eşittir
    // veya detectionRadius içindeki bir "ilgi" eşiğidir.
    // Şimdilik, detectionRadius içinde ama attackRange dışında ise chaseRange'de kabul edelim.
    public LayerMask targetLayer; // Hangi katmandaki hedeflerin taranacağı
    public string playerTag = "Player"; // Oyuncunun etiketi

    // FSMC Parametre İsimleri (Controller'da bu isimlerle parametreler olmalı)
    [Tooltip("Boolean FSM parameter set to true when a target is found.")]
    public string targetFoundParameter = "TargetFound";
    // DetectedTarget artık FSM parametresi değil, Enemy script'inde olacak.
    // public string detectedTargetParameter = "DetectedTarget";
    [Tooltip("Boolean FSM parameter indicating if the target is within attack range.")]
    public string targetInAttackRangeParameter = "IsTargetInAttackRange"; // For direct transition to attack
    [Tooltip("Boolean FSM parameter indicating if the target is within chase range (but not attack range).")]
    public string targetInChaseRangeParameter = "IsTargetInChaseRange"; // For transition to follow


    private float lastScanTime;
    private FSMC_Executer fsmcExecuterComponent; // To avoid confusion with the parameter name

    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.StateInit(stateMachine, executer);
        fsmcExecuterComponent = executer;
        animator = executer.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"EnemyIdle ({GetType().Name}): Animator not found on FSMC_Executer's GameObject '{executer.gameObject.name}'.");
        }
    }

    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        // Debug.Log("IdleEntered");
        base.OnStateEnter(stateMachine, executer);
        if (animator != null && !string.IsNullOrEmpty(idleAnimationName))
        {
            animator.Play(idleAnimationName);
        }
        lastScanTime = Time.time - scanInterval; // İlk girişte hemen tarama yapması için

        // Duruma girerken FSM parametrelerini sıfırla/ayarla
        if (!string.IsNullOrEmpty(targetFoundParameter))
            stateMachine.SetBool(targetFoundParameter, false);
        
        // Enemy script'indeki detectedTarget'ı temizle
        var enemyScript = executer.GetComponent<Enemy>(); // Enemy.cs olduğunu varsayıyoruz
        if (enemyScript != null)
        {
            enemyScript.detectedTarget = null;
        }

        if (!string.IsNullOrEmpty(targetInAttackRangeParameter))
            stateMachine.SetBool(targetInAttackRangeParameter, false);
        if (!string.IsNullOrEmpty(targetInChaseRangeParameter))
            stateMachine.SetBool(targetInChaseRangeParameter, false);
    }

    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.OnStateUpdate(stateMachine, executer);
        if (Time.time >= lastScanTime + scanInterval)
        {
            ScanForTargets(stateMachine);
            lastScanTime = Time.time;
        }
    }

    private void ScanForTargets(FSMC_Controller stateMachine)
    {
        // Debug.Log($"[{Time.frameCount}] EnemyIdle: ScanForTargets called.");
        if (fsmcExecuterComponent == null)
        {
            Debug.LogError($"EnemyIdle ({GetType().Name}): fsmcExecuterComponent is null in ScanForTargets.");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(fsmcExecuterComponent.transform.position, detectionRadius, targetLayer);
        Transform foundTarget = null;
        var enemyScript = fsmcExecuterComponent.GetComponent<Enemy>();

        if (enemyScript == null)
        {
            Debug.LogError($"EnemyIdle ({GetType().Name}): Enemy script not found on FSMC_Executer's GameObject '{fsmcExecuterComponent.gameObject.name}'. Cannot set detectedTarget.");
            return;
        }

        // Debug.Log($"[{Time.frameCount}] EnemyIdle: OverlapCircle found {hits.Length} colliders. Player tag to check: '{playerTag}'");
        foreach (Collider2D hit in hits)
        {
            // Debug.Log($"[{Time.frameCount}] EnemyIdle: Checking hit '{hit.gameObject.name}' with tag '{hit.tag}'");
            if (hit.CompareTag(playerTag))
            {
                foundTarget = hit.transform;
                // Debug.Log($"[{Time.frameCount}] EnemyIdle: Player found: {foundTarget.name}");
                break;
            }
        }

        if (enemyScript.detectedTarget != foundTarget)
        {
           // Debug.Log($"[{Time.frameCount}] EnemyIdle: detectedTarget changing from '{(enemyScript.detectedTarget == null ? "NULL" : enemyScript.detectedTarget.name)}' to '{(foundTarget == null ? "NULL" : foundTarget.name)}'");
        }
        enemyScript.detectedTarget = foundTarget;

        if (foundTarget != null)
        {
            // Debug.Log($"[{Time.frameCount}] EnemyIdle: Processing FOUND target: {foundTarget.name}");
            if (!string.IsNullOrEmpty(targetFoundParameter))
                stateMachine.SetBool(targetFoundParameter, true);

            float distanceToTarget = Vector2.Distance(fsmcExecuterComponent.transform.position, foundTarget.position);

            if (distanceToTarget <= attackRange)
            {
                if (!string.IsNullOrEmpty(targetInAttackRangeParameter))
                    stateMachine.SetBool(targetInAttackRangeParameter, true);
                if (!string.IsNullOrEmpty(targetInChaseRangeParameter))
                    stateMachine.SetBool(targetInChaseRangeParameter, false);
            }
            else
            {
                if (!string.IsNullOrEmpty(targetInAttackRangeParameter))
                    stateMachine.SetBool(targetInAttackRangeParameter, false);
                if (!string.IsNullOrEmpty(targetInChaseRangeParameter))
                    stateMachine.SetBool(targetInChaseRangeParameter, true);
            }
            // Bu log zaten vardı, onu aktif bırakalım.
            // Debug.Log($"EnemyIdle ({GetType().Name}): Target found - {foundTarget.name} at distance {distanceToTarget}. InAttackRange: {stateMachine.GetBool(targetInAttackRangeParameter)}, InChaseRange: {stateMachine.GetBool(targetInChaseRangeParameter)}");
        }
        else // foundTarget == null
        {
            // Debug.Log($"[{Time.frameCount}] EnemyIdle: Processing NULL target (target lost or not found).");
            if (!string.IsNullOrEmpty(targetFoundParameter))
                stateMachine.SetBool(targetFoundParameter, false);
            if (!string.IsNullOrEmpty(targetInAttackRangeParameter))
                stateMachine.SetBool(targetInAttackRangeParameter, false);
            if (!string.IsNullOrEmpty(targetInChaseRangeParameter))
                stateMachine.SetBool(targetInChaseRangeParameter, false);
        }
        // Debug.Log($"[{Time.frameCount}] EnemyIdle: ScanForTargets complete. Enemy.detectedTarget is '{(enemyScript.detectedTarget == null ? "NULL" : enemyScript.detectedTarget.name)}'. TargetFound FSM: {stateMachine.GetBool(targetFoundParameter)}");
    }

    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        // Debug.Log("IdleExited");
        base.OnStateExit(stateMachine, executer);
        // Gerekirse çıkışta yapılacak temizlik işlemleri
    }
}