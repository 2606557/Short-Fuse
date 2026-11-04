using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // REFERENCES

    [SerializeField] private InputActionReference moveAction;
    private Rigidbody2D rb;

    // PLAYER INPUT

    private float moveInput;

    // MOVEMENT SETTINGS
    [Range(1f, 12f)]
    [SerializeField] private float moveSpeed = 6f;

    [Range(1f, 100f)]
    [SerializeField] private float acceleration = 40f;

    private float currentSpeed;

    // SETUP

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }


    // INPUT
    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>().x;
    }

    // MOVEMENT
    private void FixedUpdate()
    {
        
        HandleAcceleration();
        HandleMovement();
    }

    // HANDLE ACCELLERATION
    private void HandleAcceleration()
    {
        // Set "Target speed" (Ideal speed if it were in vacuum)
        float targetSpeed = moveInput * moveSpeed;

        // Calculate currentspeed based on target speed but with acceleration damnpening)
        currentSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
    }


    // HANDLE MOVEMENT
    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }
}
