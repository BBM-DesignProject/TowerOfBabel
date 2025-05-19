using UnityEngine;
using FSMC.Runtime;
using System;

// Aksiyon tipini belirlemek i�in bir enum
public enum EnemyActionType
{
    MeleeAttack,
    ProjectileAttack,
    HealSelf, // �rnek ba�ka bir aksiyon
    CustomAction // Di�er �zel aksiyonlar i�in
}

[Serializable]
// [CreateAssetMenu(fileName = "EnemyActionBehaviour", menuName = "FSMC/Enemy Behaviours/Action")]
public class EnemyAction : FSMC_Behaviour
{
    [Header("General Action Settings")]
    public EnemyActionType actionType = EnemyActionType.MeleeAttack;
    [Tooltip("Animation to play for this action. Can be different for each action type if needed by using multiple behaviours or conditional logic.")]
    public string actionAnimationName = "Attack"; // Varsay�lan sald�r� animasyonu
    [Tooltip("Duration of the action. FSM should transition after this duration.")]
    public float actionDuration = 1.0f;

    [Header("Melee Attack Specifics")]
    [Tooltip("Damage for melee attack.")]
    public float meleeDamage = 10f;
    [Tooltip("Range within which melee attack can hit.")]
    public float meleeHitRange = 1.5f;
    [Tooltip("Offset from the enemy's pivot to the center of the melee attack area.")]
    public Vector2 meleeAttackOffset = Vector2.up;
    [Tooltip("Size of the OverlapBox for melee hit detection.")]
    public Vector2 meleeAttackBoxSize = new Vector2(1f, 1f);
    public LayerMask playerLayerMask;

    [Header("Projectile Attack Specifics")]
    public GameObject projectilePrefab;
    [Tooltip("Damage for projectile attack (if projectile doesn't handle its own damage).")]
    public float projectileDamage = 10f;
    [Tooltip("Optional: Transform point from which projectiles are spawned.")]
    public Transform projectileSpawnPoint;
    public float projectileSpawnOffsetForward = 1f;

    [Header("Heal Self Specifics")]
    [Tooltip("Amount to heal when HealSelf action is performed.")]
    public float healAmount = 20f;
    public string healAnimationName = "Heal"; // Opsiyonel, iyile�me i�in ayr� animasyon

    // FSMC Parametre �simleri
    [Tooltip("Transform FSM parameter for the target of the action (if applicable).")]
    public string targetTransformParameter = "DetectedTarget"; // Art�k Enemy.cs'den al�nacak
    [Tooltip("Boolean FSM parameter to signal action completion.")]
    public string actionCompleteParameter = "ActionFinished";

    // Di�er state'lerle tutarl�l�k i�in FSM parametre isimleri (Bu script'te aktif kullan�lm�yor ama tan�ml�)
    [Tooltip("Boolean FSM parameter indicating if the target is within attack range (set to false if target moves out).")]
    public string targetInAttackRangeParameter = "IsTargetInAttackRange";
    [Tooltip("Boolean FSM parameter indicating if a valid target is still present (set to false if target is lost).")]
    public string targetFoundParameter = "TargetFound";

    [Tooltip("Float FSM parameter for the enemy's current health (for HealSelf).")]
    public string currentHealthParameter = "CurrentHealth";
    [Tooltip("Float FSM parameter for the enemy's max health (for HealSelf).")]
    public string maxHealthParameter = "MaxHealth";

    private Animator animator;
    private FSMC_Executer fsmcExecuterComponent;
    private Transform currentTarget; // Enemy.cs'den al�nacak
    private float actionStateEndTime;
    private bool actionLogicPerformedThisState;
    private Enemy enemyScript; // Ana d��man script'ine referans


    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.StateInit(stateMachine, executer);
        fsmcExecuterComponent = executer;
        animator = executer.GetComponent<Animator>();
        enemyScript = executer.GetComponent<Enemy>(); // Enemy.cs oldu�unu varsay�yoruz


