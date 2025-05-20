using UnityEngine;

[CreateAssetMenu(fileName = "XpConsumeBehaviour", menuName = "SpellSystem/Spells/XpConsume")]

public class XpSpellBehaviour : SpellBehaviour
{
    public override void Consume()
    {
        Debug.Log(PlayerExperienceHandler.Instance.SpendXP());
    }
}
