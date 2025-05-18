using FSMC.Runtime;
using System;
using TowerOfBabel.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDrawInputHandler : MonoSingleton<PlayerDrawInputHandler>, PlayerInputs.IDrawingSystemActions, IInputActionHandler<PlayerInputs.DrawingSystemActions>
{
    [SerializeField]
    private FSMC_Executer executer;

    [SerializeField]
    private DrawHandler drawHandler;

    public PlayerInputs.DrawingSystemActions ActionMap { get; private set; }

    private void Start()
    {
        InitializeActionMap();
    }
    public void OnActivate(InputAction.CallbackContext context)
    {
        if(!context.performed)
            executer.SetTrigger("StartedDrawing");
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        executer.SetTrigger("CanceledDrawing");
    }

    public void OnConsume(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        executer.SetTrigger("WantToConsume");
    }

    public void InitializeActionMap()
    {
        ActionMap = PlayerInputHandler.Instance.InputActions.DrawingSystem;
        ActionMap.Enable();
        ActionMap.SetCallbacks(this);
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        
        drawHandler.CurrMousePosition = context.ReadValue<Vector2>();
    }

    public void OnDraw(InputAction.CallbackContext context)
    {
        Debug.Log(context.phase);
        if (context.performed)
        {
            executer.SetTrigger("ReturnedDrawing");
        }

        if (context.canceled)
        {
            executer.SetTrigger("NotDrawing");
        }
    }
}
