using UnityEngine;

public class Bomb : MonoBehaviour
{
    // BOMB SETTINGS

    [Range(0.5f, 10f)]
    [SerializeField] private float fuseTime = 4f;

    [Range(0.5f, 10f)]
    [SerializeField] private float explosionRadius = 3f;

    [Range(1f, 30f)]
    [SerializeField] private float explosionForce = 12f;


    // SETUP

    private void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }


    // EXPLOSION

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            // Find the rigid body to target
            Rigidbody2D targetRB = hit.attachedRigidbody;

            // Skip if there is no rb on the hit
            if (targetRB == null)
            {
                continue;
            }

            // if the rigid body is not "dynamic" then continue
            if (targetRB.bodyType != RigidbodyType2D.Dynamic)
            {
                continue;
            }

            // therefore all thats left is rbs that are dynamic (like the player etc)

            // calculate the explosion direction
            Vector2 direction = (targetRB.worldCenterOfMass - (Vector2)transform.position).normalized;

            // calculate the actual force based on direction and distance
            targetRB.AddForce(direction * explosionForce, ForceMode2D.Impulse);

            Debug.Log("Explosion hit: " + hit.name);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}