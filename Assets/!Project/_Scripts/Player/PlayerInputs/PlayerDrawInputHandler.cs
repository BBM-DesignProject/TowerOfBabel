using FSMC.Runtime;
using TowerOfBabel.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDrawInputHandler : MonoSingleton<PlayerDrawInputHandler>, PlayerInputs.IDrawingSystemActions, IInputActionHandler<PlayerInputs.DrawingSystemActions>
{
    public FSMC_Executer executer;
    public PlayerInputs.DrawingSystemActions ActionMap { get; private set; }

    private void Start()
    {
        InitializeActionMap();
    }
    public void OnActivate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("activated");
        }
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if(context.performed)
        Debug.Log("canceled");
    }

    public void OnConsume(InputAction.CallbackContext context)
    {
        Debug.Log("Consumed");
    }

    public void InitializeActionMap()
    {
        ActionMap = PlayerInputHandler.Instance.InputActions.DrawingSystem;
        ActionMap.Enable();
        ActionMap.SetCallbacks(this);
    }
}
