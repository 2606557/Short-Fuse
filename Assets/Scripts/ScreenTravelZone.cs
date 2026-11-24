using UnityEngine;

public class ScreenTravelZone : MonoBehaviour
{
    public enum TravelDirection
    {
        North,
        East,
        South,
        West
    }

    [SerializeField] private TravelDirection direction;
    [SerializeField] private ScreenTravelZone linkedZone;
    [SerializeField] private Transform arrivalPoint;
    [SerializeField] private ScreenManager screenManager;

    public ScreenTravelZone LinkedZone => linkedZone;
    public Transform ArrivalPoint => arrivalPoint;

    public ScreenBounds OwnerScreen => GetComponentInParent<ScreenBounds>();

    public Vector2 OutwardDirection
    {
        get
        {
            switch (direction)
            {
                case TravelDirection.North:
                    return Vector2.up;

                case TravelDirection.East:
                    return Vector2.right;

                case TravelDirection.South:
                    return Vector2.down;

                case TravelDirection.West:
                    return Vector2.left;

                default:
                    return Vector2.zero;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(transform.position, OutwardDirection * 2f);

        if (LinkedZone != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(transform.position, linkedZone.transform.position);
        }

        if (arrivalPoint != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(arrivalPoint.position, 0.25f);
        }
        
    }
}
