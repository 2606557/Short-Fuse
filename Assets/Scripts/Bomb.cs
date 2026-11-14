using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// WEEK 3B/C REFERENCE VERSION based on the existing Short Fuse bomb.
// Keep exactly ONE Bomb.cs in Assets.
public class Bomb : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool hasExploded;

    [Range(0.5f, 10f)] [SerializeField] private float fuseTime = 4f;
    [Range(0.5f, 10f)] [SerializeField] private float explosionRadius = 3f;
    [Range(1f, 30f)] [SerializeField] private float explosionForce = 12f;
    [SerializeField] private AudioClip fuseBeep;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private Color flashColor = Color.white;
    private Color originalColor;
    [Range(0.05f, 1f)] [SerializeField] private float slowFlashInterval = 0.4f;
    [Range(0.05f, 1f)] [SerializeField] private float fastFlashInterval = 0.15f;
    [Range(0.01f, 0.5f)] [SerializeField] private float imminentFlashInterval = 0.05f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        StartCoroutine(FlashFuse());
        Invoke(nameof(Explode), fuseTime);
    }

    // WEEK 3B: Allow a player command to detonate before the fuse completes.
    public void Detonate()
    {
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        CancelInvoke(nameof(Explode));
        StopAllCoroutines();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        HashSet<Rigidbody2D> affectedBodies = new HashSet<Rigidbody2D>();
        HashSet<ShortstuffRagdoll> affectedPlayers = new HashSet<ShortstuffRagdoll>();
        HashSet<Destructible> destroyedObjects = new HashSet<Destructible>();

        foreach (Collider2D hit in hits)
        {
            // WEEK 2: Preserve the destructible-object interaction.
            Destructible destructible = hit.GetComponentInParent<Destructible>();
            if (destructible != null && destroyedObjects.Add(destructible))
                destructible.DestroyObject();

            // WEEK 3C: The intact player becomes a ragdoll INSTEAD of
            // receiving the old rigidbody impulse (no double-launch).
            ShortstuffRagdoll ragdoll = hit.GetComponentInParent<ShortstuffRagdoll>();
            if (ragdoll != null)
            {
                if (affectedPlayers.Add(ragdoll))
                    ragdoll.ReceiveExplosion(transform.position, explosionForce);
                continue;
            }

            // Other dynamic bodies, including already-active ragdoll parts.
            Rigidbody2D targetRB = hit.attachedRigidbody;
            if (targetRB == null || targetRB.bodyType != RigidbodyType2D.Dynamic)
                continue;
            if (!targetRB.simulated || !affectedBodies.Add(targetRB))
                continue;

            Vector2 direction = targetRB.worldCenterOfMass - (Vector2)transform.position;
            direction = direction.sqrMagnitude < 0.001f ? Vector2.up : direction.normalized;
            targetRB.AddForce(direction * explosionForce, ForceMode2D.Impulse);
        }

        if (explosionSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, 0.8f);
        Destroy(gameObject);
    }

    private IEnumerator FlashFuse()
    {
        if (spriteRenderer == null) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < fuseTime)
        {
            float progress = elapsedTime / fuseTime;
            float flashInterval = slowFlashInterval;
            if (progress >= 0.9f) flashInterval = imminentFlashInterval;
            else if (progress >= 0.5f) flashInterval = fastFlashInterval;

            spriteRenderer.color = flashColor;
            if (audioSource != null && fuseBeep != null)
                audioSource.PlayOneShot(fuseBeep);
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval * 2f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
