using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMC.Runtime;
using System;

[Serializable]
public class PlayerMoveBehaviour : FSMC_Behaviour
{
    private PlayerMovement playerMovement;
    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        playerMovement = executer.GetComponent<PlayerMovement>();
    }
    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
    }

    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {   
        playerMovement.MoveToDirection();
        playerMovement.LookToDireciton();
    }


    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
    
    }
}