using UnityEngine;
using FSMC.Runtime;
using System;

[Serializable]
// [CreateAssetMenu(fileName = "EnemyActionStateBehaviour", menuName = "FSMC/Enemy Behaviours/Action")]
public class EnemyAction : FSMC_Behaviour
{
    [Header("Action State Settings")]
    [Tooltip("Base duration for this action state if the specific handler doesn't provide one. The handler's ActionDuration will override this if greater than 0.")]
    public float actionStateBaseDuration = 1.0f; 
    [Tooltip("FSM Boolean parameter to signal action completion and readiness to transition.")]
    public string actionCompleteFSMParameter = "ActionFinished";

    private FSMC_Executer fsmcExecuterComponent;
    private Enemy enemyScript; 
    private EnemyActionHandlerBase currentActionHandler; 
    private Transform currentTarget;

    private float actionStateEndTime;
    private bool actionExecutionLogicCalled; 

    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.StateInit(stateMachine, executer);
        fsmcExecuterComponent = executer;
        enemyScript = executer.GetComponent<Enemy>();
        // GetComponentInChildren, Handler'ın Enemy objesinin bir alt objesinde olmasına da izin verir.
        // Eğer Handler her zaman Enemy objesinin kendisindeyse GetComponent de yeterli.
        currentActionHandler = executer.GetComponentInChildren<EnemyActionHandlerBase>(); 

        if (enemyScript == null)
            Debug.LogError($"EnemyAction ({GetType().Name}): Enemy script not found on '{executer.gameObject.name}'. This is required to get the target.");
        if (currentActionHandler == null)
            Debug.LogError($"EnemyAction ({GetType().Name}): No EnemyActionHandlerBase component found on '{executer.gameObject.name}' or its children. An action handler is required for this state.");
    }

    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.OnStateEnter(stateMachine, executer);
        actionExecutionLogicCalled = false;
        // Debug.Log($"[{Time.frameCount}] EnemyAction: OnStateEnter. Handler: {(currentActionHandler != null ? currentActionHandler.GetType().Name : "NULL")}");

        if (enemyScript != null)
        {
            currentTarget = enemyScript.detectedTarget; 
        }
        else 
        {
            Debug.LogError($"EnemyAction ({GetType().Name}): Enemy script is null in OnStateEnter. Cannot get target.");
            if (!string.IsNullOrEmpty(actionCompleteFSMParameter))
                stateMachine.SetBool(actionCompleteFSMParameter, true); 
            return;
        }
        
        if (currentActionHandler == null)
        {
            Debug.LogError($"EnemyAction ({GetType().Name}): currentActionHandler is null in OnStateEnter. Cannot proceed.");
            if (!string.IsNullOrEmpty(actionCompleteFSMParameter))
                stateMachine.SetBool(actionCompleteFSMParameter, true); 
            return;
        }
        
        // Hedefe dönme (Handler'ın sorumluluğunda olabilir veya burada kalabilir)
        if (currentTarget != null)
        {
            Vector2 directionToTarget = (currentTarget.position - fsmcExecuterComponent.transform.position).normalized;
            if (directionToTarget != Vector2.zero)
            {
                float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
                fsmcExecuterComponent.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        
        currentActionHandler.OnActionEnter(currentTarget); 
        
        float duration = currentActionHandler.ActionDuration > 0.001f ? currentActionHandler.ActionDuration : actionStateBaseDuration;
        actionStateEndTime = Time.time + duration; 

        if (!string.IsNullOrEmpty(actionCompleteFSMParameter))
            stateMachine.SetBool(actionCompleteFSMParameter, false); 
    }

    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.OnStateUpdate(stateMachine, executer);

        if (currentActionHandler == null) 
        {
            if (!string.IsNullOrEmpty(actionCompleteFSMParameter)) stateMachine.SetBool(actionCompleteFSMParameter, true);
            return;
        }
        
        if (enemyScript != null) currentTarget = enemyScript.detectedTarget;

        float currentHandlerDuration = currentActionHandler.ActionDuration > 0.001f ? currentActionHandler.ActionDuration : actionStateBaseDuration;
        // Aksiyonun "etki noktası"nı belirler (0.0: hemen, 0.5: ortasında, 1.0: bitmeden hemen önce)
        // Bu değer, Handler'ın kendi iç mantığına veya animasyon event'lerine göre daha esnek yönetilebilir.
        // Şimdilik, sürenin ortasında bir kez tetikleme mantığını koruyalım.
        float triggerTimeRatio = 0.5f; 
        float actionTriggerTimePoint = (actionStateEndTime - currentHandlerDuration) + (currentHandlerDuration * triggerTimeRatio);

        if (!actionExecutionLogicCalled && Time.time >= actionTriggerTimePoint && Time.time < actionStateEndTime)
        {
            if (currentActionHandler.CanExecuteAction(currentTarget)) 
            {
                // Debug.Log($"[{Time.frameCount}] EnemyAction: Calling ExecuteAction on {currentActionHandler.GetType().Name}");
                currentActionHandler.ExecuteAction(currentTarget);
                actionExecutionLogicCalled = true;
            }
        }
        
        currentActionHandler.OnActionUpdate(currentTarget); 

        if (Time.time >= actionStateEndTime)
        {
            // Debug.Log($"[{Time.frameCount}] EnemyAction: Action state duration ended. Setting '{actionCompleteFSMParameter}' to true.");
            if (!string.IsNullOrEmpty(actionCompleteFSMParameter))
                stateMachine.SetBool(actionCompleteFSMParameter, true); 
        }
    }
    
    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        base.OnStateExit(stateMachine, executer);
        // Debug.Log($"[{Time.frameCount}] EnemyAction: OnStateExit. Handler: {(currentActionHandler != null ? currentActionHandler.GetType().Name : "NULL")}");
        if (currentActionHandler != null)
        {
            currentActionHandler.OnActionExit(); 
        }

        if (!string.IsNullOrEmpty(actionCompleteFSMParameter))
        {
            // Emin olmak için, eğer bir şekilde true set edilmemişse burada set et.
            if (stateMachine.GetBool(actionCompleteFSMParameter) == false) 
            {
                // Debug.LogWarning($"[{Time.frameCount}] EnemyAction: Forcing '{actionCompleteFSMParameter}' to true on exit.");
                stateMachine.SetBool(actionCompleteFSMParameter, true);
            }
        }
    }
}
