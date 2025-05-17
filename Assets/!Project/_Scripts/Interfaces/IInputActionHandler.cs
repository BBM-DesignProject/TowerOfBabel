using UnityEngine;

public interface IInputActionHandler<TActionMap>
{
    TActionMap ActionMap {get;}

    void InitializeActionMap();
}
