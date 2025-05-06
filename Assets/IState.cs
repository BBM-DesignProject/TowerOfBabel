using UnityEngine;

public interface IState
{
    public void OnEnterState();
    public void OnExitState();
    public void OnUpdateState();
}