        if (animator == null)
            Debug.LogWarning($"EnemyAction ({GetType().Name}): Animator not found on '{executer.gameObject.name}'.");
        if (enemyScript == null)
            Debug.LogError($"EnemyAction ({GetType().Name}): Enemy script not found on '{executer.gameObject.name}'. This behaviour requires it to get the target.");
        if (actionType == EnemyActionType.ProjectileAttack && projectilePrefab == null)
        {
            Debug.LogError($"EnemyAction ({GetType().Name}): Projectile Attack type selected but projectilePrefab is not assigned!");
        }
    }

    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        // Debug.Log("ActionEntered");
        base.OnStateEnter(stateMachine, executer);
        actionLogicPerformedThisState = false;

        if (enemyScript != null)
        {
            currentTarget = enemyScript.detectedTarget;
        }

        if ((actionType == EnemyActionType.MeleeAttack || actionType == EnemyActionType.ProjectileAttack) && currentTarget == null)
        {
            Debug.LogWarning($"EnemyAction ({GetType().Name}): Target-based action '{actionType}' selected but target is null on Enemy script. Action cannot proceed.");
            if (!string.IsNullOrEmpty(actionCompleteParameter))
                stateMachine.SetBool(actionCompleteParameter, true);
            return;
        }

        if (currentTarget != null)
        {
            Vector2 directionToTarget = (currentTarget.position - fsmcExecuterComponent.transform.position).normalized;
            if (directionToTarget != Vector2.zero)
            {
                float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
                fsmcExecuterComponent.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        string animToPlay = actionAnimationName;
        if (actionType == EnemyActionType.HealSelf && !string.IsNullOrEmpty(healAnimationName))
        {
            animToPlay = healAnimationName;
        }

        if (animator != null && !string.IsNullOrEmpty(animToPlay))
        {
            animator.Play(animToPlay, 0, 0f);
        }

        actionStateEndTime = Time.time + actionDuration;
        if (!string.IsNullOrEmpty(actionCompleteParameter))
            stateMachine.SetBool(actionCompleteParameter, false);
    }

    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.OnStateUpdate(stateMachine, executer);

        if (Time.time >= actionStateEndTime)
        {
            if (!string.IsNullOrEmpty(actionCompleteParameter))
                stateMachine.SetBool(actionCompleteParameter, true);
            return;
        }

        // Hedefin null olup olmad���n� tekrar kontrol etmeye gerek yok ��nk� Idle karar verecek.
        // Sadece aksiyon mant���n� tetikle.

        float actionTriggerTimePoint = actionStateEndTime - (actionDuration * 0.5f);

        if (!actionLogicPerformedThisState && Time.time >= actionTriggerTimePoint)
        {
            PerformSelectedAction(stateMachine);
            actionLogicPerformedThisState = true;
        }
    }

    private void PerformSelectedAction(FSMC_Controller stateMachine)
    {
        // currentTarget null kontrol� burada da yap�lmal�, OnEnter'da null de�ilken buraya gelindi�inde null olabilir.
        if ((actionType == EnemyActionType.MeleeAttack || actionType == EnemyActionType.ProjectileAttack) &&
            (enemyScript == null || enemyScript.detectedTarget == null))
        {
            Debug.LogWarning($"EnemyAction ({GetType().Name}): Target became null before PerformSelectedAction for a target-based action.");
            return;
        }
        // currentTarget'� tekrar alal�m, ��nk� arada de�i�mi� olabilir (�ok olas� de�il ama g�venli)
        if (enemyScript != null) currentTarget = enemyScript.detectedTarget;


        switch (actionType)
        {
            case EnemyActionType.MeleeAttack:
                PerformMeleeAttack();
                break;
            case EnemyActionType.ProjectileAttack:
                PerformProjectileAttack();
                break;
            case EnemyActionType.HealSelf:
                PerformHealSelf(stateMachine);
                break;
            case EnemyActionType.CustomAction:
                Debug.Log($"EnemyAction ({GetType().Name}): Performing CustomAction.");
                break;
        }
    }

    private void PerformMeleeAttack()
    {
        if (currentTarget == null) return; // currentTarget burada enemyScript.detectedTarget'tan al�nmal�yd�.
                                           // �stteki PerformSelectedAction i�inde zaten bu kontrol var.
        Vector2 attackDirection = fsmcExecuterComponent.transform.up;
        Vector2 attackCenter = (Vector2)fsmcExecuterComponent.transform.position + (attackDirection * meleeAttackOffset.y) + ((Vector2)fsmcExecuterComponent.transform.right * meleeAttackOffset.x);

        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackCenter, meleeAttackBoxSize, fsmcExecuterComponent.transform.eulerAngles.z, playerLayerMask);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(meleeDamage);
                Debug.Log($"{fsmcExecuterComponent.name} meleed {playerCollider.name} for {meleeDamage} damage.");
            }
        }
    }

    private void PerformProjectileAttack()
    {
        if (projectilePrefab == null) return;
        // currentTarget null kontrol� PerformSelectedAction'da yap�ld�.
        // if (currentTarget == null) return; 

        Transform spawnPointToUse = projectileSpawnPoint != null ? projectileSpawnPoint : fsmcExecuterComponent.transform;
        Vector3 spawnPos;
        Quaternion spawnRot = spawnPointToUse.rotation;

        if (projectileSpawnPoint != null)
        {
            spawnPos = projectileSpawnPoint.position;
        }
        else
        {
            spawnPos = fsmcExecuterComponent.transform.position + (fsmcExecuterComponent.transform.up * projectileSpawnOffsetForward);
        }

        GameObject projectileGO = UnityEngine.Object.Instantiate(projectilePrefab, spawnPos, spawnRot);
        Debug.Log($"{fsmcExecuterComponent.name} fired a projectile.");
    }

    private void PerformHealSelf(FSMC_Controller stateMachine)
    {
        Debug.Log($"{fsmcExecuterComponent.name} performed HealSelf for {healAmount} (simulated).");
    }

    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        Debug.Log("ActionExited");
        base.OnStateExit(stateMachine, executer);
        if (!string.IsNullOrEmpty(actionCompleteParameter))
            stateMachine.SetBool(actionCompleteParameter, true);
    }
}
