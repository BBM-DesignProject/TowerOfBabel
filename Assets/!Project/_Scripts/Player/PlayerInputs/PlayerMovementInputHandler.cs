using UnityEngine;
using TowerOfBabel.Input;
using UnityEngine.InputSystem;
using FSMC.Runtime;

public class PlayerMovementInputHandler : MonoSingleton<PlayerMovementInputHandler>, PlayerInputs.IPlayerActions
{
    [SerializeField]
    private FSMC_Executer playerMovementExecuter;

    PlayerInputs.PlayerActions playerActions;

    public void Start()
    {
        playerActions = PlayerInputHandler.Instance.InputActions.Player;
        playerActions.Enable();
        playerActions.AddCallbacks(this);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        //playerMovementExecuter.SetTrigger("AttackStartTrigger");
    }

    public void OnInteract(InputAction.CallbackContext context)
    {

    }

    public void OnLook(InputAction.CallbackContext context)
    {
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //playerMovementExecuter.SetTrigger("MoveStartTrigger");
            //playerMovementExecuter.GetCurrentState();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
    }
}
