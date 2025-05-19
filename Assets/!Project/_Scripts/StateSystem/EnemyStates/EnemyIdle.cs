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
    private FSMC_Executer fsmcExecuterComponent;
    private Enemy enemyScriptInstance; // Merkezi menzil değerlerini okumak ve detectedTarget'ı set etmek için

    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.StateInit(stateMachine, executer);
        fsmcExecuterComponent = executer;
        animator = executer.GetComponent<Animator>();
        // Enemy script'ine referans alalım, StateInit'te bir kez yapmak daha iyi.
        enemyScriptInstance = executer.GetComponent<Enemy>();

        if (animator == null)
        {
            Debug.LogWarning($"EnemyIdle ({GetType().Name}): Animator not found on FSMC_Executer's GameObject '{executer.gameObject.name}'.");
        }
        if (enemyScriptInstance == null)
        {
            Debug.LogError($"EnemyIdle ({GetType().Name}): Enemy script not found on FSMC_Executer's GameObject '{executer.gameObject.name}'. This behaviour needs it for range values.");
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
        if (enemyScriptInstance != null)
        {
            enemyScriptInstance.detectedTarget = null;
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
        if (fsmcExecuterComponent == null || enemyScriptInstance == null)
        {
            Debug.LogError($"EnemyIdle ({GetType().Name}): fsmcExecuterComponent or enemyScriptInstance is null in ScanForTargets.");
            return;
        }

        // Menzilleri Enemy script'inden oku
        float currentDetectionRadius = enemyScriptInstance.detectionRadius;
        float currentActionRange = enemyScriptInstance.actionRange; // Enemy.cs'deki actionRange'i kullan

        Collider2D[] hits = Physics2D.OverlapCircleAll(fsmcExecuterComponent.transform.position, currentDetectionRadius, targetLayer);
        Transform foundTarget = null;
        
        // Debug.Log($"[{Time.frameCount}] EnemyIdle: OverlapCircle ({currentDetectionRadius}) found {hits.Length} colliders. Player tag to check: '{playerTag}'");
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

        if (enemyScriptInstance.detectedTarget != foundTarget) // enemyScriptInstance kullanılmalı
        {
           // Debug.Log($"[{Time.frameCount}] EnemyIdle: detectedTarget changing from '{(enemyScriptInstance.detectedTarget == null ? "NULL" : enemyScriptInstance.detectedTarget.name)}' to '{(foundTarget == null ? "NULL" : foundTarget.name)}'");
        }
        enemyScriptInstance.detectedTarget = foundTarget; // enemyScriptInstance kullanılmalı

        if (foundTarget != null)
        {
            // Debug.Log($"[{Time.frameCount}] EnemyIdle: Processing FOUND target: {foundTarget.name}");
            if (!string.IsNullOrEmpty(targetFoundParameter))
                stateMachine.SetBool(targetFoundParameter, true);

            float distanceToTarget = Vector2.Distance(fsmcExecuterComponent.transform.position, foundTarget.position);

            if (distanceToTarget <= currentActionRange) // currentActionRange kullanıldı
            {
                if (!string.IsNullOrEmpty(targetInAttackRangeParameter))
                    stateMachine.SetBool(targetInAttackRangeParameter, true);
                if (!string.IsNullOrEmpty(targetInChaseRangeParameter))
                    stateMachine.SetBool(targetInChaseRangeParameter, false);
            }
            else // Hedef actionRange dışında ama currentDetectionRadius içinde
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
        // Debug.Log($"[{Time.frameCount}] EnemyIdle: ScanForTargets complete. Enemy.detectedTarget is '{(enemyScriptInstance.detectedTarget == null ? "NULL" : enemyScriptInstance.detectedTarget.name)}'. TargetFound FSM: {stateMachine.GetBool(targetFoundParameter)}");
    }

    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        // Debug.Log("IdleExited");
        base.OnStateExit(stateMachine, executer);
        // Gerekirse çıkışta yapılacak temizlik işlemleri
    }
}