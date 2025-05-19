using FSMC.Runtime;
using UnityEngine;

public abstract class EnemyActionHandlerBase : MonoBehaviour
{
    [Header("Base Action Settings")]
    [Tooltip("Default animation name for this action. Can be overridden by specific handlers.")]
    public virtual string ActionAnimationName => "Attack"; // Varsayılan, alt sınıflar override edebilir

    [Tooltip("Default duration for this action. Can be overridden by specific handlers.")]
    public virtual float ActionDuration => 1.0f; // Varsayılan, alt sınıflar override edebilir

    protected Animator animator;
    protected Enemy enemyScript; // Ana Enemy script'ine erişim için
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
        // Varsayılan olarak animasyonu oynat
        if (animator != null && !string.IsNullOrEmpty(ActionAnimationName))
        {
            animator.Play(ActionAnimationName, 0, 0f);
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