using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Dictionary<int, int> playerGamepadMap = new Dictionary<int, int>(); // Associe un joueur à un ID de gamepad

    void Awake()
    {
        instance = this;
        InputSystem.onDeviceChange += OnDeviceChange; // Écoute les changements de périphériques
    }

    void Start()
    {
        AssignGamepads();
    }

    void AssignGamepads()
    {
        playerGamepadMap.Clear();
        int playerIndex = 1;

        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad gamepad && playerIndex <= 4) // Max 4 joueurs
            {
                playerGamepadMap[playerIndex] = gamepad.deviceId;
                Debug.Log($"Joueur {playerIndex} assigné à la manette {gamepad.deviceId}");
                playerIndex++;
            }
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    Debug.Log($"Nouvelle manette connectée : {device.name}");
                    AssignGamepads(); // Réattribuer les manettes
                    break;
                case InputDeviceChange.Removed:
                    Debug.Log($"Manette déconnectée : {device.name}");
                    AssignGamepads(); // Mettre à jour les assignations
                    break;
            }
        }
    }

    public int GetPlayerGamepadID(int playerIndex)
    {
        return playerGamepadMap.ContainsKey(playerIndex) ? playerGamepadMap[playerIndex] : -1;
    }
}