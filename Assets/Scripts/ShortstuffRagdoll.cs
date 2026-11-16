using UnityEngine;

// Attach to the ROOT of a ragdoll prefab. The root has no Rigidbody2D.
// Child parts (torso, head, legs) have Rigidbody2D + Collider2D and are joined with HingeJoint2D.
public class ShortstuffRagdoll : MonoBehaviour
{
    [SerializeField] private Rigidbody2D torso;
    [SerializeField] private Rigidbody2D[] bodyParts;
    [SerializeField] private float blastSpin = 250f;

    public Vector2 TorsoPosition => torso != null ? torso.position : (Vector2)transform.position;
    public float TorsoSpeed => torso != null ? torso.linearVelocity.magnitude : 0f;

    public void Activate(Vector2 initialVelocity, Vector2 explosionImpulse, Vector2 explosionOrigin)
    {
        if (torso == null)
        {
            Debug.LogError("Ragdoll prefab: assign Torso.");
            return;
        }

        foreach (Rigidbody2D part in bodyParts)
        {
            if (part == null) continue;
            part.linearVelocity = initialVelocity;
        }

        // Impulse on the torso, and spin on the connected parts.
        torso.AddForce(explosionImpulse, ForceMode2D.Impulse);
        float spinSign = torso.position.x >= explosionOrigin.x ? -1f : 1f;
        torso.angularVelocity = spinSign * blastSpin;
    }
}
