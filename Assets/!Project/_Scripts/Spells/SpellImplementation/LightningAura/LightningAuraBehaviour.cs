using UnityEngine;

[CreateAssetMenu(fileName = "LightningAuraBehaviour", menuName = "SpellSystem/Spells/LightningAura")]
public class LightningAuraBehaviour : SpellBehaviour
{
    public GameObject lightningAura;



    public override void Consume()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        Instantiate(lightningAura, player);
    }
}
