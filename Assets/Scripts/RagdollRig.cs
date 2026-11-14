using UnityEngine;

// WEEK 3C: Put this on the ROOT of the ragdoll prefab (no Rigidbody2D on root).
// The Rigidbody2D parts are CHILDREN joined by HingeJoint2D components.
public class RagdollRig : MonoBehaviour
{
    [SerializeField] private Rigidbody2D torso;
    [SerializeField] private Rigidbody2D[] parts;
    [SerializeField, Range(0f, 8f)] private float tumbleImpulse = 1.5f;

    public Vector2 TorsoPosition => torso.position;
    public Vector2 TorsoVelocity => torso.linearVelocity;

    public void SetStartingVelocity(Vector2 velocity)
    {
        foreach (Rigidbody2D part in parts)
        {
            part.linearVelocity = velocity;
            part.angularVelocity = 0f;
        }
    }

    public void ApplyExplosion(Vector2 explosionOrigin, float impulse)
    {
        Vector2 direction = torso.worldCenterOfMass - explosionOrigin;
        direction = direction.sqrMagnitude < 0.001f ? Vector2.up : direction.normalized;

        // Dividing the total impulse by mass gives the WHOLE ragdoll a
        // coherent launch before its joints start flopping around.
        float totalMass = 0f;
        foreach (Rigidbody2D part in parts) totalMass += part.mass;
        if (totalMass <= 0f) return;

        foreach (Rigidbody2D part in parts)
        {
            float share = part.mass / totalMass;
            part.AddForce(direction * impulse * share, ForceMode2D.Impulse);
        }

        torso.AddTorque(-Mathf.Sign(direction.x == 0f ? 1f : direction.x)
                        * tumbleImpulse, ForceMode2D.Impulse);
    }

    public bool IsSettled(LayerMask groundLayer)
    {
        bool touchingGround = false;

        foreach (Rigidbody2D part in parts)
        {
            if (part.linearVelocity.magnitude > 1.5f ||
                Mathf.Abs(part.angularVelocity) > 180f)
                return false;

            Collider2D[] colliders = part.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                if (collider.IsTouchingLayers(groundLayer))
                    touchingGround = true;
            }
        }

        return touchingGround;
    }
}
