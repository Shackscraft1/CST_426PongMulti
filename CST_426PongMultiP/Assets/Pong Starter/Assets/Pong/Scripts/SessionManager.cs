using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/*
 * SessionManager is intentionally light in the starter project.
 * Local Pong starts immediately. The GameManager, NetworkManager, and button
 * references mark where you will start a Host or Client session.
 */

public class SessionManager : MonoBehaviour
{
    [Header("Multiplayer")]
    [SerializeField] GameManager gameManager;
    
    // This does not already exist in the scene, you need to add it and reference it
    [SerializeField] NetworkManager networkManager;

    [Header("Multiplayer UI")]
    [SerializeField] Button startHostButton;
    [SerializeField] Button startClientButton;
    [SerializeField] Canvas sessionUI;

    void Awake()
    {
        // Hide Host/Client until you are ready to wire the session.
        sessionUI.gameObject.SetActive(false);
        
        startHostButton.onClick.AddListener(() => Debug.Log("TODO: Start Host"));
        startClientButton.onClick.AddListener(() => Debug.Log("TODO: Start Client"));
    }
}