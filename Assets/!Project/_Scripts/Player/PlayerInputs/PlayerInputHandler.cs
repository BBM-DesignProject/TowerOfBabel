using TowerOfBabel.Input;
using UnityEngine;

public class PlayerInputHandler : MonoSingleton<PlayerInputHandler>
{
    private PlayerInputs inputActions;

    public PlayerInputs InputActions => inputActions;
    public override void Init()
    {
        inputActions = new PlayerInputs();
    }
}


//public abstract class InputActionHandler<T, TActionMap>: MonoSingleton<T> where T: MonoSingleton<T>
//{
//    TActionMap actionMap;
//}