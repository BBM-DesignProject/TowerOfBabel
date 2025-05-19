using PDollarGestureRecognizer;
using UnityEngine;

public abstract class SpellBehaviour: ScriptableObject
{
    [SerializeField]
    protected float minimumRecognisionScore = 0.8f;
    [SerializeField]
    protected string gestureClass;

    // in seconds
    [SerializeField]
    protected float timeToCharge = 1f;


    protected Result compareResult;

    private void OnEnable()
    {
        InitGesture();
    }
    public bool IsGestureAccomplished(Result gestureResult)
    {
        var test = gestureResult.GestureClass.Equals(compareResult.GestureClass) && gestureResult.Score >= compareResult.Score;
        return test;
    }
    protected void InitGesture()
    {
        compareResult = new Result() { GestureClass = gestureClass, Score = minimumRecognisionScore };
    }

    public abstract void Consume();
}
