using System;
using System.Threading;
using UnityEngine;


[CreateAssetMenu(fileName = "LightningBehaviour", menuName = "SpellSystem/Spells/Lightning")]
public class LightningBehaviour : SpellBehaviour
{
    public float radius;
    public LayerMask enemyLayer;
    public int maxTargets;

    public GameObject lightningPrefab;


    public override void Consume()
    {

        LineRenderer renderer = GameObject.FindGameObjectWithTag("Drawing").GetComponent<LineRenderer>();
        Vector2 effectCenter = renderer.GetCenterOfPoints();
        Debug.Log(effectCenter);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(effectCenter, radius, enemyLayer);

        // Cap number of targets
        int targetCount = maxTargets;

        StruckeEnemies(hitEnemies, targetCount);
    }

    private void StruckeEnemies(Collider2D[] hitEnemies, int targetCount)
    {
        if (hitEnemies.Length == 0) return;

        if (targetCount > 0)
        {

            for (int i = 0; i < targetCount; i++)
            {

                Enemy enemy = hitEnemies[i%hitEnemies.Length].GetComponent<Enemy>();
                if (enemy != null)
                {

                    
                    CreateLightningEffect(enemy.transform.position, enemy);
                }
            }

            //// Play sound effect
            //AudioManager.instance.PlaySound("lightning");
        }
    }

    private void CreateLightningEffect(Vector2 spawnPoint, Enemy enemyToHit)
    {
        // Instantiate lightning bolt between points
        GameObject lightning = Instantiate(lightningPrefab, spawnPoint, Quaternion.Euler(new Vector3(0, 0, 0)));
        LightningProjectile projectile = lightning.GetComponent<LightningProjectile>();
        if(projectile != null)
        {
            projectile.enemy = enemyToHit;
        }
    }
}
