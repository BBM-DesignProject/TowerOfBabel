using FSMC.Runtime;
using PDollarGestureRecognizer;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    private FSMC_Executer playerExecuter;

    private bool consumeResult;
    bool ConsumeResult { 
        get 
        {
            bool returnValue = consumeResult;
            consumeResult = false;
            return returnValue;
        } 
        set 
        {
            consumeResult = value;
        } 
    }
    public void Attack(bool isSuccessfull)
    {
        ConsumeResult = isSuccessfull;
        playerExecuter.SetTrigger("AttackStartTrigger");
        
    }
}
