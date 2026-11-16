using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // INPUT ACTIONS (assign the same Move, Jump and Bomb actions as Week 2)
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference bombAction;
    [SerializeField] private InputActionReference detonateAction; // NEW: create an action

    // EXISTING REFERENCES
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TMP_Text bombCounterText;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform bombSpawnPoint;

    // VISUALS
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private bool spriteFacesRight = true;

    // BOMBS
    [Range(1, 10)] [SerializeField] private int maxBombs = 3;
    [SerializeField] private float throwForce = 9f;
    [Range(0f, 90f)] [SerializeField] private float throwAngle = 40f;
    [SerializeField] private float throwSpin = 180f; // degrees/second, applied via angular velocity
    private int currentBombs;
    private Bomb activeBomb;

    // RAGDOLL
    [SerializeField] private ShortstuffRagdoll ragdollPrefab;
    [SerializeField] private float minimumStunTime = 1.5f;
    [SerializeField] private float maximumStunTime = 4f;
    [SerializeField] private float recoveryCheckRadius = 0.22f;
    [SerializeField] private float recoveryHeight = 0.15f;
    private ShortstuffRagdoll activeRagdoll;
    private Coroutine recoveryRoutine;
    private bool stunned;

    // MOVEMENT (original Week 2 values)
    [Range(1f, 12f)] [SerializeField] private float moveSpeed = 6f;
    [Range(1f, 100f)] [SerializeField] private float acceleration = 40f;
    [Range(1f, 20f)] [SerializeField] private float jumpForce = 10f;
    [Range(0.05f, 0.5f)] [SerializeField] private float groundCheckRadius = 0.15f;
    [Range(0f, 1f)] [SerializeField] private float airControl = 0.3f;

    private Rigidbody2D rb;
    private Collider2D[] playerColliders;
    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;
    private float moveInput;
    private bool jumpRequested;
    private bool grounded;
    private float currentSpeed;

    public int FacingDirection { get; private set; } = 1;
    public bool IsStunned => stunned;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerColliders = GetComponentsInChildren<Collider2D>();
        Collider2D mainCollider = GetComponent<Collider2D>();
        if (mainCollider != null)
        {
            standingColliderSize = mainCollider.bounds.size;
            standingColliderOffset = (Vector2)(mainCollider.bounds.center - transform.position);
        }
        else standingColliderSize = new Vector2(0.6f, 0.9f);
        currentBombs = maxBombs;
        UpdateBombUI();
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump;
        }
        if (bombAction != null)
        {
            bombAction.action.Enable();
            bombAction.action.performed += OnBomb;
        }
        if (detonateAction != null)
        {
            detonateAction.action.Enable();
            detonateAction.action.performed += OnDetonate;
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
            jumpAction.action.Disable();
        }
        if (bombAction != null)
        {
            bombAction.action.performed -= OnBomb;
            bombAction.action.Disable();
        }
        if (detonateAction != null)
        {
            detonateAction.action.performed -= OnDetonate;
            detonateAction.action.Disable();
        }
        if (moveAction != null) moveAction.action.Disable();
    }

    private void Update()
    {
        if (stunned) return;

        moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>().x : 0f;
        if (moveInput > 0.1f) FacingDirection = 1;
        else if (moveInput < -0.1f) FacingDirection = -1;

        if (characterSprite != null)
            characterSprite.flipX = spriteFacesRight ? FacingDirection < 0 : FacingDirection > 0;
    }

    private void FixedUpdate()
    {
        if (stunned) return;
        CheckGround();
        HandleAcceleration();
        HandleMovement();
        HandleJump();
    }

    private void CheckGround()
    {
        grounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void HandleAcceleration()
    {
        float targetSpeed = moveInput * moveSpeed;
        float rate = grounded ? acceleration : acceleration * airControl;
        currentSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);
    }

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!stunned) jumpRequested = true;
    }

    private void HandleJump()
    {
        if (jumpRequested && grounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpRequested = false;
    }

    private void OnBomb(InputAction.CallbackContext context)
    {
        if (!stunned) ThrowBomb();
    }

    private void ThrowBomb()
    {
        // One active bomb at a time; inventory remains on this controller.
        if (currentBombs <= 0 || activeBomb != null || bombPrefab == null || bombSpawnPoint == null)
            return;

        GameObject instance = Instantiate(bombPrefab, bombSpawnPoint.position, Quaternion.identity);
        Bomb bomb = instance.GetComponent<Bomb>();
        if (bomb == null)
        {
            Debug.LogError("Bomb prefab needs the Bomb component.");
            Destroy(instance);
            return;
        }

        activeBomb = bomb;
        bomb.Exploded += OnBombExploded;
        currentBombs--;
        UpdateBombUI();

        float radians = throwAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(FacingDirection * Mathf.Cos(radians), Mathf.Sin(radians));
        bomb.Launch(direction * throwForce, -FacingDirection * throwSpin);
    }

    private void OnDetonate(InputAction.CallbackContext context)
    {
        if (!stunned && activeBomb != null)
            activeBomb.Detonate();
    }

    private void OnBombExploded(Bomb bomb)
    {
        if (activeBomb == bomb)
        {
            activeBomb.Exploded -= OnBombExploded;
            activeBomb = null;
        }
    }

    public void RefillBombs()
    {
        currentBombs = maxBombs;
        UpdateBombUI();
    }

    private void UpdateBombUI()
    {
        if (bombCounterText != null)
            bombCounterText.text = "BOMBS: " + currentBombs;
    }

    // Called by Bomb instead of applying an impulse to the normal player body.
    public void ReceiveExplosion(Vector2 explosionOrigin, Vector2 impulse)
    {
        if (stunned) return;
        if (ragdollPrefab == null)
        {
            // Allows the rest of the new bomb system to work before the rig is built.
            rb.AddForce(impulse, ForceMode2D.Impulse);
            return;
        }

        stunned = true;
        moveInput = 0f;
        jumpRequested = false;
        Vector2 previousVelocity = rb.linearVelocity;

        activeRagdoll = Instantiate(ragdollPrefab, transform.position, Quaternion.identity);
        activeRagdoll.Activate(previousVelocity, impulse, explosionOrigin);

        if (characterSprite != null) characterSprite.enabled = false;
        foreach (Collider2D col in playerColliders)
            if (col != null) col.enabled = false;
        rb.simulated = false;

        recoveryRoutine = StartCoroutine(RecoverWhenReady());
    }

    private IEnumerator RecoverWhenReady()
    {
        yield return new WaitForSeconds(minimumStunTime);
        float elapsed = minimumStunTime;

        while (activeRagdoll != null)
        {
            Vector2 torso = activeRagdoll.TorsoPosition;
            // Wait until the ragdoll is near solid ground; don't recover mid-flight.
            bool nearGround = Physics2D.OverlapCircle(
                torso + Vector2.down * 0.55f, recoveryCheckRadius, groundLayer) != null;

            bool slowEnough = activeRagdoll.TorsoSpeed < 2f;
            if (nearGround && (slowEnough || elapsed >= maximumStunTime))
            {
                Vector2 candidate = torso + Vector2.up * recoveryHeight;
                // Check clearance using the player's main collider bounds.
                bool blocked = Physics2D.OverlapBox(candidate + standingColliderOffset,
                    standingColliderSize * 0.85f, 0f, groundLayer) != null;

                if (!blocked)
                {
                    FinishRecovery(candidate);
                    yield break;
                }
            }

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        recoveryRoutine = null;
    }

    private void FinishRecovery(Vector2 position)
    {
        Destroy(activeRagdoll.gameObject);
        activeRagdoll = null;
        transform.position = position;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        foreach (Collider2D col in playerColliders)
            if (col != null) col.enabled = true;
        if (characterSprite != null) characterSprite.enabled = true;
        currentSpeed = 0f;
        stunned = false;
        recoveryRoutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
