using PDollarGestureRecognizer;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballBehaviour", menuName = "SpellSystem/Spells/Fireball")]
public class FireballBehaviour : SpellBehaviour
{
    public GameObject fireballProjectile;

    public Vector3 direction;

    public Vector3 spawnPosition;

    public override void Consume()
    {
        LineRenderer renderer = GameObject.FindGameObjectWithTag("Drawing").GetComponent<LineRenderer>();

        Vector3[] positions = new Vector3[renderer.positionCount];
        Vector2 center = Vector2.zero;
        renderer.GetPositions(positions);
        foreach (var item in positions)
        {
            center = center + (Vector2)item;
        }
        spawnPosition = center / renderer.positionCount;
        Debug.Log(spawnPosition);
        Instantiate(fireballProjectile, spawnPosition, Quaternion.Euler(new Vector3(60,0,0)));
    }

    
}
