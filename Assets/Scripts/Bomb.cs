using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bomb : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Rigidbody2D rb;

    [Range(0.5f, 10f)] [SerializeField] private float fuseTime = 4f;
    [Range(0.5f, 10f)] [SerializeField] private float explosionRadius = 3f;
    [Range(1f, 30f)] [SerializeField] private float explosionForce = 12f;
    [SerializeField] private AudioClip fuseBeep;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private Color flashColor = Color.white;
    [Range(0.05f, 1f)] [SerializeField] private float slowFlashInterval = 0.4f;
    [Range(0.05f, 1f)] [SerializeField] private float fastFlashInterval = 0.15f;
    [Range(0.01f, 0.5f)] [SerializeField] private float imminentFlashInterval = 0.05f;

    private Color originalColor = Color.white;
    private bool hasExploded;
    public event Action<Bomb> Exploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        StartCoroutine(FlashFuse());
        Invoke(nameof(Detonate), fuseTime);
    }

    // One fixed impulse from the player; the Rigidbody2D handles the trajectory.
    public void Launch(Vector2 impulse, float spinDegreesPerSecond)
    {
        rb.AddForce(impulse, ForceMode2D.Impulse);
        rb.angularVelocity = spinDegreesPerSecond;
    }

    public void Detonate()
    {
        if (hasExploded) return;
        hasExploded = true;
        CancelInvoke(nameof(Detonate));
        StopAllCoroutines();
        Exploded?.Invoke(this);
        Explode();
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        HashSet<Rigidbody2D> processedBodies = new HashSet<Rigidbody2D>();
        HashSet<Destructible> processedDestructibles = new HashSet<Destructible>();
        HashSet<PlayerController> processedPlayers = new HashSet<PlayerController>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            Destructible destructible = hit.GetComponentInParent<Destructible>();
            if (destructible != null && processedDestructibles.Add(destructible))
                destructible.DestroyObject();

            PlayerController player = hit.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                if (processedPlayers.Add(player) && !player.IsStunned)
                {
                    Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
                    Vector2 centre = playerBody != null ? playerBody.worldCenterOfMass : (Vector2)player.transform.position;
                    Vector2 direction = centre - (Vector2)transform.position;
                    if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
                    player.ReceiveExplosion(transform.position, direction.normalized * explosionForce);
                }
                continue;
            }

            Rigidbody2D target = hit.attachedRigidbody;
            if (target == null || target == rb || target.bodyType != RigidbodyType2D.Dynamic)
                continue;
            if (!processedBodies.Add(target)) continue;

            Vector2 forceDirection = target.worldCenterOfMass - (Vector2)transform.position;
            if (forceDirection.sqrMagnitude < 0.0001f) forceDirection = Vector2.up;
            target.AddForce(forceDirection.normalized * explosionForce, ForceMode2D.Impulse);
        }

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, 0.8f);
        Destroy(gameObject);
    }

    private IEnumerator FlashFuse()
    {
        float elapsed = 0f;
        while (elapsed < fuseTime && !hasExploded)
        {
            float progress = elapsed / fuseTime;
            float interval = progress >= 0.9f ? imminentFlashInterval :
                             progress >= 0.5f ? fastFlashInterval : slowFlashInterval;
            if (spriteRenderer != null) spriteRenderer.color = flashColor;
            if (audioSource != null && fuseBeep != null) audioSource.PlayOneShot(fuseBeep);
            yield return new WaitForSeconds(interval);
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(interval);
            elapsed += interval * 2f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
