using UnityEngine;

public class LightningProjectile : SpellProjectile
{
    public ParticleSystem lightningEffect;
    [SerializeField] private float delayBeforeDamage = 0.5f; // Adjust based on your animation

    public Enemy enemy;

    private bool CanDamage = false;
    public override void CastSpell()
    {
        // Make sure looping is turned off
        var main = lightningEffect.main;
        main.loop = false;
        main.prewarm = false; // Make sure prewarm is off
        lightningEffect.GetComponent<Renderer>().sortingLayerName = "VFX";

        main.stopAction = ParticleSystemStopAction.None;


        // Get all particle systems in this hierarchy (including children)
        ParticleSystem[] allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

        // Disable looping on ALL of them
        foreach (ParticleSystem ps in allParticleSystems)
        {
            var mainChildren = ps.main;
            mainChildren.loop = false;
            ps.GetComponent<Renderer>().sortingLayerName = "VFX";

        }


        // Also disable continuous emission
        var emission = lightningEffect.emission;
        emission.enabled = false; // Or set rates to 0

        // Play the effect
        lightningEffect.Play();

        // Calculate when to enable the collider (e.g., at 50% of the effect duration)
        float enableTime = main.duration * delayBeforeDamage;

        // Start the timing coroutine
        StartCoroutine(SynchronizeColliderWithEffect(enableTime));

    }

    private void Start()
    {
        CastSpell();
    }

    private void Update()
    {
        DealDamage();
        if (lightningEffect.isStopped) Destroy(gameObject);

    }
    public void DealDamage()
    {
        if (!CanDamage) return;
        enemy.TakeDamage(damageAmount);
        Debug.Log("Spell projectile hit " + enemy.name + " for " + damageAmount + " damage.");
        CanDamage = false;
    }

    private System.Collections.IEnumerator SynchronizeColliderWithEffect(float enableTime)
    {
        // Wait until the specified time in the effect's animation
        yield return new WaitForSeconds(enableTime);

        //// Enable the collider at the appropriate moment in the animation
        CanDamage = true;

        // Wait for the remaining duration of the effect
        float remainingTime = lightningEffect.main.duration - enableTime;
        yield return new WaitForSeconds(remainingTime);

        //// Disable the collider when the effect finishes
        CanDamage = false;
    }
}
