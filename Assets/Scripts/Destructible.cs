using UnityEngine;

public class Destructible : MonoBehaviour
{
    // DESTRUCTION

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}