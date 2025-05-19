using FSMC.Runtime;
using PDollarGestureRecognizer;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    private FSMC_Executer playerExecuter;
    private Result consumeResult;

    private bool isConsumed;
    Result ConsumeResult { 
        get 
        {
            if (!isConsumed)
            {
                isConsumed = true;
                return consumeResult;
            }
            else return new Result() { GestureClass = "null", Score = 0f };
        } 
        set 
        {
            consumeResult = value;
            isConsumed = false;
        } 
    }
    public void WantToAttackWithResult(Result result)
    {
        ConsumeResult = result;
        playerExecuter.SetTrigger("AttackStartTrigger");
    }
    public bool Attack()
    {
        return SpellHandler.Instance.ConsumeIfResultMatch(ConsumeResult);
    }
}
