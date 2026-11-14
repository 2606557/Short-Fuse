using UnityEngine;

// WEEK 3B: Rigidbody2D + Collider2D live on the SAME bomb root.
[RequireComponent(typeof(Rigidbody2D))]
public class BombMotion : MonoBehaviour
{
    [SerializeField, Range(1f, 20f)] private float throwImpulse = 8f;
    [SerializeField, Range(0f, 8f)] private float spinImpulse = 1.5f;
    [SerializeField, Range(0.5f, 5f)] private float dropSpeed = 1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Throw(int facingDirection, Vector2 playerVelocity)
    {
        Vector2 direction = new Vector2(facingDirection, 0.85f).normalized;

        // Inherit a little player velocity without making throws unpredictable.
        rb.linearVelocity = playerVelocity * 0.25f;
        rb.AddForce(direction * throwImpulse, ForceMode2D.Impulse);
        rb.AddTorque(-facingDirection * spinImpulse, ForceMode2D.Impulse);
    }

    public void Drop()
    {
        // A vertical drop, including when Shortstuff is airborne.
        rb.linearVelocity = Vector2.down * dropSpeed;
        rb.angularVelocity = 0f;
    }
}
