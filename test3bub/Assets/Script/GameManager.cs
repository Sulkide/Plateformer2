using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayerInputManager playerInputManager;
    public List<GameObject> playerPrefabs; 
    public Transform parentForPlayers; // Référence au GameObject parent dans la scène
    private int nextPrefabIndex = 0; 
    
    public bool isSulkidePresent;
    public bool isDarckoxPresent;
    public bool isSulanaPresent;
    public bool isSlowPresent;

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
        // Choix du prefab selon la liste
        if (playerPrefabs.Count > 0)
        {
            playerInputManager.playerPrefab = playerPrefabs[nextPrefabIndex];
            nextPrefabIndex = (nextPrefabIndex + 1) % playerPrefabs.Count;
        }
        
        // Définir le GameObject parent si la référence est renseignée
        if (parentForPlayers != null)
        {
            player.transform.SetParent(parentForPlayers);
        }
    }
}