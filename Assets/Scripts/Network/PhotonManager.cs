using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private int maxPlayersPerRoom = 2;

    private GameManager gm;

    private bool isConnecting;

    public int MaxPlayersPerRoom
    {
        get { return maxPlayersPerRoom; }
    }

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.EnableCloseConnection = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        //gm = GetComponent<GameManager>();
        // automatically connect to server
        // replaced into pressing enter on the start screen in MainMenuUI
        /*if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        } */

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // connecting to a lobby
    public void CreateNewRoom(string lobbyName)
    {
        if (PhotonNetwork.IsConnected)
        {

            // will create the lobby, or join it if it exists (thats the assumption anyway)
            GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().state = MainMenuState.CONNECTING;

            // NOTE:
            // I'm trying something here where i make the lobbies have 1 extra space on them so that a spectator can join
            // this will mean that when the 3rd (when 2 player mode is enabled) or 5th player joins they will need to be considered a spectator player
            if (!GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().Enabled4PlayerMode)
            {
                PhotonNetwork.CreateRoom(lobbyName, new RoomOptions { MaxPlayers = 3 }, TypedLobby.Default);
            }
            else
            {
                PhotonNetwork.CreateRoom(lobbyName, new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
            }
            
        }
        else
        {
            
            Debug.LogError("NOT CONNECTED TO SERVER!");
        }
        
    }

    public void JoinExistingRoom(string lobbyName)
    {
        if (PhotonNetwork.IsConnected)
        {
            GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().state = MainMenuState.CONNECTING;
            PhotonNetwork.JoinRoom(lobbyName);
        }
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().state = MainMenuState.ONLINE_MENU;
            GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().SetMenuObjects();
        }
        
    }

    // TODO: Have a loading screen between entering the scene and actually joining the room so that it doesn't look off
    public override void OnJoinedRoom()
    {
        Debug.Log("JoinedRoom");
        /*
        // TODO: Change this so that the players are able to decide if they wanna be Lewis or Bryn, rather than setting Player 1 as Lewis and Player 2 as Bryn
        GameObject temp = PhotonNetwork.Instantiate("Player", new Vector3(-1.92f, 1.39f, 0), Quaternion.identity);
        // TODO: replace these with setting the correct sprites later
        // TODO: Make sure player stats change with the character
        switch (PhotonNetwork.LocalPlayer.ActorNumber)
        {
            case 1:
                temp.GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 255);
                break;
            case 2:
                temp.GetComponent<SpriteRenderer>().color = new Color32(255, 246, 0, 255);
                break;

        }
        
        FindObjectOfType<CameraFollow>().OnConnectionToGame(PhotonNetwork.LocalPlayer.ActorNumber); */
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().state = MainMenuState.PLAYER_LOBBY;
            GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().SetMenuObjects();
            GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().SetLobbyUI();
            
        }


    }

    // calls upon failure to join a room
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // nothing was happening earlier so i figured i'd put this in
        Debug.LogError(message);

        GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().DisplayError(message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().DisplayError(message);
    }

    public override void OnLeftRoom()
    {
        // check if you're the host
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            foreach (var players  in PhotonNetwork.PlayerList)
            {
                if (players != PhotonNetwork.LocalPlayer)
                {
                    Debug.LogWarning("attempting to kick " + players.NickName);
                    PhotonNetwork.CloseConnection(players);
                    
                }
            }
            PhotonNetwork.LeaveRoom(true);

        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause != DisconnectCause.ApplicationQuit && cause != DisconnectCause.DisconnectByClientLogic)
        {
            string errorDetails = "";
            switch (cause)
            {
                case DisconnectCause.None:
                    errorDetails = "Unknown error";
                    break;
                case DisconnectCause.ExceptionOnConnect:
                    errorDetails = "The server is unavailable";
                    break;
                case DisconnectCause.DnsExceptionOnConnect:
                    errorDetails = "DNS Error\nYou may be offline";
                    break;
                case DisconnectCause.Exception:
                    errorDetails = "An exception occured\nThe server you are joining may be a local server";
                    break;
                case DisconnectCause.ServerAddressInvalid:
                    errorDetails = "The server address is invalid";
                    break;
                case DisconnectCause.ServerTimeout:
                    errorDetails = "The server timed out";
                    break;
                case DisconnectCause.ClientTimeout:
                    errorDetails = "The client timed out";
                    break;
                case DisconnectCause.DisconnectByServerReasonUnknown:
                    errorDetails = "Unknown error";
                    break;
                case DisconnectCause.InvalidAuthentication:
                    errorDetails = "Client authentication failed";
                    break;
                case DisconnectCause.MaxCcuReached:
                    errorDetails = "Master server maximum capacity reached";
                    break;
                case DisconnectCause.InvalidRegion:
                    errorDetails = "Unsupported region";
                    break;
                case DisconnectCause.DisconnectByOperationLimit:
                    errorDetails = "Too many operations were being called";
                    break;
                default:
                    errorDetails = "Disconnect code: " + cause;
                    break;

            }
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().DisplayError(errorDetails);
            }
            
            
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().state = MainMenuState.MAIN_MENU;
                GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().SetMenuObjects();
            }
        }
        Debug.LogWarningFormat("HECKTIC: Client disconnected from photon with reason {0}", cause);
    }

    // returns a string of the host's username
    public string GetMasterClientUserName()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
                return player.NickName;
            }
        }

        return null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            // maintain a consistent reference of if someones player 1 or 2
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("This player is Player 1");
                ExitGames.Client.Photon.Hashtable newProperty = new ExitGames.Client.Photon.Hashtable { { "isPlayer1", true } };
                ExitGames.Client.Photon.Hashtable newProperty1 = new ExitGames.Client.Photon.Hashtable { { "PlayerNumber", 1 }, { "IsSpectator", false } };
                newPlayer.SetCustomProperties(newProperty);
                newPlayer.SetCustomProperties(newProperty1);
            }
            else if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                Debug.Log("This player is Player 2");
                ExitGames.Client.Photon.Hashtable newProperty = new ExitGames.Client.Photon.Hashtable { { "isPlayer1", false } };
                ExitGames.Client.Photon.Hashtable newProperty1 = new ExitGames.Client.Photon.Hashtable { { "PlayerNumber", 2 }, { "IsSpectator", false } };
                newPlayer.SetCustomProperties(newProperty);
                newPlayer.SetCustomProperties(newProperty1);
            }
            else if (PhotonNetwork.CurrentRoom.PlayerCount == 3)
            {
                // TODO: MAKE THIS BETTER
                Debug.Log("This player is Player 3");
                ExitGames.Client.Photon.Hashtable newProperty = new ExitGames.Client.Photon.Hashtable { { "isPlayer1", false } };
                ExitGames.Client.Photon.Hashtable newProperty1 = new ExitGames.Client.Photon.Hashtable { { "PlayerNumber", 3 }, { "IsSpectator", true } };
                newPlayer.SetCustomProperties(newProperty1);
            }
            else if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
            {
                Debug.Log("This player is Player 4");
                ExitGames.Client.Photon.Hashtable newProperty = new ExitGames.Client.Photon.Hashtable { { "isPlayer1", false } };
                ExitGames.Client.Photon.Hashtable newProperty1 = new ExitGames.Client.Photon.Hashtable { { "PlayerNumber", 4 }, { "IsSpectator", false } };
                newPlayer.SetCustomProperties(newProperty1);
            }
            
            
            GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().UpdatePlayerListOnLobby();
            //GameObject.Find("MainMenuManager").GetPhotonView().RPC("UpdatePlayerListOnLobby", RpcTarget.All);
        }
        
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            //GameObject.Find("MainMenuManager").GetComponent<MainMenuUI>().UpdatePlayerListOnLobby();
        }
    }
}
