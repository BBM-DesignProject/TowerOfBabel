using System.Collections.Generic;
using UnityEngine;

public class SnowProjectile : SpellProjectile
{
    public ParticleSystem particleOfField;
    [SerializeField] private Collider2D spellCollider;
    [SerializeField] private float delayBeforeDamage = 0.5f; // Adjust based on your animation


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


   
        // Play the effect
        particleOfField.Play();

        // Calculate when to enable the collider (e.g., at 50% of the effect duration)
        float enableTime = main.duration * delayBeforeDamage;

        // Start the timing coroutine
        StartCoroutine(SynchronizeColliderWithEffect(enableTime));

    }

    private void Start()
    {

        // Disable collider initially
        spellCollider.enabled = false;

        CastSpell();
    }

    private void Update()
    {
        if (particleOfField.isStopped) Destroy(gameObject);

    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        DealDamage(collision);
    }

    void DealDamage(Collider2D other)
    {
        
        // Düþmana çarpýp çarpmadýðýný kontrol et
        if (other.CompareTag(enemyTag))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log("Spell projectile hit " + other.name + " for " + damageAmount + " damage.");
            }
        }
        
    }

    private System.Collections.IEnumerator SynchronizeColliderWithEffect(float enableTime)
    {
        // Wait until the specified time in the effect's animation
        yield return new WaitForSeconds(enableTime);

        // Enable the collider at the appropriate moment in the animation
        spellCollider.enabled = true;

        // Wait for the remaining duration of the effect
        float remainingTime = particleOfField.main.duration - enableTime;
        yield return new WaitForSeconds(remainingTime);

        // Disable the collider when the effect finishes
        spellCollider.enabled = false;
    }
}
