using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMC.Runtime;
using System;
using PDollarGestureRecognizer;

[Serializable]
public class ConsumeBehaviour : FSMC_Behaviour
{
    DrawHandler drawHandler;
    Result result;
    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        drawHandler = executer.GetComponent<DrawHandler>();
    }
    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        Debug.Log("Consume");
        result = drawHandler.RecognizePointCloud(drawHandler.DrawedPoints);
        drawHandler.playerAttack.WantToAttackWithResult(result);
        stateMachine.SetTrigger("Consumed");

    }
    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
    
    }
    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        Debug.Log($"{result.GestureClass} : {result.Score}");
    }
}