using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMC.Runtime;
using System;

[Serializable]
public class PlayerAttackBehaviour : FSMC_Behaviour
{
    PlayerAttack playerAttack;
    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        playerAttack = executer.GetComponent<PlayerAttack>();
    }
    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        bool isAttackAccomplished = playerAttack.Attack();
        Debug.Log(isAttackAccomplished);
        stateMachine.SetTrigger("AttackFinishedTrigger");
    }

    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
    
    }

    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
    
    }
}