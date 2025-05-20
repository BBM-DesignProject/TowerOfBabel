using UnityEngine;

[CreateAssetMenu(fileName = "WavesBehaviour", menuName = "SpellSystem/Spells/Waves")]

public class WavesSpellBehaviour : SpellBehaviour
{
    public GameObject wavesProjectile;

    public override void Consume()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        LineRenderer renderer = GameObject.FindGameObjectWithTag("Drawing").GetComponent<LineRenderer>();
        
        
        var shootDirection = renderer.GetCenterOfPoints() - (Vector2)playerTransform.position;
        

        var projectile = Instantiate(wavesProjectile, playerTransform.position, Quaternion.FromToRotation(Vector3.right,shootDirection));


    }

}
