<<<<<<< Updated upstream

=======
using System;
using System.Collections;
>>>>>>> Stashed changes
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{

    // INPUT ACTION REFERENCES
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference bombAction;
<<<<<<< Updated upstream

    // COMPONENT REFERENCES
=======
    [SerializeField] private InputActionReference detonateAction;
>>>>>>> Stashed changes
    private Rigidbody2D rb;

    // GROUND CHECKING
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;


    // UI
    [SerializeField] private TMP_Text bombCounterText;

<<<<<<< Updated upstream
=======
    // RAGDOLL REFERENCES
    [SerializeField] private ShortstuffRagdoll ragdollPrefab;
    private ShortstuffRagdoll activeRagdoll;
    private bool stunned;
    private Collider2D[] playerColliders;
    private SpriteRenderer characterSprite;

    // STUN SETTINGS
    [SerializeField] private float minimumStunTime = 1.5f;
    [SerializeField] private float maximumStunTime = 4f;
    [SerializeField] private float recoveryCheckRadius = 0.22f;
    [SerializeField] private float recoveryHeight = 0.15f;

    // COLIDER DIMMESNSIONS
    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;
>>>>>>> Stashed changes

    // BOMB REFERENCES

    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform bombSpawnPoint;


    // BOMB INVENTORY

    [Range(1, 10)]
    [SerializeField] private int maxBombs = 3;

    private int currentBombs;

<<<<<<< Updated upstream
=======
    // BOMB THROW SETTINGS
    [SerializeField] private float throwForce = 9f;

    [Range(0f, 90f)]
    [SerializeField] private float throwAngle = 40f;

    [SerializeField] private float throwSpin = 360f;

    private Bomb activeBomb;



>>>>>>> Stashed changes

    // PLAYER INPUT

    private float moveInput;
    private bool jumpRequested = false;
    private bool grounded;
    private bool isDead;


    public int FacingDirection { get; private set; } = 1;
    [SerializeField] private bool spriteFacesRight = true;

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

        // Find Ragdoll/Graphics Objects
        playerColliders = GetComponentsInChildren<Collider2D>();

        characterSprite = GetComponentInChildren<SpriteRenderer>();

        Collider2D mainCollider = GetComponent<Collider2D>();
        if (mainCollider != null)
        {
            standingColliderSize = mainCollider.bounds.size;

            standingColliderOffset = (Vector2)(mainCollider.bounds.center - transform.position);
        }
    }

    private void OnEnable()
    {
        moveAction.action.Enable();

        jumpAction.action.Enable();
        jumpAction.action.performed += OnJump;

        bombAction.action.Enable();
        bombAction.action.performed += OnBomb;

        detonateAction.action.Enable();
        detonateAction.action.performed += OnDetonate;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();

        jumpAction.action.performed -= OnJump;
        jumpAction.action.Disable();

        bombAction.action.performed -= OnBomb;
        bombAction.action.Disable();

        detonateAction.action.performed -= OnDetonate;
        detonateAction.action.Disable();
    }


    // INPUT

    private void Update()
    {
<<<<<<< Updated upstream
        if (isDead) return;
=======
        if (stunned) return;
>>>>>>> Stashed changes

        moveInput = moveAction.action.ReadValue<Vector2>().x;

        if (moveInput > 0.1f)
        {
            FacingDirection = 1;
        }
        else if (moveInput < -0.1f)
        {
            FacingDirection = -1;
        }

        if (characterSprite != null)
        {
            characterSprite.flipX = spriteFacesRight
                ? FacingDirection < 0
                : FacingDirection > 0;
        }
    }


    // MOVEMENT

    private void FixedUpdate()
    {
<<<<<<< Updated upstream
        if (isDead) return;
=======
        if (stunned) return;
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
        if (isDead) return;
=======
        if (stunned) return;
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
        if (isDead) return;

        PlaceBomb();
=======
        if (stunned) return;

        ThrowBomb();
>>>>>>> Stashed changes
    }

    private void ThrowBomb()
    {
        if (activeBomb != null)
        {
            return;
        }

        Debug.Log("Bombs Requested");

        if (currentBombs <= 0)
        {
            Debug.LogWarning("Out of Bombs! Please Collect More!");
            return;
        }

<<<<<<< Updated upstream
        Instantiate(
            bombPrefab,
            bombSpawnPoint.position,
            Quaternion.identity
        );
=======
        {
            
        }

        GameObject instance = Instantiate(bombPrefab, bombSpawnPoint.position, Quaternion.identity);

        float radians = throwAngle * Mathf.Deg2Rad;

        Vector2 direction = new Vector2(FacingDirection * Mathf.Cos(radians), Mathf.Sin(radians));

        Bomb bomb = instance.GetComponent<Bomb>();

        activeBomb = bomb;

        activeBomb.Exploded += OnBombExploded;

        if (bomb!= null)
        {
            bomb.Launch(direction * throwForce, -FacingDirection * throwSpin);
        }
>>>>>>> Stashed changes

        currentBombs--;

        SaveBombInventory();
        UpdateBombUI();

        Debug.Log("Bombs remaining: " + currentBombs);
    }

