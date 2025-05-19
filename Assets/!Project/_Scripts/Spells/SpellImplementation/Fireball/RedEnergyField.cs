using System.Collections.Generic;
using UnityEngine;

public class RedEnergyField : SpellProjectile
{
    public ParticleSystem particleOfField;
    public float timeElapsed;

    private List<Transform> allreadyCollidedObjects = new();

    public override void CastSpell()
    {
        // Make sure looping is turned off
        var main = particleOfField.main;
        main.loop = false;
        main.prewarm = false; // Make sure prewarm is off

        main.stopAction = ParticleSystemStopAction.None;

        // Get all particle systems in this hierarchy (including children)
        ParticleSystem[] allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

        // Disable looping on ALL of them
        foreach (ParticleSystem ps in allParticleSystems)
        {
            var mainChildren = ps.main;
            mainChildren.loop = false;
        }


        // Also disable continuous emission
        var emission = particleOfField.emission;
        emission.enabled = false; // Or set rates to 0

        // Play the effect
        particleOfField.Play();

    }

    private void Start()
    {
        CastSpell();
    }

    private void Update()
    {
        if (particleOfField.isStopped) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Düþmana çarpýp çarpmadýðýný kontrol et
        if (other.CompareTag(enemyTag) && allreadyCollidedObjects.Exists(p => p.Equals(other.transform)))
        {
            allreadyCollidedObjects.Add((other.transform));
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log("Spell projectile hit " + other.name + " for " + damageAmount + " damage.");
            }
        }
    }



}

public abstract class SpellProjectile: MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    public float damageAmount = 15f;
    public string enemyTag = "Enemy";

    public abstract void CastSpell();
}