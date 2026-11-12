
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // SINGLETON

    // A singleton ensures that only one instance of this class exists.
    // The [STATIC] [INSTANCE] allows other scripts to access our GameManager
    // without needing a direct reference to it.

    public static GameManager Instance { get; private set; } // SINGLETON INSTANCE


    // BOMB INVENTORY

    [SerializeField] private bool bombsUnlocked = true;

    private int currentBombs = 3;
    private int maxBombs = 3;


    // CHECKPOINT DATA

    private string checkpointID;
    private string checkpointScene;
    private Vector3 checkpointPosition;
    private bool hasCheckpoint;


    // DEATH SETTINGS

    [Range(0f, 3f)]
    [SerializeField] private float respawnDelay = 0.5f;

    private bool playerDying;


    // SETUP

    private void Awake()
    {
        // IMPLEMENTING A SINGLETON AND ENSURING ONLY ONE INSTANCE EVER EXISTS

        // If no GameManager exists, this becomes our singleton.
        if (Instance == null)
        {
            Instance = this;

            // Keep the GameManager alive when scenes are loaded or reloaded.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If a GameManager already exists, destroy this duplicate.
            Destroy(gameObject);
            return;
        }

        if (!bombsUnlocked)
        {
            currentBombs = 0;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Start runs after the scene objects have completed Awake.
        // Our checkpoints have now registered their child colliders.
        SetupInitialSpawn();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }


    // BOMB INVENTORY

    public int GetBombCount()
    {
        return bombsUnlocked ? currentBombs : 0;
    }

    public bool BombsUnlocked()
    {
        return bombsUnlocked;
    }

    public void SetBombCount(int amount)
    {
        if (!bombsUnlocked)
        {
            return;
        }

        currentBombs = Mathf.Clamp(amount, 0, maxBombs);
    }

    public void UnlockBombs()
    {
        bombsUnlocked = true;
        currentBombs = maxBombs;
    }


    // INITIAL SPAWN

    private void SetupInitialSpawn()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        Checkpoint checkpoints = FindFirstObjectByType<Checkpoint>();

        if (player == null || checkpoints == null)
        {
            Debug.LogWarning("Initial spawn requires a Player and Checkpoint system.");
            return;
        }

        // Find the checkpoint nearest to the player's editor position.
        Collider2D nearest = checkpoints.FindNearestCheckpoint(
            player.transform.position
        );

        if (nearest == null)
        {
            Debug.LogWarning("No checkpoint colliders found!");
            return;
        }

        // Activate the checkpoint, registering its safe ground position.
        checkpoints.ActivateCheckpoint(nearest);

        // Spawn the player using the registered checkpoint position.
        if (hasCheckpoint)
        {
            player.SpawnAt(checkpointPosition);
        }
    }


    // CHECKPOINTS

    public void SetCheckpoint(string id, Vector3 position)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (hasCheckpoint &&
            checkpointScene == sceneName &&
            checkpointID == id)
        {
            return;
        }

        checkpointID = id;
        checkpointScene = sceneName;
        checkpointPosition = position;
        hasCheckpoint = true;

        Debug.Log("Checkpoint activated: " + id);
    }


    // PLAYER DEATH

    public void PlayerDied()
    {
        if (playerDying)
        {
            return;
        }

        StartCoroutine(RespawnPlayer());
    }

    private IEnumerator RespawnPlayer()
    {
        playerDying = true;

        yield return new WaitForSecondsRealtime(respawnDelay);

        currentBombs = bombsUnlocked ? maxBombs : 0;

        string sceneToLoad = SceneManager.GetActiveScene().name;

        if (hasCheckpoint)
        {
            sceneToLoad = checkpointScene;
        }

        SceneManager.LoadScene(sceneToLoad);
    }


    // SCENE LOADING

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerDying = false;

        // No saved checkpoint means the initial spawn is handled in Start.
        if (!hasCheckpoint || scene.name != checkpointScene)
        {
            return;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player == null)
        {
            return;
        }

        // Checkpoint positions represent the player's feet.
        player.SpawnAt(checkpointPosition);

        Debug.Log("Player respawned at checkpoint: " + checkpointID);
    }
}
