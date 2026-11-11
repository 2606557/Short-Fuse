
using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    // REFERENCES

    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    private Rigidbody2D rb;


    // PATROL SETTINGS

    [Range(0.5f, 5f)]
    [SerializeField] private float moveSpeed = 2f;

    [Range(0f, 3f)]
    [SerializeField] private float waitTime = 1f;

    private Vector2 destination;
    private bool headingToB = true;


    // SETUP

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SetDestination();
    }


    // SET DESTINATION

    private void SetDestination()
    {
        destination = headingToB ? pointB.position : pointA.position;

        headingToB = !headingToB;

        BeginJourney();
    }


    // BEGIN JOURNEY

    private void BeginJourney()
    {
        StartCoroutine(TravelToDestination());
    }


    // TRAVEL TO DESTINATION

    private IEnumerator TravelToDestination()
    {
        while (Vector2.Distance(rb.position, destination) > 0.01f)
        {
            Vector2 nextPosition = Vector2.MoveTowards(
                rb.position,
                destination,
                moveSpeed * Time.fixedDeltaTime
            );

            rb.MovePosition(nextPosition);

            yield return new WaitForFixedUpdate();
        }

        WaitAtDestination();
    }


    // WAIT AT DESTINATION

    private void WaitAtDestination()
    {
        StartCoroutine(WaitBeforeMoving());
    }

    private IEnumerator WaitBeforeMoving()
    {
        yield return new WaitForSeconds(waitTime);

        SetDestination();
    }


    // PLAYER COLLISION

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
        {
            return;
        }

        player.Die();
    }
}
