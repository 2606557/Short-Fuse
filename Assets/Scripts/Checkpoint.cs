using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // REFERENCES

    private BoxCollider2D[] checkpointColliders;

    [SerializeField] private LayerMask groundLayer;


    // GROUND DETECTION

    [Range(0.5f, 10f)]
    [SerializeField] private float groundSearchDistance = 5f;


    // SETUP

    private void Awake()
    {
        // Find every BoxCollider2D on this object and its children.
        checkpointColliders = GetComponentsInChildren<BoxCollider2D>();

        Debug.Log("Checkpoint system found " + checkpointColliders.Length + " checkpoint colliders.");

        // Log each collider and its assigned index.
        for (int i = 0; i < checkpointColliders.Length; i++)
        {
            Debug.Log("Checkpoint " + i + ": " + checkpointColliders[i].name);
        }
    }

    // FIND NEAREST CHECKPOINT

    public Collider2D FindNearestCheckpoint(Vector3 position)
    {
        Collider2D nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (BoxCollider2D checkpointCollider in checkpointColliders)
        {
            // Find the closest point on this checkpoint's collider.
            Vector2 closestPoint = checkpointCollider.ClosestPoint(position);

            float distance = Vector2.Distance(position, closestPoint);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = checkpointCollider;
            }
        }

        return nearest;
    }

    // FIND SPAWN POSITION

    public bool FindSpawnPosition(Collider2D checkpointCollider, out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;

        Vector2 rayOrigin = checkpointCollider.bounds.center;

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            groundSearchDistance,
            groundLayer
        );

        if (hit.collider == null)
        {
            Debug.LogWarning("No ground found beneath " + checkpointCollider.name);
            return false;
        }

        spawnPosition = hit.point;
        return true;
    }


    // ACTIVATE CHECKPOINT

    public void ActivateCheckpoint(Collider2D checkpointCollider)
    {
        for (int i = 0; i < checkpointColliders.Length; i++)
        {
            if (checkpointColliders[i] != checkpointCollider)
            {
                continue;
            }

            if (!FindSpawnPosition(checkpointCollider, out Vector3 spawnPosition))
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(
                    i.ToString(),
                    spawnPosition
                );
            }

            return;
        }
    }


    // GIZMOS

    private void OnDrawGizmosSelected()
    {
        if (checkpointColliders == null) return;

        Gizmos.color = Color.yellow;

        foreach (BoxCollider2D checkpointCollider in checkpointColliders)
        {
            if (checkpointCollider == null) continue;

            Vector3 origin = checkpointCollider.bounds.center;

            Gizmos.DrawLine(
                origin,
                origin + Vector3.down * groundSearchDistance
            );
        }
    }
}