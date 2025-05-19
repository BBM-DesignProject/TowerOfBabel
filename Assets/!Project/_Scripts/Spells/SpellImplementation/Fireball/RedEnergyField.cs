using UnityEngine;

public class RedEnergyField : SpellProjectile
{
    public ParticleSystem particleOfField;
    public float timeElapsed;

    public override void CastSpell()
    {
        // Make sure looping is turned off
        var main = particleOfField.main;
        main.loop = false;
        main.prewarm = false; // Make sure prewarm is off

        main.stopAction = ParticleSystemStopAction.Destroy;

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
        particleOfField = GetComponent<ParticleSystem>();

        CastSpell();
        timeElapsed = 0;
    }

    private void Update()
    {
        //timeElapsed += Time.deltaTime;
        //if (timeElapsed >= particleOfField.main.duration) particleOfField.Stop();
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