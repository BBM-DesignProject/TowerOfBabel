using PDollarGestureRecognizer;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballBehaviour", menuName = "SpellSystem/Spells/Fireball")]
public class FireballBehaviour : SpellBehaviour
{
    public GameObject fireballProjectile;

    public Vector3 direction;

    public Vector3 spawnPosition;

    public override void Consume()
    {
        spawnPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        direction = GameObject.FindGameObjectWithTag("Player").transform.right;

        Instantiate(fireballProjectile, spawnPosition, Quaternion.FromToRotation(Vector3.right, direction));
    }

    
}
