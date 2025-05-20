using UnityEngine;

public class LightningAuraProjectile : SpellProjectile
{
    public ParticleSystem particleOfField;
    [SerializeField] private Collider2D spellCollider;


    private float elapsedTime;
    public override void CastSpell()
    {

        particleOfField.GetComponent<Renderer>().sortingLayerName = "VFX";


        // Get all particle systems in this hierarchy (including children)
        ParticleSystem[] allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

        // Disable looping on ALL of them
        foreach (ParticleSystem ps in allParticleSystems)
        {

            ps.GetComponent<Renderer>().sortingLayerName = "VFX";

        }
        // Play the effect
        particleOfField.Play();


    }

    private void Start()
    {
        elapsedTime = 0f;
        CastSpell();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= lifetime) particleOfField.Stop(true);
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
