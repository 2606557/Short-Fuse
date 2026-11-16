using System;
using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{

    // REFERENCES
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Rigidbody2D rb;

    // BOMB SETTINGS

    [Range(0.5f, 10f)]
    [SerializeField] private float fuseTime = 4f;

    [Range(0.5f, 10f)]
    [SerializeField] private float explosionRadius = 3f;

    [Range(1f, 30f)]
    [SerializeField] private float explosionForce = 12f;

    [SerializeField] private AudioClip fuseBeep;
    [SerializeField] private AudioClip explosionSound;

    private bool hasExploded;

    public event Action<Bomb> Exploded;

    // BOMB FLASH SETTINGS

    [SerializeField] private Color flashColor = Color.white;
    private Color originalColor;

    [Range(0.05f, 1f)]
    [SerializeField] private float slowFlashInterval = 0.4f;

    [Range(0.05f, 1f)]
    [SerializeField] private float fastFlashInterval = 0.15f;

    [Range(0.01f, 0.5f)]
    [SerializeField] private float imminentFlashInterval = 0.05f;


    // SETUP

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();

        originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        StartCoroutine(FlashFuse());// Start flashing on a loop

        Invoke(nameof(Detonate), fuseTime); // Explode after (fuseTime) seconds

    }

    // LAUNCH
    public void Launch(Vector2 impulse, float spinDegreesPerSecond)
    {
        rb.AddForce(impulse, ForceMode2D.Impulse);
        rb.angularVelocity = spinDegreesPerSecond;
    }

    // DETONATE
    public void Detonate()
    {
        if (hasExploded) return;

        hasExploded = true;
        CancelInvoke(nameof(Detonate));
        StopAllCoroutines();
        Explode();
    }


    // EXPLOSION

    private void Explode()
    {
        Exploded?.Invoke(this);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            // Destroy any objects that are "Desctructible"
            Destructible destructible = hit.GetComponentInParent<Destructible>(); // Success - assign the desctible script to this variable OR Fail - it wont find anything

            if (destructible != null)
            {
                destructible.DestroyObject();
            }

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
            Vector2 direction = (targetRB.worldCenterOfMass - (Vector2)transform.position);

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.up;
            }

            Vector2 impulse = direction.normalized * explosionForce;

            // Check whether this RB2D belongs to the player

            PlayerController player = targetRB.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ReceiveExplosion(transform.position, impulse);
            }

            else

                targetRB.AddForce(impulse, ForceMode2D.Impulse);

            Debug.Log("Explosion hit: " + hit.name);
        }

        AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, 0.8f); 
        Destroy(gameObject);


    }

    private IEnumerator FlashFuse()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fuseTime)
        {
            float progress = elapsedTime / fuseTime; // Create a percentage based on time since bomb started
            Debug.Log("progress is: " + progress);

            float flashInterval = slowFlashInterval;
            Debug.Log("flash interval is: " + flashInterval);

            if (progress >= 0.9f)
            {
                flashInterval = imminentFlashInterval;
            }

            else if (progress >= 0.5f)
            {
                flashInterval = fastFlashInterval;
            }

            // Loop the colours
            spriteRenderer.color = flashColor;
            audioSource.PlayOneShot(fuseBeep);
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);

            // Increment the timer
            elapsedTime += flashInterval * 2f;
        }

    }




    // DEBUG AND GIZMOS

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}