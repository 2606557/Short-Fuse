using UnityEngine;
using UnityEngine.InputSystem;

// WEEK 3A: This reads the SAME Move action as PlayerController.
// It does not enable/disable the action: PlayerController owns that.
public class ShortstuffPresentation : MonoBehaviour
{
    [Header("Input / Art")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private Sprite standingSprite;
    [SerializeField] private Sprite crouchingSprite;

    [Header("Tuning")]
    [SerializeField, Range(0.1f, 1f)] private float deadZone = 0.25f;
    [SerializeField, Range(0.1f, 1f)] private float crouchThreshold = 0.5f;

    // These values are also read by PlayerController in Week 3B.
    public int FacingDirection { get; private set; } = 1;
    public bool IsCrouching { get; private set; }

    private void Update()
    {
        if (moveAction == null || characterSprite == null) return;

        Vector2 movement = moveAction.action.ReadValue<Vector2>();

        if (movement.x > deadZone) FacingDirection = 1;
        else if (movement.x < -deadZone) FacingDirection = -1;

        IsCrouching = movement.y < -crouchThreshold;

        // Source sprites should be drawn facing RIGHT.
        characterSprite.flipX = FacingDirection < 0;

        if (standingSprite != null && crouchingSprite != null)
            characterSprite.sprite = IsCrouching ? crouchingSprite : standingSprite;
    }
}
