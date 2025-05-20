using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMC.Runtime;
using System;
using PDollarGestureRecognizer;
using UnityEngine.EventSystems;

[Serializable]
public class DrawingBehaviour : FSMC_Behaviour
{
    DrawHandler drawHandler;

    public override void StateInit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        drawHandler = executer.GetComponent<DrawHandler>();
    }
    public override void OnStateEnter(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        drawHandler.PrepareForNewStroke();
    }

    public override void OnStateUpdate(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        Vector2 mousePos = drawHandler.CurrMousePosition;

        if (mousePos.Equals(Vector2.positiveInfinity)) return;

        drawHandler.AddPointToLine(mousePos);

    }

    public override void OnStateExit(FSMC_Controller stateMachine, FSMC_Executer executer)
    {
        var test = drawHandler.DrawedPoints;
    }
}