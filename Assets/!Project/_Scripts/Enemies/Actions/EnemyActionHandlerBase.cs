using FSMC.Runtime;
using UnityEngine;

public abstract class EnemyActionHandlerBase : MonoBehaviour
{
    [Header("Base Action Settings")]
    [Tooltip("Name of the Animator trigger parameter to play the action animation.")]
    public string actionAnimationTrigger = "AttackTrigger"; // Varsayılan trigger adı

    [Tooltip("Default duration for this action. Specific handlers can override this via their ActionDuration property.")]
    public float baseActionDuration = 1.0f;

    // ActionAnimationName özelliği artık doğrudan kullanılmayacak, trigger tercih edilecek.
    // İstenirse bir fallback olarak kalabilir veya kaldırılabilir.
    // public virtual string ActionAnimationName => "Attack";

    // ActionDuration özelliği, baseActionDuration'ı veya alt sınıfın override'ını kullanacak.
    public virtual float ActionDuration => baseActionDuration;

    protected Animator animator;
    protected Enemy enemyScript;
    protected FSMC_Executer fsmcExecuter; // FSMC_Executer'a erişim için (opsiyonel)

    protected virtual void Awake()
    {
        animator = GetComponentInParent<Animator>(); // Veya GetComponent<Animator>() / GetComponentInChildren<Animator>()
        enemyScript = GetComponentInParent<Enemy>();
        fsmcExecuter = GetComponentInParent<FSMC_Executer>();

        if (enemyScript == null)
        {
            Debug.LogError($"{GetType().Name} on {gameObject.name}: Enemy script not found in parent. This is required.");
        }
        // Animator veya FSMC_Executer opsiyonel olabilir, her aksiyon bunları kullanmayabilir.
        // if (animator == null)
        // {
        //     Debug.LogWarning($"{GetType().Name} on {gameObject.name}: Animator not found in parent.");
        // }
    }

    /// <summary>
    /// Checks if this action can currently be executed.
    /// Can be overridden for specific conditions (e.g., cooldown, resource check).
    /// </summary>
    /// <param name="target">The current target for the action, if any.</param>
    /// <returns>True if the action can be executed, false otherwise.</returns>
    public virtual bool CanExecuteAction(Transform target)
    {
        return true; // Varsayılan olarak her zaman çalıştırılabilir
    }

    /// <summary>
    /// Executes the defined action.
    /// </summary>
    /// <param name="target">The current target for the action, if any.</param>
    public abstract void ExecuteAction(Transform target);

    /// <summary>
    /// Called by the FSM Behaviour when the action state is entered.
    /// Useful for one-time setup or animation triggering.
    /// </summary>
    public virtual void OnActionEnter(Transform target)
    {
        // Varsayılan olarak trigger'ı kullanarak animasyonu tetikle
        if (animator != null && !string.IsNullOrEmpty(actionAnimationTrigger))
        {
            animator.SetTrigger(actionAnimationTrigger);
            // Debug.Log($"{GetType().Name} on {gameObject.name}: Triggering '{actionAnimationTrigger}'");
        }
        else if (animator == null)
        {
            Debug.LogWarning($"{GetType().Name} on {gameObject.name}: Animator is null. Cannot trigger animation.");
        }
        else
        {
            Debug.LogWarning($"{GetType().Name} on {gameObject.name}: actionAnimationTrigger is not set. Cannot trigger animation.");
        }
    }

    /// <summary>
    /// Called by the FSM Behaviour every update tick while in the action state.
    /// Useful for actions that need continuous updates or checks.
    /// </summary>
    public virtual void OnActionUpdate(Transform target)
    {
        // Alt sınıflar implemente edebilir
    }

    /// <summary>
    /// Called by the FSM Behaviour when the action state is exited.
    /// Useful for cleanup.
    /// </summary>
    public virtual void OnActionExit()
    {
        // Alt sınıflar implemente edebilir
    }
}