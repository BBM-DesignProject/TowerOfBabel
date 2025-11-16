using System.Collections;
using PDollarGestureRecognizer;
using UnityEngine;

public class SpellHandler : MonoSingleton<SpellHandler>
{
    public SpellSO[] registeredSpells = new SpellSO[8];
    private int index = 0;


    public float duration;
    public AnimationCurve  curve;
    public Camera playerCamera;
    private void Start()
    {
        playerCamera = Camera.main;
        foreach (var item in registeredSpells)
        {
            if (item != null) item.InitGesture();
        }
    }
    public bool ConsumeIfResultMatch(Result result)
    {
        foreach (var item in registeredSpells)
        {

            if (item!=null && item.IsGestureAccomplished(result))
            {
                

                item.Consume();
                StartCoroutine(Shaking());
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

    private IEnumerator Shaking()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime/duration);
            playerCamera.transform.position += Random.insideUnitSphere * strength;
            yield return null;
        }
    }


}
