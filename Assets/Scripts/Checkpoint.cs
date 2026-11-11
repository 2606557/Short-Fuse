
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // CHECKPOINT SETTINGS

    [SerializeField] private string checkpointID = "Checkpoint";


    // REFERENCES

    [SerializeField] private Transform spawnPoint;


    // TRIGGER DETECTION

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
        {
            return;
        }

        if (!player.IsGrounded())
        {
            return;
        }

        ActivateCheckpoint();
    }


    // ACTIVATE CHECKPOINT

    private void ActivateCheckpoint()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("Checkpoint has no SpawnPoint!");
            return;
        }

        GameManager.Instance.SetCheckpoint(
            checkpointID,
            spawnPoint.position
        );
    }
}
