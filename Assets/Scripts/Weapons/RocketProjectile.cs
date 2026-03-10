using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectile : MonoBehaviour
{
    [Header("Explosion Force")]
    public float explosionForce = 20f;
    public float upwardsModifier = 1.5f;


    public float speed;
    public float damage;
    public float explosionRadius = 5f;

    public LayerMask damageMask;

    Vector3 direction;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Destroy(gameObject, 20f); // Destroy after 20 seconds if it doesn't hit anything
    }

    public void Initialize(Vector3 dir, float rocketSpeed, float damage)
    {
        direction = dir.normalized;
        speed = rocketSpeed;
        this.damage = damage;

        rb.velocity = direction * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            // Damage
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }

            // Player pushback
            if (hit.TryGetComponent<PlayerMovement>(out var player))
            {
                player.AddExplosionForce(transform.position, 20f, explosionRadius);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}