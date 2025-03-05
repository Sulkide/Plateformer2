using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayerInputManager playerInputManager;
    public List<GameObject> playerPrefabs; 
    private int nextPrefabIndex = 0; 

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (playerInputManager == null)
            playerInputManager = GetComponent<PlayerInputManager>();

        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    void OnPlayerJoined(PlayerInput player)
    {
        
        if (playerPrefabs.Count > 0)
        {
            playerInputManager.playerPrefab = playerPrefabs[nextPrefabIndex];
            
            nextPrefabIndex = (nextPrefabIndex + 1) % playerPrefabs.Count;
        }
    }
}