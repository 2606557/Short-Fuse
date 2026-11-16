using UnityEngine;

public class ShortstuffRagdoll : MonoBehaviour
{
    // References
    [SerializeField] private Rigidbody2D torso;
    [SerializeField] private Rigidbody2D[] bodyParts;

    [SerializeField] private float blastSpin = 250f;

    // Public variables
    public Vector2 TorsoPosition => torso.position;
    public float TorsoSpeed => torso.linearVelocity.magnitude;

    private void Start()
    {
        // Activate(Vector2.zero, new Vector2(-25f, 50f), torso.transform.position + new Vector3(50f, 10f, 0f));
    }

    public void Activate(Vector2 initialVelocity, Vector2 explosionImpulse, Vector2 explosionOrigin)
    {
        // Transfer the player's initial velocity
        foreach (Rigidbody2D part in bodyParts)
        {
            if (part != null)
            {
                part.linearVelocity = initialVelocity;
            }
        }

        // Apply the explosion force to the chest
        torso.AddForce(explosionImpulse, ForceMode2D.Impulse);

        // Chose a spin direction based on the explosion position
        float spinDirection = torso.position.x >= explosionOrigin.x
            ? -1f
            : 1f;

        // Rotate the chest
        torso.angularVelocity = spinDirection * blastSpin;

    }

}
