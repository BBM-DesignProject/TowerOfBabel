using PDollarGestureRecognizer;
using UnityEngine;

public abstract class SpellBehaviour: ScriptableObject
{
    [SerializeField]
    protected float minimumRecognisionScore;
    [SerializeField]
    protected string gestureClass;

    // in seconds
    [SerializeField]
    protected float timeToCharge;


    protected Result compareResult;


    public bool IsGestureAccomplished(Result gestureResult)
    {

        var test = gestureResult.GestureClass.Equals(compareResult.GestureClass) && gestureResult.Score >= compareResult.Score;
        return test;
    }
    public void InitGesture()
    {
        compareResult = new Result() { GestureClass = gestureClass, Score = minimumRecognisionScore };
    }

    public abstract void Consume();
}
