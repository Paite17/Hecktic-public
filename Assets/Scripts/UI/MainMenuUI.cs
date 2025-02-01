using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using MrLewisPaite.JSONSave;
using System.IO;
using UnityEngine.TextCore.Text;

public enum MainMenuState
{
    START_SCREEN,
    MAIN_MENU,
    ONLINE_MENU,
    HOST_GAME_SETTINGS,
    PLAYER_LOBBY,
    CONNECTING,
    CONNECTION_ERROR,
    CREDITS
}

public class MainMenuUI : MonoBehaviourPunCallbacks, IPunObservable
{
    public MainMenuState state;

    [Header("Menu Objects")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject onlineMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject connectingScreen;
    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private GameObject hostSettingsMenu;

    [Header("Start Screen")]
    [SerializeField] private GameObject startScreenConnecting;
    [SerializeField] private GameObject pressStartText;

    [Header("Error UI")]
    [SerializeField] private GameObject errorPopUp;
    [SerializeField] private TMP_Text errorText;

    [Header("Online Menu UI")]
    [SerializeField] private TMP_InputField userName;
    [SerializeField] private TMP_InputField lobbyCodeInputField;

    
    [Header("Host Settings UI")]
    [SerializeField] private TMP_Dropdown charDropdown;
    [SerializeField] private TMP_Dropdown playerCountDropdown;

    [Header("Lobby UI")]
    [SerializeField] private GameObject hostSettings;
    [SerializeField] private TMP_Text lobbyNameText; // lobby would probably display as [username]'s lobby
    [SerializeField] private List<UIPlayerEntry> playerInfo;
    [SerializeField] private List<Image> playerChar;
    [SerializeField] private TMP_Text lobbyCode;
    [SerializeField] private GameObject lobbyPlayerInfoTemplate;
    [SerializeField] private Transform lobbyContentContainer;
    [SerializeField] private TMP_InputField timeLimitInput;
    [SerializeField] private TMP_InputField collectableAmountInput;
    [SerializeField] private TMP_InputField lifeAmountInput;
    [SerializeField] private TMP_InputField winAmountInput;
    [SerializeField] private TMP_Dropdown mapSelection;
    [SerializeField] private TMP_Dropdown charSelect;
    [SerializeField] private Button startButton;
    [SerializeField] private Button characterButton;

    [Header("Character Select UI")]
    [SerializeField] private GameObject charSelectMenu;
    [SerializeField] private TMP_Text charNameText;
    [SerializeField] private List<Character> allCharacters;
    [SerializeField] private List<GameObject> charModels;

    [Header("Misc Objects")]
    [SerializeField] private PhotonManager photonManager;
    [SerializeField] private PlayerStatus currentPlayerStatus;
    [SerializeField] private GameObject offlinePopup;
    [SerializeField] private GameObject creditsScreen;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image levelProgressBar;
    [SerializeField] private GameObject quitButton;
    public int lobbyWinAmount;
    public int lobbyLifeAmount;

    [Header("Event Mode")]
    [SerializeField] private GameObject eventLobbyButton;
    [SerializeField] private TMP_Text eventModeLabel;
    [SerializeField] private GameObject eventModeDetailsButton;
    [SerializeField] private GameObject eventModeDetailsPopup;
    [SerializeField] private TMP_Text eventModeDetailsModeLabel;
    [SerializeField] private TMP_Text eventModeDescription;
    [SerializeField] private TMP_Text eventModeLobbyText;
    [SerializeField] private Image eventModePromoImage;
    [SerializeField] private float eventMode_CheckTimerLength;
    public string eventMode_GameMode;
    public string eventMode_StartTime;
    public string eventMode_EndTime;
    public string eventMode_Description;

    private bool lobbyHasLoadedBefore;

    private ExitGames.Client.Photon.Hashtable lobbySettings = new ExitGames.Client.Photon.Hashtable();

    private int localCharID = 1;
    private int roundTime;
    private int mcguffinAmount;
    private float eventMode_CheckTimer;

    private bool enabled4PlayerMode;


    // leveling
    private PlayerLevelling playerLevel;

    public bool Enabled4PlayerMode
    {
        get { return enabled4PlayerMode; }
    }


    // Start is called before the first frame update
    void Start()
    {


        string path = Application.persistentDataPath + "/playerLevelData.json";
        if (File.Exists(path))
        {
            PlayerLevelling loadedLevel = JSONStorage.LoadFromFile<PlayerLevelling>(path);
            playerLevel = loadedLevel;
        }
        else
        {
            playerLevel = new PlayerLevelling { CurrentLevel = 0, CurrentEXP = 0, EXPToNext = 350, HasLaunched = false };

            JSONStorage.SaveDataToFile(playerLevel, path);
        }

        PressCharacterButton(1);
        lobbySettings = new ExitGames.Client.Photon.Hashtable()
        {
            { "GameMode",  eventMode_GameMode },
            { "Player1Character", localCharID },
            { "Player2Character", localCharID },
            { "RoundTime", roundTime },
            { "McGuffinAmount", 5 },
            { "WinAmount", lobbyWinAmount },
            { "LifeAmount", lobbyLifeAmount }
        };
        levelText.text = "Level " + playerLevel.CurrentLevel;

        
        
        GetImageFill();
        if (PhotonNetwork.IsConnected)
        {
            state = MainMenuState.MAIN_MENU;
        }
        SetMenuObjects();
        FindObjectOfType<AudioManager>().Play("TitleScreen");

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            quitButton.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (state == MainMenuState.ONLINE_MENU)
        {
            eventMode_CheckTimer += Time.deltaTime;

            if (eventMode_CheckTimer > eventMode_CheckTimerLength)
            {
                //fbManager.GetCurrentMode();
            }
        }



        if (state == MainMenuState.START_SCREEN)
        {
            // eh
            if (UnityEngine.Input.anyKeyDown)
            {
                OpenMainMenu();
                
            }
        }

        if (state == MainMenuState.CONNECTION_ERROR)
        {
            if (UnityEngine.Input.anyKeyDown)
            {
                StartCoroutine(DismissError());
            }
        }

        if (state == MainMenuState.PLAYER_LOBBY)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.PlayerList.Length < 2 && timeLimitInput.text != null && collectableAmountInput.text != null && lifeAmountInput.text != null)
                {
                    startButton.interactable = false;
                }
                else
                {
                    startButton.interactable = true;
                }

                for (int i = 0; i < playerInfo.Count; i++)
                {
                    playerInfo[i].playerPingText.text = PhotonNetwork.GetPing() + "ms";
                }
            }
        }


    }


    // perhaps this should only be called by the master client? (at least in regards to the settings only the host can change
    public void UpdateLobbySettings()
    {
        lobbySettings = new ExitGames.Client.Photon.Hashtable()
        {
            { "RoundTime", roundTime },
            { "McGuffinAmount", mcguffinAmount },
            { "WinAmount", lobbyWinAmount },
            { "LifeAmount", lobbyLifeAmount },
            { "GameMode", eventMode_GameMode }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(lobbySettings);
    }
    
    public void OnOnlineBackButton()
    {
        StartCoroutine(DismissError());
    }

    // grrr i hate coroutines
    private IEnumerator DismissError()
    {
        PhotonNetwork.Disconnect();

        yield return new WaitForSeconds(0.5f);

        state = MainMenuState.MAIN_MENU;
        SetMenuObjects();
    }

    public void DisplayError(string error)
    {
        state = MainMenuState.CONNECTION_ERROR;
        SetMenuObjects();
        errorText.text = error;  
    }

    // Online button
    public void PlayOnline()
    {
        mainMenu.SetActive(false);
        startScreenConnecting.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }

    // confirmation of a popup
    public void PlayOffline()
    {
        ExitGames.Client.Photon.Hashtable newProperty1 = new ExitGames.Client.Photon.Hashtable { { "PlayerNumber", 1 }, { "IsSpectator", false } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProperty1);
        FindObjectOfType<AudioManager>().StopMusic("TitleScreen");
        SceneManager.LoadScene("LevelScene");
    }

    // saying no on the offline prompt
    public void NoToOffline()
    {
        offlinePopup.SetActive(false);
    }

    // open credits !!!!
    public void OpenCredits()
    {
        state = MainMenuState.CREDITS;
        SetMenuObjects();
    }

    // bye bye credits
    public void CloseCredits()
    {
        state = MainMenuState.MAIN_MENU;
        SetMenuObjects();
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    // popup about offline mode
    public void MakePopupAppear()
    {
        offlinePopup.SetActive(true);
    }

    // the REAL press anything to continue
    public void OpenMainMenu()
    {
        state = MainMenuState.MAIN_MENU;
        SetMenuObjects();
    }

    // when pressing the 'Create lobby' button on the menu
    public void ShowHostSettings()
    {
        state = MainMenuState.HOST_GAME_SETTINGS;
        SetMenuObjects();
    }

    public void OnPlayerCountChange()
    {
        if (playerCountDropdown.value == 0)
        {
            enabled4PlayerMode = false;
        }
        else
        {
            enabled4PlayerMode = true;
        }
        
    }

    // when pressing the 'Create Lobby' button on the host setting bit
    public void OnCreateLobbyButton()
    {
        if (PhotonNetwork.LocalPlayer.NickName != null)
        {
            StartCoroutine(LobbyCreateOnDelay());
        }
        else
        {
            Debug.LogError("Username is blank!");
            return;
        }  
    }

    private IEnumerator LobbyCreateOnDelay()
    {
        photonManager.CreateNewRoom(GenerateLobbyName());

        yield return new WaitForSeconds(0.5f);
        PressCharacterButton(1);
        ApplyCharacter();
        ExitGames.Client.Photon.Hashtable newProperty = new ExitGames.Client.Photon.Hashtable { { "isPlayer1", true } };
        ExitGames.Client.Photon.Hashtable newProperty1 = new ExitGames.Client.Photon.Hashtable { { "PlayerNumber", 1 }, { "IsSpectator", false } };
        lobbySettings = new ExitGames.Client.Photon.Hashtable() { { "GameMode", "HECKTIC" } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(lobbySettings);
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProperty);
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProperty1);

        ExitGames.Client.Photon.Hashtable playerTable = new ExitGames.Client.Photon.Hashtable()
        {
            { "Player1Character",  localCharID },
            { "Player2Character", localCharID },
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(playerTable);
        state = MainMenuState.PLAYER_LOBBY;
        SetMenuObjects();
        SetLobbyUI();
    }

    // when pressing the 'Join Lobby' button
    public void OnJoinLobbyButton()
    {
        // check if input field is blank
        if (lobbyCodeInputField.text != string.Empty)
        {
            photonManager.JoinExistingRoom(lobbyCodeInputField.text);
        }
    }

    // on disconnect button within the lobby UI
    public void OnLobbyDisconnectButton()
    {
        PhotonNetwork.LeaveRoom();
        lobbyHasLoadedBefore = false;
        playerInfo.Clear();
    }

   
    public void OnRoundStartButton()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork: This client is trying to load a level but it isn't the master");
            return;
        }
        Debug.LogFormat("PhotonNetwork: Loading level: {0}", PhotonNetwork.CurrentRoom);
        Debug.Log(mapSelection.itemText.text);
        
        UpdateLobbySettings();

        PhotonNetwork.LoadLevel(mapSelection.captionText.text);
    }

    private void GetImageFill()
    {
        float fillAmount = (float)playerLevel.CurrentEXP / playerLevel.EXPToNext;
        levelProgressBar.fillAmount = fillAmount;
    }

    private string GenerateLobbyName()
    {
        int nums = UnityEngine.Random.Range(100000, 999999);
        string conversion = Convert.ToString(nums);
        Debug.Log(conversion);
        return conversion;
    }

    // upon pressing the 'Event Details' button
    public void DisplayEventDetails()
    {
        eventModeDetailsPopup.SetActive(true);
        eventModeDetailsModeLabel.text = eventMode_GameMode;
        eventModeDescription.text = eventMode_Description;
    }

    // setting everything for the UI on the lobby
    public void SetLobbyUI()
    {
        // lobby code
        lobbyCode.text = "Lobby Code:\n" + PhotonNetwork.CurrentRoom.Name;

        // if event mode 
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object thisGameMode))
        {
            string gameMode = thisGameMode.ToString();
            eventModeLobbyText.text = gameMode;

            // TODO: Adjust lobby further for future game modes that change things
        }

        // top label
        lobbyNameText.text = photonManager.GetMasterClientUserName() + "'s " + "Lobby";

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            hostSettings.SetActive(true);
            UpdateCollectable();
        }    
        else
        {
            hostSettings.SetActive(false);
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsSpectator", out object isSpectator))
        {
            if ((bool)isSpectator)
            {
                characterButton.interactable = false;
            }
            else
            {
                characterButton.interactable = true;
            }
        }

        
        StartCoroutine(UpdateDetailsOnDelay());
    }

    // the big switch statement that'll sort out which objects should be active based on the scene
    public void SetMenuObjects()
    {
        switch (state)
        {
            case MainMenuState.START_SCREEN:
                startScreen.SetActive(true);
                startScreenConnecting.SetActive(false);
                pressStartText.SetActive(true);
                onlineMenu.SetActive(false);
                hostSettingsMenu.SetActive(false);
                lobbyMenu.SetActive(false);
                connectingScreen.SetActive(false);
                errorPopUp.SetActive(false);
                offlinePopup.SetActive(false);
                creditsScreen.SetActive(false);
                charSelectMenu.SetActive(false);
                eventLobbyButton.SetActive(false);
                eventModeLabel.gameObject.SetActive(false);
                eventModeDetailsButton.SetActive(false);
                break;
            case MainMenuState.MAIN_MENU:
                startScreen.SetActive(false);
                startScreenConnecting.SetActive(false);
                pressStartText.SetActive(false);
                mainMenu.SetActive(true);
                onlineMenu.SetActive(false);
                hostSettingsMenu.SetActive(false);
                lobbyMenu.SetActive(false);
                connectingScreen.SetActive(false);
                errorPopUp.SetActive(false);
                offlinePopup.SetActive(false);
                creditsScreen.SetActive(false);
                charSelectMenu.SetActive(false);
                eventLobbyButton.SetActive(false);
                eventModeLabel.gameObject.SetActive(false);
                eventModeDetailsButton.SetActive(false);
                break;
            case MainMenuState.ONLINE_MENU:
                startScreen.SetActive(false);
                startScreenConnecting.SetActive(false);
                pressStartText.SetActive(false);
                mainMenu.SetActive(false);
                onlineMenu.SetActive(true);
                hostSettingsMenu.SetActive(false);
                lobbyMenu.SetActive(false);
                connectingScreen.SetActive(false);
                errorPopUp.SetActive(false);
                offlinePopup.SetActive(false);
                creditsScreen.SetActive(false);
                charSelectMenu.SetActive(false);
                break;
            case MainMenuState.HOST_GAME_SETTINGS:
                startScreen.SetActive(false);
                startScreenConnecting.SetActive(false);
                pressStartText.SetActive(false);
                onlineMenu.SetActive(false);
                mainMenu.SetActive(false);
                hostSettingsMenu.SetActive(true);
                lobbyMenu.SetActive(false);
                connectingScreen.SetActive(false);
                errorPopUp.SetActive(false);
                offlinePopup.SetActive(false);
                creditsScreen.SetActive(false);
                eventLobbyButton.SetActive(false);
                eventModeLabel.gameObject.SetActive(false);
                eventModeDetailsButton.SetActive(false);
                break;
            case MainMenuState.PLAYER_LOBBY:
                startScreen.SetActive(false);
                startScreenConnecting.SetActive(false);
                pressStartText.SetActive(false);
                onlineMenu.SetActive(false);
                mainMenu.SetActive(false);
                hostSettingsMenu.SetActive(false);
                lobbyMenu.SetActive(true);
                connectingScreen.SetActive(false);
                errorPopUp.SetActive(false);
                offlinePopup.SetActive(false);
                creditsScreen.SetActive(false);
                eventLobbyButton.SetActive(false);
                eventModeLabel.gameObject.SetActive(false);
                eventModeDetailsButton.SetActive(false);
                break;
            case MainMenuState.CONNECTING:
                startScreen.SetActive(false);
                startScreenConnecting.SetActive(false);
                pressStartText.SetActive(false);
                mainMenu.SetActive(false);
                onlineMenu.SetActive(false);
                hostSettingsMenu.SetActive(false);
                lobbyMenu.SetActive(false);
                connectingScreen.SetActive(true);
                errorPopUp.SetActive(false);
                offlinePopup.SetActive(false);
                creditsScreen.SetActive(false);
                charSelectMenu.SetActive(false);
                eventLobbyButton.SetActive(false);
                eventModeLabel.gameObject.SetActive(false);
                eventModeDetailsButton.SetActive(false);
                break;
            case MainMenuState.CONNECTION_ERROR:
                startScreen.SetActive(false);
                startScreenConnecting.SetActive(false);
                pressStartText.SetActive(false);
                mainMenu.SetActive(false);
                onlineMenu.SetActive(false);
                hostSettingsMenu.SetActive(false);
                lobbyMenu.SetActive(false);
                connectingScreen.SetActive(false);
                errorPopUp.SetActive(true);
                offlinePopup.SetActive(false);
                creditsScreen.SetActive(false);
                charSelectMenu.SetActive(false);
                eventLobbyButton.SetActive(false);
                eventModeLabel.gameObject.SetActive(false);
                eventModeDetailsButton.SetActive(false);
                break;
            case MainMenuState.CREDITS:
                startScreen.SetActive(false);
                startScreenConnecting.SetActive(false);
                pressStartText.SetActive(false);
                mainMenu.SetActive(false);
                onlineMenu.SetActive(false);
                hostSettingsMenu.SetActive(false);
                lobbyMenu.SetActive(false);
                connectingScreen.SetActive(false);
                errorPopUp.SetActive(false);
                offlinePopup.SetActive(false);
                creditsScreen.SetActive(true);
                charSelectMenu.SetActive(false);
                eventLobbyButton.SetActive(false);
                eventModeLabel.gameObject.SetActive(false);
                eventModeDetailsButton.SetActive(false);
                break;

        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        photonView.RPC("CallListUpdateOverNetwork", RpcTarget.All);
    }

    [PunRPC]
    private  void CallListUpdateOverNetwork()
    {
        StartCoroutine(UpdateDetailsOnDelay());
    }

    private IEnumerator UpdateDetailsOnDelay()
    {
        Debug.Log("UpdatePlayerListOnLobby() called");
        if (lobbyHasLoadedBefore)
        {
            Debug.Log("Has loaded lobby list before, clearing");
            UIPlayerEntry[] allPlayerListObjs = FindObjectsOfType<UIPlayerEntry>();

            if (PhotonNetwork.IsMasterClient)
            {
                foreach (var obj in allPlayerListObjs)
                {
                    PhotonNetwork.Destroy(obj.gameObject);
                    playerInfo.Clear();
                }
                
            }
            lobbyHasLoadedBefore = false;
        }

        if (lobbyContentContainer == null)
        {
            Debug.LogError("Something happened to lobbyContentContainer");
            yield break;
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var player = PhotonNetwork.PlayerList[i];

            // Check if the player entry already exists to avoid duplication
            if (playerInfo.Exists(p => p.thisPlayer != null && p.thisPlayer.ActorNumber == player.ActorNumber))
            {
                Debug.Log($"Player {player.NickName} already exists in the list.");
                continue;
            }

            // Only instantiate if the player doesn't have an existing entry
            Vector3 pos = lobbyContentContainer.position;

            // Instantiate the player entry using PhotonNetwork.Instantiate
            var player_obj = PhotonNetwork.Instantiate("PlayerEntry", pos, Quaternion.identity);

            // Set the player data
            player_obj.GetComponent<UIPlayerEntry>().thisPlayer = player;

            // Set the parent and configure UI details
            player_obj.transform.SetParent(lobbyContentContainer.transform, false);
            player_obj.GetComponent<UIPlayerEntry>().playerNameText.text = player.NickName;
            player_obj.GetComponent<UIPlayerEntry>().playerPingText.text = PhotonNetwork.GetPing() + "ms";
            player_obj.transform.localScale = Vector2.one;

            // Add the player info to the list, avoiding duplicates
            playerInfo.Add(player_obj.GetComponent<UIPlayerEntry>());

            yield return new WaitForSeconds(0.5f); // Optional delay to handle UI instantiation smoothly
        }

        // After instantiation, update the list's appearance
        ApplyCharacter();
        UpdateRoomPlayerListAppearance();
        lobbyHasLoadedBefore = true;
        
        photonView.RPC("FixTheUIPlease", RpcTarget.Others);
        
        //photonView.RPC("UpdateRoomPlayerListAppearance", RpcTarget.AllViaServer);
        //photonView.RPC("UpdatePlayerInfo", RpcTarget.All);
    }

    public void UpdatePlayerListOnLobby()
    {
        photonView.RPC("UpdateListOverNetwork", RpcTarget.MasterClient);
    }

    [PunRPC]
    private void UpdateListOverNetwork()
    {
        StartCoroutine(UpdateDetailsOnDelay());
    }


    [PunRPC]
    private void FixTheUIPlease()
    {
        UIPlayerEntry[] plrs = FindObjectsOfType<UIPlayerEntry>();
        foreach (var current in plrs)
        {
            playerInfo.Add(current);
        }


        /*for (int i = 0; i < plrs.Length; i++)
        {
            plrs[i].playerNameText.text = PhotonNetwork.PlayerList[i].NickName;
            plrs[i].playerPingText.text = PhotonNetwork.GetPing() + "ms";
            plrs[i].thisPlayer = PhotonNetwork.PlayerList[i];
            plrs[i].transform.SetParent(lobbyContentContainer, false);
        } */

    }


    public void UpdateTimeLimit()
    {
        roundTime = Convert.ToInt32(timeLimitInput.text);
    }

    public void UpdateCollectable()
    {
        mcguffinAmount = Convert.ToInt32(collectableAmountInput.text);
    }

    public void UpdateLives()
    {
        lobbyLifeAmount = Convert.ToInt32(lifeAmountInput.text);
    }

    public void UpdateWinAmount()
    {
        lobbyWinAmount = Convert.ToInt32(winAmountInput.text);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }

    // display or hide character selection popup
    public void ToggleCharacterMenu(bool toggle)
    {
        charSelectMenu.SetActive(toggle);
    }

    // upon pressing one of the buttons representing a character
    public void PressCharacterButton(int charID)
    {
        ResetCharacterObjectRender();
        Character thisChar = null;
        // get character
        foreach (var current in allCharacters)
        {
            if (current.ID == charID)
            {
                thisChar = current;
            }
        }

        thisChar.charObject.SetActive(true);
        charNameText.text = thisChar.charName;
        localCharID = thisChar.ID;
    }

    // clear out the character being rendered so that a different one can become active
    private void ResetCharacterObjectRender()
    {
        for (int i = 0; i < charModels.Count; i++)
        {
            charModels[i].SetActive(false);
        } 
    }

    public void ApplyCharacter()
    {
        charSelectMenu.SetActive(false);

        ExitGames.Client.Photon.Hashtable playerTable = new ExitGames.Client.Photon.Hashtable()
        {
            { "Character", localCharID }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerTable);
    }

    // skibidi
    private void UpdateRoomPlayerListAppearance()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            // Log to check if the player entries exist correctly
            Debug.Log($"Updating player list for player {player.ActorNumber}");

            foreach (var info in playerInfo)
            {
                // Check if the player info entry is valid and has been initialized properly
                if (info == null || info.thisPlayer == null)
                {
                    Debug.LogWarning($"Skipping player entry as it is not initialized properly. ActorNumber: {player.ActorNumber}");
                    continue;  // Skip any invalid or uninitialized playerInfo entries
                }

                // Proceed with updating only if the player entry is correctly initialized
                if (info.thisPlayer.ActorNumber == player.ActorNumber)
                {
                    Debug.Log($"Updating UI for player {player.NickName}, ActorNumber: {player.ActorNumber}");

                    // check if spectator
                    if (player.CustomProperties.TryGetValue("IsSpectator", out object isSpectator))
                    {
                        if (!(bool)isSpectator)
                        {
                            // Update the character icon based on the player's chosen character
                            if (player.CustomProperties.TryGetValue("Character", out object characterID))
                            {
                                info.GetComponentInChildren<Animator>().SetInteger("Char", Convert.ToInt32(characterID));
                            }
                        }
                        else
                        {
                            info.GetComponentInChildren<Animator>().SetBool("IsSpectator", true);
                        }
                    }
                    
                }
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Character"))
        {
            UpdateRoomPlayerListAppearance();
        }
    }

    // Called when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} has left the room.");

        // Remove the player from the playerInfo list and update the UI
        for (int i = 0; i < playerInfo.Count; i++)
        {
            if (playerInfo[i].thisPlayer.ActorNumber == otherPlayer.ActorNumber)
            {
                Destroy(playerInfo[i].gameObject); // Destroy the player UI entry
                playerInfo.RemoveAt(i);            // Remove the entry from the list
                break;
            }
        }

        // Update the UI after removing the player
        UpdateRoomPlayerListAppearance();
    }

    // Called when the master client leaves the room
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master client has left. New master client: {newMasterClient.NickName}");

        // If the local player was the original master client, disband the room
        if (!PhotonNetwork.IsMasterClient) return;


        PhotonNetwork.LeaveRoom();
        lobbyHasLoadedBefore = false;
        playerInfo.Clear();
        /*
        if (newMasterClient == null)
        {
            Debug.Log("Original master client left, disbanding the room.");
            PhotonNetwork.LeaveRoom();
            
        } */
    }

}
