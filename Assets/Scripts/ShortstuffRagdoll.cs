using System.Collections;
using UnityEngine;

// WEEK 3C: Goes on the NORMAL player root, alongside PlayerController.
// The ragdoll prefab is a separate object, never a child of the player.
public class ShortstuffRagdoll : MonoBehaviour
{
    [Header("Normal Player")]
    [SerializeField] private PlayerController controller;
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private SpriteRenderer normalSprite;

    [Header("Ragdoll")]
    [SerializeField] private RagdollRig ragdollPrefab;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField, Range(0.5f, 4f)] private float minimumStunTime = 1.5f;
    [SerializeField, Range(2f, 10f)] private float maximumStunTime = 5f;
    [SerializeField, Range(0f, 1f)] private float recoveryLift = 0.25f;

    private RagdollRig activeRig;
    private bool knockedOut;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    public bool IsKnockedOut => knockedOut;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (playerBody == null) playerBody = GetComponent<Rigidbody2D>();
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();

        // Record the NORMAL player's outline BEFORE switching physics off.
        Bounds bounds = playerCollider.bounds;
        originalColliderSize = bounds.size;
        originalColliderOffset = (Vector2)bounds.center - playerBody.position;
    }

    public void ReceiveExplosion(Vector2 origin, float impulse)
    {
        if (knockedOut || ragdollPrefab == null) return;

        knockedOut = true;
        Vector2 oldVelocity = playerBody.linearVelocity;

        activeRig = Instantiate(ragdollPrefab, playerBody.position, Quaternion.identity);
        activeRig.SetStartingVelocity(oldVelocity);

        // Make the real player disappear from BOTH rendering and physics.
        controller.ResetForRagdoll();
        controller.enabled = false;
        normalSprite.enabled = false;
        playerBody.simulated = false;

        activeRig.ApplyExplosion(origin, impulse);
        StartCoroutine(RecoverAfterExplosion());
    }

    private IEnumerator RecoverAfterExplosion()
    {
        float elapsed = 0f;

        while (elapsed < maximumStunTime)
        {
            yield return new WaitForFixedUpdate();
            elapsed += Time.fixedDeltaTime;

            if (elapsed >= minimumStunTime && activeRig.IsSettled(groundLayer))
                break;
        }

        if (activeRig == null) yield break;

        Vector2 candidate = activeRig.TorsoPosition + Vector2.up * recoveryLift;

        // Move upward in small increments to avoid recreating the player's
        // collider inside the ground or a solid obstacle.
        for (int attempt = 0; attempt < 16; attempt++)
        {
            bool blocked = Physics2D.OverlapBox(
                candidate + originalColliderOffset,
                originalColliderSize * 0.9f,
                0f,
                groundLayer
            ) != null;

            if (!blocked) break;
            candidate += Vector2.up * 0.1f;
        }

        Vector2 inheritedVelocity = activeRig.TorsoVelocity * 0.25f;
        Destroy(activeRig.gameObject);
        activeRig = null;

        playerBody.position = candidate;
        playerBody.rotation = 0f;
        playerBody.linearVelocity = inheritedVelocity;
        playerBody.angularVelocity = 0f;
        playerBody.simulated = true;

        normalSprite.enabled = true;
        knockedOut = false;
        controller.enabled = true;
    }

    private void OnDestroy()
    {
        // Critical if a checkpoint/death handler destroys the real player.
        if (activeRig != null) Destroy(activeRig.gameObject);
    }
}