<<<<<<< Updated upstream

    // BOMB INVENTORY
=======
    private void OnDetonate(InputAction.CallbackContext context)
    {
        if (stunned) return;

        if (activeBomb != null)
        {
            activeBomb.Detonate();
        }
    }
>>>>>>> Stashed changes

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

    private void OnBombExploded(Bomb bomb)
    {
        if (activeBomb == bomb)
        {
            activeBomb.Exploded -= OnBombExploded;
            activeBomb = null;
        }
    }

    public void ReceiveExplosion(Vector2 explosionOrigin, Vector2 impulse)
    {
        // Ignore further explosions while already stunned
        if (stunned) return;

        stunned = true;

        // Remember our velocity before disabling physics
        Vector2 previousVelocity = rb.linearVelocity;

        Debug.Log("Player caught in explosion!");

        // Make the ragdoll
        activeRagdoll = Instantiate(ragdollPrefab, transform.position, Quaternion.identity);
        activeRagdoll.Activate(rb.linearVelocity, impulse, explosionOrigin);

        // Hide the normal character
        if (characterSprite != null)
        {
            characterSprite.enabled = false;
        }

        // Disable player colliders
        foreach (Collider2D col in playerColliders)
        {
            col.enabled = false;
        }

        // Stop normal player physics
        rb.simulated = false;

        // Clear pending movement
        moveInput = 0f;
        jumpRequested = false;

        StartCoroutine(RecoverWhenReady());
    }

    private IEnumerator RecoverWhenReady()
    {
        // Always remain stunned for at least this long
        yield return new WaitForSeconds(minimumStunTime);

        float elapsed = minimumStunTime;

        while (activeRagdoll != null)
        {
            Debug.Log("elapsed = " + elapsed);

            Vector2 torsoPosition = activeRagdoll.TorsoPosition;

            // Check for ground beneath the ragdoll
            bool nearGround = Physics2D.OverlapCircle(
                torsoPosition + Vector2.down * 0.55f,
                recoveryCheckRadius,
                groundLayer
            ) != null;

            // Check whether the ragdoll has slowed down
            bool slowEnough = activeRagdoll.TorsoSpeed < 2f;

            Debug.Log("Slow Enough = " + slowEnough);

            Debug.Log("Near Ground = " +  nearGround);

            // Decide whether we can recover
            if (nearGround && (slowEnough || elapsed >= maximumStunTime))
            {



                // Choose a position slightly above the ragdoll chest
                Vector2 candidate = torsoPosition + Vector2.up * recoveryHeight;

                // check whether the player would overlap solid terrain
                bool blocked = Physics2D.OverlapBox(candidate + standingColliderOffset, standingColliderSize * 0.5f, 0f, groundLayer) != null;

                Debug.Log("Blocked = " + blocked);

                // if not blocked then finish recovery
                if (!blocked)
                {
                    FinishRecovery(candidate);
                    yield break;
                }
                
            }

            elapsed += 0.1f;

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void FinishRecovery(Vector2 position)
    {
        // Remove the temporary ragdoll
        Destroy(activeRagdoll.gameObject);

        activeRagdoll = null;

        // Move the normal player to the recovery position
        transform.position = position;

        // Restore physics
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Restore colliders
        foreach (Collider2D col in playerColliders)
        {
            col.enabled = true;
        }

        // Restore the normal character artwork
        if (characterSprite != null)
        {
            characterSprite.enabled = true;
        }

        // Reset movement state
        currentSpeed = 0f;
        moveInput = 0f;
        jumpRequested = false;

        stunned = false;
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
        int objectLayer = other.gameObject.layer;

        int deathZoneLayer = LayerMask.NameToLayer("DeathZone");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int checkpointLayer = LayerMask.NameToLayer("Checkpoint");

        Debug.Log("Player entered trigger: " + other.name +
                  " | Layer: " + LayerMask.LayerToName(objectLayer));

        // DEATH ZONES AND ENEMIES

        if (objectLayer == deathZoneLayer || objectLayer == enemyLayer)
        {
            Debug.Log("Deadly trigger detected!");
            Die();
            return;
        }

        // CHECKPOINTS

        if (objectLayer == checkpointLayer)
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
