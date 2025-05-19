using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMC.Runtime;
using System;

[Serializable]
public class PlayerIdleBeheviour : FSMC_Behaviour
{
    private PlayerMovement playerMovement;

    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        playerMovement = executer.GetComponent<PlayerMovement>();

    }
    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        Debug.Log("Entered idle");
    }

    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        playerMovement.LookToDireciton();
    }

    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
    
    }
}