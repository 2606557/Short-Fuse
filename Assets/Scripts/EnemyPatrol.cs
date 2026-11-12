
using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    // REFERENCES

    [SerializeField] private Transform enemy;
    [SerializeField] private Transform patrolPivot;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    private Rigidbody2D rb;


    // PATROL SETTINGS

    [Range(0.5f, 5f)]
    [SerializeField] private float moveSpeed = 2f;

    [Range(0f, 3f)]
    [SerializeField] private float waitTime = 1f;

    [SerializeField] private bool startOnNearestPoint = true;

    private Vector2 destination;
    private bool headingToB = true;


    // SETUP

    private void Awake()
    {
        if (enemy != null)
        {
            rb = enemy.GetComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        if (enemy == null || patrolPivot == null ||
            pointA == null || pointB == null || rb == null)
        {
            Debug.LogError("Patrolling Enemy is missing references!", this);
            return;
        }

        SetStartingPosition();
        SetDestination();
    }


    // STARTING POSITION

    private void SetStartingPosition()
    {
        if (!startOnNearestPoint)
        {
            return;
        }

        float distanceA = Vector2.Distance(patrolPivot.position, pointA.position);
        float distanceB = Vector2.Distance(patrolPivot.position, pointB.position);

        Vector2 nearestPoint = distanceA <= distanceB
            ? (Vector2)pointA.position
            : (Vector2)pointB.position;

        headingToB = distanceA <= distanceB;

        Vector2 offset = (Vector2)patrolPivot.position - rb.position;

        rb.position = nearestPoint - offset;
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
        while (Vector2.Distance(patrolPivot.position, destination) > 0.01f)
        {
            Vector2 nextOrigin = Vector2.MoveTowards(
                patrolPivot.position,
                destination,
                moveSpeed * Time.fixedDeltaTime
            );

            Vector2 movement = nextOrigin - (Vector2)patrolPivot.position;

            rb.MovePosition(rb.position + movement);

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


    // GIZMOS

    private void OnDrawGizmos()
    {
        if (pointA == null || pointB == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(pointA.position, pointB.position);

        Gizmos.DrawWireSphere(pointA.position, 0.2f);
        Gizmos.DrawWireSphere(pointB.position, 0.2f);

        if (patrolPivot != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(patrolPivot.position, 0.12f);
        }
    }
}
