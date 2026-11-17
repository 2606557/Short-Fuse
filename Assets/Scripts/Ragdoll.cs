using UnityEngine;

public class Ragdoll : MonoBehaviour
{

    // REFERENCES
    [SerializeField] private Rigidbody2D torso;
    [SerializeField] private Rigidbody2D[] bodyParts;
    [SerializeField] private float blastSpin = 250f;

    public Vector2 TorsoPosition => torso.position;

    public float TorsoSpeed => torso.linearVelocity.magnitude;

    // SETUP
    public void Activate(Vector2 initialVelocity, Vector2 explosionImpulse, Vector2 explosionOrigin)
    {
        // transfer the player's existing velocity
        foreach (Rigidbody2D part in bodyParts)
        {
            if (part != null)
            {
                part.linearVelocity = initialVelocity;
            }
        }

        // apply the explosion force to the chest
        torso.AddForce(explosionImpulse, ForceMode2D.Impulse);
        float spinDirection = (torso.position.x >= explosionOrigin.x
            ? -1f
            : 1f);

        // rotate the chest
        torso.angularVelocity = spinDirection * blastSpin;
    }

}
