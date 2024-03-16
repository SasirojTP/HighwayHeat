using System;
using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using QFSW.QC;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManagerScript : NetworkBehaviour
{
    [Header ("Variable")]
    public NetworkVariable<int> playerNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<NetworkString> hostJoinCode = new NetworkVariable<NetworkString>(
        new NetworkString { info = "Code" }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isJoinCodeCorrect = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public GameManager gameManager;
    UInt32 playerColor = 1652872834;
    bool isApproveConnection = false;

    public List<Transform> startPosition = new List<Transform>();

    [Command("set-approve")]
    public bool SetIsApproveConnection()
    {
        isApproveConnection = !isApproveConnection;
        return isApproveConnection;
    }

    [Header ("UI")]
    public GameObject loginPanel;
    public GameObject leaveButton;
    public GameObject lobbyPanel;
    public TMP_InputField joinCodeInputField;
    [SerializeField] TMP_Text gameCodeText;
    [SerializeField] TMP_Text playerAmountText;
    [SerializeField] GameObject popUpPanel;
    [SerializeField] TextMeshProUGUI errorText;


    public struct NetworkString : INetworkSerializable
    {
        public FixedString32Bytes info;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref info);
        }

        public override string ToString()
        {
            return info.ToString();
        }

        public static implicit operator NetworkString(string v) => 
            new NetworkString() { info = new FixedString32Bytes(v) };
    }
    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        loginPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        popUpPanel.SetActive(false);
    }

    private void Update()
    {
        UpdatePlayerAmountText();
        UpdateGameCodeText();
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        Debug.Log("HandleClientDisconnect client ID = " + clientId);
        if (NetworkManager.Singleton.IsHost) { playerNum.Value--; }
        else if (NetworkManager.Singleton.IsClient) { Leave(); }
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown(); // shutdown
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            SceneManager.LoadScene(0);

        }
        else if (NetworkManager.Singleton.IsClient)
        {
            if (isJoinCodeCorrect.Value == true)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                PopUpJoinCodeIncorrect();
            }
            NetworkManager.Singleton.Shutdown(); // shutdown
        }
        loginPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log("HandleClientConnected client ID = " + clientId);
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            loginPanel.SetActive(false);
            lobbyPanel.SetActive(true);
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
        playerNum.Value = 0;
        hostJoinCode.Value = RandomJoinCode();
        UpdateGameCodeText();
        print(hostJoinCode.Value);
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        gameManager.OnCreateServer();
        Debug.Log("start host");
    }

    NetworkString RandomJoinCode()
    {
        var character = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var codeLength = 6;
        var randomCode = new char[codeLength];
        for (int i = 0; i < codeLength; i++)
        {
            randomCode[i] = character[UnityEngine.Random.Range(0, character.Length)];
        }
        return new NetworkString() { info = new FixedString32Bytes(new string(randomCode)) };
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;

        int byteLength = connectionData.Length;
        bool isApproved = false;
        if (byteLength > 0)
        {
            string combinedString = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);
            isApproved = ApproveJoinCode(combinedString, hostJoinCode.Value);
            SetPlayerCarColor();
        }

        // Your approval logic determines the following values
        response.Approved = isApproved;
        response.CreatePlayerObject = true;

        // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        //response.PlayerPrefabHash = null;
        response.PlayerPrefabHash = playerColor;
        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = startPosition[playerNum.Value].position;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;
        NetworkLog.LogInfoServer("spawnPos of " + clientId + " is " + response.Position.ToString());
        // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        response.Reason = "Some reason for not approving the client";

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
        playerNum.Value++;
        gameManager.playerAlive.Value++;
    }

    public void Client()
    {
        string hostJoinCode = joinCodeInputField.GetComponent<TMP_InputField>().text;
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(hostJoinCode);
        NetworkManager.Singleton.StartClient();
        Debug.Log("start client");
    }

    bool ApproveJoinCode(NetworkString clientJoinCode, NetworkString hostJoinCode)
    {
        bool isApprove = System.String.Equals(clientJoinCode.info.ToString().Trim(), hostJoinCode.info.ToString().Trim()) ? true : false;
        if (isApprove == true)
        {
            gameCodeText.text = "Game Code: " + hostJoinCode.info;
            isJoinCodeCorrect.Value = true;
            print("Game Code: " + hostJoinCode.info);
        }
        else
        {
            isJoinCodeCorrect.Value = false;
        }
        Debug.Log("ApproveJoinCode = " + isApprove);
        return isApprove;
    }

    void PopUpJoinCodeIncorrect()
    {
        popUpPanel.SetActive(true);
        errorText.text = "Code incorrect";
    }
    void UpdateGameCodeText()
    {
        gameCodeText.text = "Game Code: " + hostJoinCode.Value;
    }

    void UpdatePlayerAmountText()
    {
        playerAmountText.text = "Player " + playerNum.Value.ToString() + "/4";
    }

    void SetPlayerCarColor()
    {
        switch (playerNum.Value)
        {
            case 0:
                playerColor = 1652872834;
                break;
            case 1:
                playerColor = 1158519564;
                break;
            case 2:
                playerColor = 676951331;
                break;
            case 3:
                playerColor = 3589217347;
                break;
        }
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }
}
