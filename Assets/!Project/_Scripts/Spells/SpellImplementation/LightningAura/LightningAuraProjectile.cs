using UnityEngine;

public class LightningAuraProjectile : SpellProjectile
{
    public ParticleSystem particleOfField;
    [SerializeField] private Collider2D spellCollider;

    private float start;
    public override void CastSpell()
    {
        // Make sure looping is turned off
        var main = particleOfField.main;
        main.loop = true;
        main.prewarm = true; // Make sure prewarm is off

        main.stopAction = ParticleSystemStopAction.None;

        // Get all particle systems in this hierarchy (including children)
        ParticleSystem[] allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

        // Disable looping on ALL of them
        foreach (ParticleSystem ps in allParticleSystems)
        {
            var mainChildren = ps.main;
            mainChildren.loop = true;
        }
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
}
