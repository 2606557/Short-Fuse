using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private ScreenManager screenManager;
    
    private Camera cameraComponent;
    private Vector2 followVelocity;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (screenManager.IsTransitioning)
        {
            return;
        }
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = target.position;

        targetPosition = ClampToScreen(targetPosition, screenManager.CurrentScreen);

        Vector2 newPosition = Vector2.SmoothDamp(currentPosition, targetPosition, ref followVelocity, smoothTime);

        newPosition = ClampToScreen(newPosition, screenManager.CurrentScreen);

        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

    public Vector2 ClampToScreen(Vector2 position, ScreenBounds screen)
    {
        Bounds bounds = screen.WorldBounds;

        float halfHeight = cameraComponent.orthographicSize;
        float halfWidth = halfHeight * cameraComponent.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;

        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        if (minX > maxX)
        {
            position.x = bounds.center.x;
        }
        else
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }
        if (minY > maxY)
        {
            position.y = bounds.center.y;
        }
        else
        {
            position.y = Mathf.Clamp(position.y, minY, maxY);
        }

        return position;
    }

    public void SetCameraPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);

        followVelocity = Vector2.zero;
    }
}
