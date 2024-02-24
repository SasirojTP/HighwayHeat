using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using QFSW.QC;
using TMPro;

public class LoginManagerScript : MonoBehaviour
{
    public int playerNum = 0;

    public TMP_InputField userNameInputField;

    public List<Transform> startPosition = new List<Transform>();

    private bool isApproveConnection = false;
    [Command("set-approve")]
    public bool SetIsApproveConnection()
    {
        isApproveConnection = !isApproveConnection;
        return isApproveConnection;
    }

    public GameObject loginPanel;
    public GameObject leaveButton;

    public GameManager gameManager;

    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        loginPanel.SetActive(true);
        leaveButton.SetActive(false);
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        Debug.Log("HandleClientDisconnect client ID = " + clientId);
        if (NetworkManager.Singleton.IsHost) { }
        else if (NetworkManager.Singleton.IsClient) {playerNum--; Leave(); }
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown(); // shutdown
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;

        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown(); // shutdown
        } 
        // show login panel
        loginPanel.SetActive(true);
        // hide leave button
        leaveButton.SetActive(false);
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log("HandleClientConnected client ID = " + clientId);
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            loginPanel.SetActive(false);
            leaveButton.SetActive(true);
        }
    }

    private void HandleServerStarted()
    {
        Debug.Log("HandleServerStarted");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public void Host()
    {
        playerNum = 0;
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        gameManager.OnCreateServer();
        Debug.Log("start host");
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;

        int byteLength = connectionData.Length;
        bool isApproved = false;
        int characterPrefabIndex = 0;
        if (byteLength > 0)
        {
            string combinedString = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);
            string[] extractedStrings = HelperScript.ExtractStrings(combinedString);
            for (int i = 0; i < extractedStrings.Length; i++)
            {
                if (i == 0)
                {
                    string clientData = extractedStrings[i];
                    string hostData = userNameInputField.GetComponent<TMP_InputField>().text;
                    isApproved = ApproveConnection(clientData, hostData);
                }
                else if (i == 1){
                    characterPrefabIndex = int.Parse(extractedStrings[i]);
                }
            }
        }

        // Your approval logic determines the following values
        response.Approved = isApproved;
        response.CreatePlayerObject = true;

        // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        //response.PlayerPrefabHash = null;
        response.PlayerPrefabHash = null;
        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = startPosition[playerNum].position;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;
        NetworkLog.LogInfoServer("spawnPos of " + clientId + " is " + response.Position.ToString());
        // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        response.Reason = "Some reason for not approving the client";

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
        playerNum++;

    }

    public void Client()
    {
        string userName = userNameInputField.GetComponent<TMP_InputField>().text;
        string[] inputFields = { userName };
        string clientData = HelperScript.CombineStrings(inputFields);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(clientData);
        NetworkManager.Singleton.StartClient();
        Debug.Log("start client");
    }

    public bool ApproveConnection(string clientData, string hostData)
    {
        bool isApprove = System.String.Equals(clientData.Trim(), hostData.Trim()) ? false : true;
        Debug.Log("isApprove = " + isApprove);

        return isApprove && (playerNum < 4);
    }
}
