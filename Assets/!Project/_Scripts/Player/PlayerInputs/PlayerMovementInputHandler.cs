using UnityEngine;
using TowerOfBabel.Input;
using UnityEngine.InputSystem;
using FSMC.Runtime;
using System.Runtime.InteropServices;

public class PlayerMovementInputHandler : MonoSingleton<PlayerMovementInputHandler>, PlayerInputs.IPlayerActions, IInputActionHandler<PlayerInputs.PlayerActions>
{
    [SerializeField]
    private FSMC_Executer playerMovementExecuter;

    [SerializeField]
    private PlayerMovement playerMovement;


    public PlayerInputs.PlayerActions ActionMap { get; private set; }

    private void Start()
    {
        InitializeActionMap();
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
        playerMovement.ReadMousePosition(context.ReadValue<Vector2>());
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerMovement.OnMove(context.ReadValue<Vector2>());
            playerMovementExecuter.SetTrigger("MoveStartTrigger");
        }
        else if(context.canceled) playerMovementExecuter.SetTrigger("MoveFinishedTrigger");

    }
    private void OnDestroy()
    {
        ActionMap.RemoveCallbacks(this);
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
    }

    public void InitializeActionMap()
    {
        ActionMap = PlayerInputHandler.Instance.InputActions.Player;
        ActionMap.Enable();
        ActionMap.SetCallbacks(this);
    }
}
