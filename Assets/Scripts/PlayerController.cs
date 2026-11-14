using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// WEEK 3B/C REFERENCE VERSION based on the existing Short Fuse controller.
// Keep exactly ONE PlayerController.cs in Assets.
public class PlayerController : MonoBehaviour
{
    // INPUT ACTION REFERENCES
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference bombAction;
    [SerializeField] private InputActionReference detonateAction; // WEEK 3B

    // COMPONENT REFERENCES
    private Rigidbody2D rb;
    [SerializeField] private ShortstuffPresentation presentation; // WEEK 3A/B

    // LAYERS
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask deathZoneLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask checkpointLayer;

    // UI
    [SerializeField] private TMP_Text bombCounterText;

    // BOMB REFERENCES
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform bombSpawnPoint;
    [SerializeField, Range(0f, 1f)] private float throwSpawnOffset = 0.4f;
    private Bomb activeBomb; // Only ONE bomb active at any time

    // BOMB INVENTORY
    [Range(1, 10)]
    [SerializeField] private int maxBombs = 3;
    private int currentBombs;

    // PLAYER INPUT
    private float moveInput;
    private bool jumpRequested;
    private bool grounded;
    private bool isDead;

    // MOVEMENT SETTINGS
    [Range(1f, 12f)]
    [SerializeField] private float moveSpeed = 6f;
    [Range(1f, 100f)]
    [SerializeField] private float acceleration = 40f;
    private float currentSpeed;
    [Range(1f, 20f)]
    [SerializeField] private float jumpForce = 10f;
    [Range(0.05f, 0.5f)]
    [SerializeField] private float groundCheckRadius = 0.15f;
    [Range(0f, 1f)]
    [SerializeField] private float airControl = 0.3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (presentation == null) presentation = GetComponent<ShortstuffPresentation>();

        currentBombs = maxBombs;
        if (GameManager.Instance != null)
            currentBombs = GameManager.Instance.GetBombCount();

        UpdateBombUI();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        jumpAction.action.performed += OnJump;
        bombAction.action.Enable();
        bombAction.action.performed += OnBomb;
        if (detonateAction != null)
        {
            detonateAction.action.Enable();
            detonateAction.action.performed += OnDetonate;
        }
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.performed -= OnJump;
        jumpAction.action.Disable();
        bombAction.action.performed -= OnBomb;
        bombAction.action.Disable();
        if (detonateAction != null)
        {
            detonateAction.action.performed -= OnDetonate;
            detonateAction.action.Disable();
        }
    }

    private void Update()
    {
        if (isDead) return;
        Vector2 movement = moveAction.action.ReadValue<Vector2>();
        moveInput = movement.y < -0.5f ? 0f : movement.x;
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        CheckGround();
        HandleAcceleration();
        HandleMovement();
        HandleJump();
    }

    private void HandleAcceleration()
    {
        float targetSpeed = moveInput * moveSpeed;
        float currentAcceleration = CalculateAcceleration();
        currentSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x, targetSpeed,
            currentAcceleration * Time.fixedDeltaTime);
    }

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isDead) return;
        jumpRequested = true;
    }

    private void HandleJump()
    {
        if (jumpRequested && grounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpRequested = false;
    }

    private void CheckGround()
    {
        grounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    public bool IsGrounded() => grounded && !isDead;

    private float CalculateAcceleration()
    {
        return grounded ? acceleration : acceleration * airControl;
    }

    private void OnBomb(InputAction.CallbackContext context)
    {
        if (isDead) return;
        PlaceBomb();
    }

    private void OnDetonate(InputAction.CallbackContext context)
    {
        if (isDead) return;
        if (activeBomb != null) activeBomb.Detonate();
    }

    private void PlaceBomb()
    {
        if (activeBomb != null)
        {
            Debug.Log("Wait for the active bomb to explode (or detonate it).");
            return;
        }
        if (currentBombs <= 0)
        {
            Debug.LogWarning("Out of Bombs! Please Collect More!");
            return;
        }

        bool crouching = presentation != null && presentation.IsCrouching;
        int facing = presentation != null ? presentation.FacingDirection : 1;

        Vector3 spawnPosition = bombSpawnPoint.position;
        if (!crouching) spawnPosition += Vector3.right * facing * throwSpawnOffset;

        GameObject instance = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        Bomb bomb = instance.GetComponent<Bomb>();
        BombMotion motion = instance.GetComponent<BombMotion>();
        if (bomb == null || motion == null)
        {
            Debug.LogError("Bomb prefab needs both Bomb and BombMotion components.");
            Destroy(instance);
            return;
        }

        if (crouching) motion.Drop();
        else motion.Throw(facing, rb.linearVelocity);

        activeBomb = bomb;
        currentBombs--;
        SaveBombInventory();
        UpdateBombUI();
    }

    public void ResetForRagdoll()
    {
        // Prevent a buffered jump or stale horizontal speed after recovery.
        jumpRequested = false;
        moveInput = 0f;
        currentSpeed = 0f;
    }

    public void RefillBombs()
    {
        if (GameManager.Instance != null && !GameManager.Instance.BombsUnlocked())
            return;
        currentBombs = maxBombs;
        SaveBombInventory();
        UpdateBombUI();
    }

    public void UnlockBombs()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnlockBombs();
        currentBombs = maxBombs;
        SaveBombInventory();
        UpdateBombUI();
    }

    private void SaveBombInventory()
    {
        if (GameManager.Instance != null) GameManager.Instance.SetBombCount(currentBombs);
    }

    private void UpdateBombUI()
    {
        if (bombCounterText != null) bombCounterText.text = "BOMBS: " + currentBombs;
    }

    public void SpawnAt(Vector3 groundPosition)
    {
        Vector3 feetOffset = transform.position - groundCheck.position;
        transform.position = groundPosition + feetOffset;
        rb.linearVelocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int deadlyLayers = deathZoneLayer.value | enemyLayer.value;
        if ((deadlyLayers & (1 << other.gameObject.layer)) != 0)
        {
            Die();
            return;
        }
        if ((checkpointLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Checkpoint checkpoint = other.GetComponentInParent<Checkpoint>();
            if (checkpoint != null) checkpoint.ActivateCheckpoint(other);
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
        else Debug.LogError("No GameManager found!");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
