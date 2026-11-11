
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // INSTANCE

    public static GameManager Instance { get; private set; }


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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!bombsUnlocked)
        {
            currentBombs = 0;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
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

        if (!hasCheckpoint || scene.name != checkpointScene)
        {
            return;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player == null)
        {
            return;
        }

        player.transform.position = checkpointPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
