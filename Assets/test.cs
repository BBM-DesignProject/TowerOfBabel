using FSMC.Runtime;
using Unity.VisualScripting;
using UnityEngine;

[IncludeInSettings(true)]
public class test: FSMC_Behaviour
{

    public void OnEnterState()
    {
        Debug.Log("Entered");
    }

    public void OnExitState()
    {
        Debug.Log("Exited"); ;
    }

    public void OnUpdateState()
    {
        Debug.Log("Updated");

    }
}
