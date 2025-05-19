using PDollarGestureRecognizer;
using System.Collections.Generic;
using UnityEngine;

public class SpellHandler : MonoSingleton<SpellHandler>
{
    public SpellSO[] registeredSpells = new SpellSO[8];
    private int index = 0;

    public bool ConsumeIfResultMatch(Result result)
    {
        foreach (var item in registeredSpells)
        {

            if (item!=null && item.IsGestureAccomplished(result))
            {
                item.Consume();
                return true;
            }
        }
        return false;
    }

    public void RegisterSpell(SpellSO spell)
    {
        if (index >= 8) return;
        registeredSpells[index++] = spell;
    }


}
