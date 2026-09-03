using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

/*
 * SessionManager is intentionally light in the starter project.
 * Local Pong starts immediately. The GameManager, NetworkManager, and button
 * references mark where you will start a Host or Client session.
 */

public class SessionManager : NetworkBehaviour
{
    [Header("Multiplayer")]
    [SerializeField] GameManager gameManager;
    
    // This does not already exist in the scene, you need to add it and reference it
    [SerializeField] NetworkManager networkManager;

    [Header("Multiplayer UI")]
    [SerializeField] Button startHostButton;
    [SerializeField] Button startClientButton;
    [SerializeField] Canvas sessionUI;
    [SerializeField] GameObject connectionButtons;

    void Awake()
    {
        // Hide Host/Client until you are ready to wire the session.
        sessionUI.gameObject.SetActive(true);
        connectionButtons.SetActive(true);
        
        startHostButton.onClick.AddListener(StartHost);
        startClientButton.onClick.AddListener(StartClient);
    }

    void StartHost()
    {
        if (networkManager.StartHost())
            connectionButtons.SetActive(false);
    }

    void StartClient()
    {
        if (networkManager.StartClient())
            connectionButtons.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        networkManager.OnClientConnectedCallback += OnCLientConnected;
    }
    
    public override void OnNetworkDespawn()
    {
        networkManager.OnClientConnectedCallback -= OnCLientConnected;
    }

    void OnCLientConnected(ulong clientId)
    {
        Debug.Log($"CLient {clientId} connected" + $"({networkManager.ConnectedClients.Count} total");
        
        if (networkManager.ConnectedClients.Count == 2)
            gameManager.StartGame();
    }
}