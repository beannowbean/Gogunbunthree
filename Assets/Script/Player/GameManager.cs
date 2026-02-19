using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Spawn Settings")]
    public GameObject playerPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}