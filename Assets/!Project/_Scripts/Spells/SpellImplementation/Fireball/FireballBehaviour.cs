using PDollarGestureRecognizer;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballBehaviour", menuName = "SpellSystem/Spells/Fireball")]
public class FireballBehaviour : SpellBehaviour
{
    public GameObject fireballProjectile;



    public override void Consume()
    {
        LineRenderer renderer = GameObject.FindGameObjectWithTag("Drawing").GetComponent<LineRenderer>();
        var spawnPosition = renderer.GetCenterOfPoints();
        Debug.Log(spawnPosition);
        Instantiate(fireballProjectile, spawnPosition, Quaternion.Euler(new Vector3(0,0,0)));
    }
}
