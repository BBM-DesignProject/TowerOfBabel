using PDollarGestureRecognizer;
using UnityEngine;

[CreateAssetMenu(fileName ="SpellObject",menuName = "SpellSystem/SpellObject")]
public class SpellSO : ScriptableObject
{
    public SpellBehaviour spellBehaviour;

    public void Consume()
    {
        spellBehaviour.Consume();
    }

    public bool IsGestureAccomplished(Result result)
    {
        return spellBehaviour.IsGestureAccomplished(result);
    }
}
