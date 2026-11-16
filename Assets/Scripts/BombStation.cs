using UnityEngine;

public class BombStation : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || player.IsStunned) return;
        player.RefillBombs();
        Debug.Log("Bomb station: refilled bombs.");
    }
}
