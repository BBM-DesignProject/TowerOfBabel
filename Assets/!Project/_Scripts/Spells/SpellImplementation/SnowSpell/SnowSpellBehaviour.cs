using UnityEngine;

[CreateAssetMenu(fileName = "SnowSpellBehaviour", menuName = "SpellSystem/Spells/SnowSpell")]
public class SnowSpellBehaviour : SpellBehaviour
{
    public GameObject snowProjectile;



    public override void Consume()
    {
        LineRenderer renderer = GameObject.FindGameObjectWithTag("Drawing").GetComponent<LineRenderer>();
        var spawnPosition = renderer.GetCenterOfPoints();
        Debug.Log(spawnPosition);
        Instantiate(snowProjectile, spawnPosition, Quaternion.Euler(new Vector3(0, 0, 0)));
    }
}
