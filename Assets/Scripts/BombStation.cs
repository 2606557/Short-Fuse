using UnityEngine;

public class BombStation : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null) // Whatever triped the trap was not our player so ignore it
        {
            Debug.Log("Somthing that was not th Player has entered the bomb station");
            return;
        }

        Debug.Log("Player has entered the bomb station - Refilling bombs to max");
        player.RefillBombs();
    }

}
