using UnityEngine;

public class BombStation : MonoBehaviour
{
    // TRIGGER DETECTION

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
        {
            return;
        }

        Debug.Log("Something... or someone... has entered the bomb station.");
    }
}
