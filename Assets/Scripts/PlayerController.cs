
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{

    // INPUT ACTION REFERENCES
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference bombAction;

    // COMPONENT REFERENCES
    private Rigidbody2D rb;

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


    // BOMB INVENTORY

    [Range(1, 10)]
    [SerializeField] private int maxBombs = 3;

    private int currentBombs;


    // PLAYER INPUT

    private float moveInput;
    private bool jumpRequested = false;
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


    // SETUP

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        currentBombs = maxBombs;

        if (GameManager.Instance != null)
        {
            currentBombs = GameManager.Instance.GetBombCount();
        }

        UpdateBombUI();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();

        jumpAction.action.Enable();
        jumpAction.action.performed += OnJump;

        bombAction.action.Enable();
        bombAction.action.performed += OnBomb;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();

        jumpAction.action.performed -= OnJump;
        jumpAction.action.Disable();

        bombAction.action.performed -= OnBomb;
        bombAction.action.Disable();
    }


    // INPUT

    private void Update()
    {
        if (isDead) return;

        moveInput = moveAction.action.ReadValue<Vector2>().x;
    }


    // MOVEMENT

    private void FixedUpdate()
    {
        if (isDead) return;

        CheckGround();
        HandleAcceleration();
        HandleMovement();
        HandleJump();
    }


    // HANDLE ACCELLERATION

    private void HandleAcceleration()
    {
        float targetSpeed = moveInput * moveSpeed;

        float currentAcceleration = CalculateAcceleration();

        currentSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            currentAcceleration * Time.fixedDeltaTime
        );
    }


    // HANDLE MOVEMENT

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(
            currentSpeed,
            rb.linearVelocity.y
        );
    }


    // HANDLE JUMPING

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isDead) return;

        jumpRequested = true;
    }

    private void HandleJump()
    {
        if (jumpRequested && grounded)
        {
            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );
        }

        jumpRequested = false;
    }


    // CHECK GROUNDING

    private void CheckGround()
    {
        grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;
    }

    public bool IsGrounded()
    {
        return grounded && !isDead;
    }


    // CALCULATE ACCELERATION

    private float CalculateAcceleration()
    {
        if (grounded)
        {
            return acceleration;
        }

        return acceleration * airControl;
    }


    // BOMBS

    private void OnBomb(InputAction.CallbackContext context)
    {
        if (isDead) return;

        PlaceBomb();
    }

    private void PlaceBomb()
    {
        Debug.Log("Bombs Requested");

        if (currentBombs <= 0)
        {
            Debug.LogWarning("Out of Bombs! Please Collect More!");
            return;
        }

        Instantiate(
            bombPrefab,
            bombSpawnPoint.position,
            Quaternion.identity
        );

        currentBombs--;

        SaveBombInventory();
        UpdateBombUI();

        Debug.Log("Bombs remaining: " + currentBombs);
    }


    // BOMB INVENTORY

    public void RefillBombs()
    {
        if (GameManager.Instance != null &&
            !GameManager.Instance.BombsUnlocked())
        {
            return;
        }

        currentBombs = maxBombs;

        SaveBombInventory();
        UpdateBombUI();
    }

    public void UnlockBombs()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockBombs();
        }

        currentBombs = maxBombs;

        SaveBombInventory();
        UpdateBombUI();
    }

    private void SaveBombInventory()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetBombCount(currentBombs);
        }
    }

    private void UpdateBombUI()
    {
        if (bombCounterText == null) return;

        bombCounterText.text = "BOMBS: " + currentBombs;
    }


    // PLAYER SPAWNING

    public void SpawnAt(Vector3 groundPosition)
    {
        // Calculate the distance between the player's origin and feet.
        Vector3 feetOffset = transform.position - groundCheck.position;

        // Position the player so their feet match the spawn position.
        transform.position = groundPosition + feetOffset;

        // Remove any movement carried over from the previous position.
        rb.linearVelocity = Vector2.zero;
    }


    // PLAYER TRIGGERS

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Log every trigger the player enters, regardless of its layer.
        Debug.Log("Player entered trigger: " + other.name +
                  " | Layer: " + LayerMask.LayerToName(other.gameObject.layer));

        int deadlyLayers = deathZoneLayer.value | enemyLayer.value;

        // Check for Death Zones and enemies.
        if ((deadlyLayers & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("Deadly trigger detected!");
            Die();
            return;
        }

        // Check for checkpoint triggers.
        if ((checkpointLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("Checkpoint layer detected: " + other.name);

            Checkpoint checkpoint = other.GetComponentInParent<Checkpoint>();

            if (checkpoint != null)
            {
                Debug.Log("Checkpoint parent found: " + checkpoint.name);
                checkpoint.ActivateCheckpoint(other);
            }
            else
            {
                Debug.LogWarning("Checkpoint trigger has no Checkpoint parent!");
            }
        }
    }


    // PLAYER DEATH

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("Shortstuff has died!");

        // Tell the GameManager to begin the respawn process.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
        else
        {
            Debug.LogError("No GameManager found!");
        }

        // Remove the player immediately.
        Destroy(gameObject);
    }


    // GIZMOS AND DEBUGS

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
