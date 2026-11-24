using UnityEngine;

public class ScreenBounds : MonoBehaviour
{
    [SerializeField] private Vector2 screenSize = new Vector2(24f, 14f);

    public Bounds WorldBounds
    {
        get
        {
            return new Bounds(transform.position, new Vector3(screenSize.x, screenSize.y, 0f));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(WorldBounds.center, WorldBounds.size);
    }
}
