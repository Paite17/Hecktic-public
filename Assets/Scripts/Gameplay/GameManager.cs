using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using MrLewisPaite.JSONSave;
using System.IO;
using UnityEngine.UI;
using System;
using System.Xml;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager Instance;

    [SerializeField] private MapData currentMapData;

    // TODO: All the games settings from the lobby screen, and making sure they reflect what was input by the host

    [SerializeField] private bool debugMode;

    [SerializeField] private float mcguffinSpawnTimerLength;
    [SerializeField] private float mcguffinSpawnTimer;

    private bool gameStarted;
    [SerializeField] private bool mcguffinSpawned;
    [SerializeField] private float playerCheckTimerLength;

    private bool gameTimer;

    private bool hasTimeLimit;

    private float roundTimer;

    private bool hasEnded;


    private PlayerStatus[] allPlayerStats;

    // TODO: add lists for item boxes and enemy placements soon
    private BreakableBrick[] allBricks;
    private CoinRef[] allCoins;
    private PowerupBlock[] allPowerupBlocks;
    private EnemySpawnPoint[] allEnemySpawns;
    //private InteractableTiles tileInteractive;

    [SerializeField] private List<GameObject> powerupPool;

    // the lobby screen's customisable parameters
    [SerializeField] private float timeLimit;    // if 0 then infinite time
    [SerializeField] private float maxMcGuffinCount;   // the max amount of mcguffins a player needs to win, should never be 0
    [SerializeField] private float maxLiveCount;  // set player lives during intro, if 0 then a bool on the player should be set instead
    [SerializeField] private float maxNumberOfWins;

    [Header("Endgame UI")]
    [SerializeField] private TMP_Text winText;
    [SerializeField] private GameObject hostOptions;
    [SerializeField] private GameObject waitingForHostLabel;
    [SerializeField] private GameObject levellingObj;
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject resettingScreen;
    [SerializeField] private TMP_Dropdown mapDropdown;

    [Header("Misc")]
    [SerializeField] private GameObject waitForPlayersScreen;
    [SerializeField] private GameObject musicCreditText;
    [SerializeField] private bool pickleQuestActive;


    // leveling
    private PlayerLevelling playerLevel;

    private string path;

    private bool balls;

    private bool bothPlayersReady;

    private float playerCheckTimer;

    private int deadPlayers;

    private bool playerIsSpectator;

    [SerializeField] private List<Player> spectatorPlayerList;   // a list of the players that excludes the spectator themself

    private Player spec_currentPlayerTarget; // the player that is currently being spectated

    private CameraFollow spectatorCamera;

    public MapData CurrentMapData
    {
        get { return currentMapData; }
    }

    public bool McguffinSpawned
    {
        get { return mcguffinSpawned; }
        set { mcguffinSpawned = value; }
    }

    public float TimeLimit
    {
        get { return timeLimit; }
    }

    public float MaxMcGuffinCount
    {
        get { return maxMcGuffinCount; }
    }

    public float MaxLiveCount
    {
        get { return maxLiveCount; }
    }

    public float RoundTimer
    {
        get { return roundTimer; }
    }

    public bool HasTimeLimit
    {
        get { return hasTimeLimit; }
    }

    public bool PickleQuestActive
    {
        get { return pickleQuestActive; }
        set { pickleQuestActive = value; }
    }

    public bool PlayerIsSpectator
    {
        get { return playerIsSpectator; }
    }

    public Player spec_CurrentPlayerTarget
    {
        get { return spec_currentPlayerTarget; }
    }

    // Start is called before the first frame update
    void Start()
    {
        spectatorPlayerList = new List<Player>();
        spectatorCamera = Camera.main.GetComponent<CameraFollow>();
        if (debugMode)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom("Debug");
            ExitGames.Client.Photon.Hashtable debugHash = new ExitGames.Client.Photon.Hashtable { { "isPlayer1", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(debugHash);
        }

        path = Application.persistentDataPath + "/playerLevelData.json";
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
        mcguffinSpawned = true;

        if (!debugMode)
        {
            FindObjectOfType<AudioManager>().StopAllMusic();

            FindObjectOfType<AudioManager>().Play("Connecting");
        }

        Instance = this;
        mcguffinSpawnTimerLength = UnityEngine.Random.Range(15, 30);

        allBricks = FindObjectsOfType<BreakableBrick>();
        allCoins = FindObjectsOfType<CoinRef>();
        allPowerupBlocks = FindObjectsOfType<PowerupBlock>();
        allEnemySpawns = FindObjectsOfType<EnemySpawnPoint>();
        //tileInteractive = FindObjectOfType<InteractableTiles>();



        if (!PhotonNetwork.OfflineMode)
        {
            foreach (var current in PhotonNetwork.PlayerList)
            {
                if (current.CustomProperties.TryGetValue("IsSpectator", out object isSpectator))
                {
                    Debug.Log("PLAYER IsSpectator = " + (bool)isSpectator);
                    if (!(bool)isSpectator)
                    {
                        spectatorPlayerList.Add(current);
                    }
                }
            }

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerNumber"))
            {
                int isP1 = (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerNumber"];

                // spectator check
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsSpectator", out object isSpectator))
                {
                    if (!(bool)isSpectator)
                    {
                        // WHEN THE PLAYER IS NOT A SPECTATOR
                        Debug.LogWarning("isP1 = " + isP1);
                        switch (isP1)
                        {
                            case 1:
                                GameObject temp = null;
                                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Character", out object p1Character))
                                {
                                    string charName = Character.GetCharacterName(Convert.ToInt32(p1Character));
                                    temp = PhotonNetwork.Instantiate(charName + "Player", new Vector3(currentMapData.Player1SpawnX, -5, 0), Quaternion.identity);
                                    temp.GetComponent<PlayerStatus>().CharState = Character.GetCharacterState(Convert.ToInt32(p1Character));
                                    Debug.LogWarning("P1 CHAR ID = " + Convert.ToInt32(p1Character));
                                    FindObjectOfType<PUN2_Chat>().GetLocalPlayerForChat();
                                }
                                break;
                            case 2:
                                GameObject temp2 = null;
                                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Character", out object p2Character))
                                {
                                    string charName = Character.GetCharacterName(Convert.ToInt32(p2Character));
                                    Debug.LogWarning("P2 CHAR ID = " + Convert.ToInt32(p2Character));
                                    temp2 = PhotonNetwork.Instantiate(charName + "Player", new Vector3(currentMapData.Player2SpawnX, -5, 0), Quaternion.identity);
                                    temp2.GetComponent<PlayerStatus>().CharState = Character.GetCharacterState(Convert.ToInt32(p2Character));
                                    FindObjectOfType<PUN2_Chat>().GetLocalPlayerForChat();
                                }
                                break;
                            case 3:
                                GameObject temp3 = null;
                                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Character", out object p3Character))
                                {
                                    string charName = Character.GetCharacterName(Convert.ToInt32(p3Character));
                                    Debug.LogWarning("P2 CHAR ID = " + Convert.ToInt32(p3Character));
                                    temp3 = PhotonNetwork.Instantiate(charName + "Player", new Vector3(currentMapData.Player3SpawnX, -5, 0), Quaternion.identity);
                                    temp3.GetComponent<PlayerStatus>().CharState = Character.GetCharacterState(Convert.ToInt32(p3Character));
                                    FindObjectOfType<PUN2_Chat>().GetLocalPlayerForChat();
                                }
                                break;
                            case 4:
                                GameObject temp4 = null;
                                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Character", out object p4Character))
                                {
                                    string charName = Character.GetCharacterName(Convert.ToInt32(p4Character));
                                    Debug.LogWarning("P2 CHAR ID = " + Convert.ToInt32(p4Character));
                                    temp4 = PhotonNetwork.Instantiate(charName + "Player", new Vector3(currentMapData.Player3SpawnX, -5, 0), Quaternion.identity);
                                    temp4.GetComponent<PlayerStatus>().CharState = Character.GetCharacterState(Convert.ToInt32(p4Character));
                                    FindObjectOfType<PUN2_Chat>().GetLocalPlayerForChat();
                                }
                                break;
                        }
                    }
                    else
                    {
                        // WHEN THE PLAYER IS A SPECTATOR
                        playerIsSpectator = true;

                        // make the default spectated player the first in the list so that nothing breaks;
                        Debug.Log("SPECTATOR TEST: " + spectatorPlayerList[0].NickName);
                        spec_currentPlayerTarget = spectatorPlayerList[0];
                        UIScript.Instance.SetSpectatorUI();
                        spectatorCamera.GetComponent<CameraFollow>().hasConnected = true;
                        
                    }
                }
                
            }   
        }
        else
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Character", out object p1Character))
            {
                GameObject tempDef = null;
                string charName = Character.GetCharacterName(Convert.ToInt32(p1Character));
                tempDef = PhotonNetwork.Instantiate(charName + "Player", new Vector3(currentMapData.Player1SpawnX, -5, 0), Quaternion.identity);
                tempDef.GetComponent<PlayerStatus>().CharState = Character.GetCharacterState(Convert.ToInt32(p1Character));
            }  
            else
            {
                GameObject tempDef = PhotonNetwork.Instantiate("LewPlayer", new Vector3(currentMapData.Player1SpawnX, -5, 0), Quaternion.identity);
                tempDef.GetComponent<PlayerStatus>().CharState = PlayableCharacterState.LEW;
            }           
        }

        GetLobbySettings();
    }

    // Update is called once per frame
    void Update()
    {

        if (!bothPlayersReady)
        {
            playerCheckTimer += Time.deltaTime;
        }


        if (playerCheckTimer > playerCheckTimerLength)
        {
            CheckForPlayers();
            playerCheckTimer = 0;
        }

        if (gameStarted && !mcguffinSpawned)
        {
            mcguffinSpawnTimer += Time.deltaTime;
        }

        if (mcguffinSpawnTimer >= mcguffinSpawnTimerLength)
        {

            if (PhotonNetwork.IsMasterClient)
            {
                SpawnNewMcGuffin();
            }
        }

        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    SpawnNewMcGuffin();
                }
            }
        }

        if (gameStarted)
        {
            if (hasTimeLimit)
            {
                roundTimer -= Time.deltaTime;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                CheckWin();
            }

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsSpectator", out object IsSpectator))
            {
                if ((bool)IsSpectator)
                {
                    // i cannot imagine that this is in any way efficient, but i am banking on the fact that this game just isn't complicated for it to work fine
                    spectatorCamera.target = GetPlayerTransform(spec_CurrentPlayerTarget);
                }               
            }
        }

    }

    public override void OnLeftRoom()
    {
        //SceneManager.LoadScene(0);
    }

    // 
    private Transform GetPlayerTransform(Player thisPlayer)
    {
        PlayerMovement[] allPlrMove = FindObjectsOfType<PlayerMovement>();

        foreach (var current in allPlrMove)
        {
            if (current.photonView.Owner == thisPlayer)
            {
                return current.transform;
            }
        }

        // use plr 1 as a fallback
        return allPlrMove[0].transform;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.CustomProperties.TryGetValue("IsSpectator", out object isSpectator))
        {
            if (!(bool)isSpectator)
            {
                if (!hasEnded)
                {
                    // cancell the game cus someone left
                    Debug.LogWarning("SOMEONE LEFT AAAAAA");
                    //PhotonNetwork.Disconnect();
                    FindObjectOfType<AudioManager>().StopAllMusic();
                    SceneManager.LoadScene("CommunicationError");
                }
                else
                {
                    FindObjectOfType<AudioManager>().StopAllMusic();
                    PhotonNetwork.LeaveRoom(false);
                    PhotonNetwork.LeaveLobby();
                    PhotonNetwork.Disconnect();
                    SceneManager.LoadScene("MainMenu");
                }
            }
            else
            {
                // TODO: a popup that notifies the other players that the spectator left
                UIScript.Instance.SpectatorLeaveNotifDisplay(otherPlayer);
            }
        }
        
        
    }

    private IEnumerator RoundBeginningSequence()
    {
        FindObjectOfType<PUN2_Chat>().GetLocalPlayerForChat();
        waitForPlayersScreen.SetActive(false);     
        
        // funky animations then spawn the players
        FindObjectOfType<AudioManager>().Play("GameStart");
        UIScript.Instance.StartIntro();

        GetLobbySettings();  // will probably break LMAO

        //SetLobbySettings();
        // set time limit
        if (timeLimit > 0)
        {
            hasTimeLimit = true;
            roundTimer = timeLimit;
        }
        PlayerMovement[] tempList = FindObjectsOfType<PlayerMovement>();

        if (maxLiveCount > 0)
        {
            tempList[0].Status.AmountOfLives = maxLiveCount;
            tempList[0].Status.InfiniteLives = false;
        }
        else
        {
            tempList[0].Status.InfiniteLives = true;
        }

        if (!debugMode)
        {
            
            if (maxLiveCount > 0)
            {
                tempList[1].Status.AmountOfLives = maxLiveCount;
                tempList[1].Status.InfiniteLives = false;
            }
            else
            {
                tempList[0].Status.InfiniteLives = true;
                tempList[1].Status.InfiniteLives = true;
            }

            /*
            if (PhotonNetwork.CurrentRoom.PlayerCount > 2)
            {
                tempList[2].Status.AmountOfLives = maxLiveCount;
                tempList[2].Status.InfiniteLives = false;

                if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
                {
                    tempList[3].Status.AmountOfLives = maxLiveCount;
                    tempList[3].Status.InfiniteLives = false;
                }
            } */
        }
        yield return new WaitForSeconds(1.35f);
        

        tempList[0].transform.position = new Vector3(tempList[0].transform.position.x, currentMapData.Player1SpawnY, tempList[0].transform.position.z);
        tempList[0].GetComponent<Rigidbody2D>().velocity = Vector3.zero;        
        
        if (!debugMode)
        {
            tempList[1].transform.position = new Vector3(tempList[1].transform.position.x, currentMapData.Player2SpawnY, tempList[1].transform.position.z);
            tempList[1].GetComponent<Rigidbody2D>().velocity = Vector3.zero;      
            
            /*
            if (PhotonNetwork.CurrentRoom.PlayerCount > 2)
            {
                tempList[2].transform.position = new Vector3(tempList[1].transform.position.x, currentMapData.Player3SpawnY, tempList[2].transform.position.z);
                tempList[2].GetComponent<Rigidbody2D>().velocity = Vector3.zero;

                if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
                {
                    tempList[3].transform.position = new Vector3(tempList[1].transform.position.x, currentMapData.Player4SpawnY, tempList[3].transform.position.z);
                    tempList[3].GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                }
            } */
        }

        tempList[0].CanMove = true;
        if (!PhotonNetwork.OfflineMode)
        {
            tempList[1].CanMove = true;
            /*
            if (PhotonNetwork.CurrentRoom.PlayerCount > 2)
            {
                tempList[2].CanMove = true;

                if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
                {
                    tempList[3].CanMove = true;
                }
            } */
        }
        

        allPlayerStats = FindObjectsOfType<PlayerStatus>();
        if (PhotonNetwork.IsMasterClient)
        {
            bool funky = false;
            if (!funky)
            {
                InitialEnemySpawn();
                funky = true;
            }          
            PhotonNetwork.InstantiateRoomObject("McGuffin", currentMapData.McguffinSpawnLocations[0], Quaternion.identity);
            photonView.RPC("McGuffinSpawnAlert", RpcTarget.All);            
            photonView.RPC("BeginMultiplayerMusic", RpcTarget.All);
            //yield return new WaitForSeconds(0.4f);
        }

        gameStarted = true;
        UIScript.Instance.GetPlayers();

    }

    // to be called in the new 'Waiting for Players' phase, needs to get called every few seconds until the condition is met
    private void CheckForPlayers()
    {
        if (!bothPlayersReady)
        {
            if (!PhotonNetwork.OfflineMode)
            {
                PlayerMovement[] allPlrs = FindObjectsOfType<PlayerMovement>();


                if (allPlrs.Length >= spectatorPlayerList.Count)
                {
                    bothPlayersReady = true;
                    FindObjectOfType<AudioManager>().StopMusic("Connecting");
                    GetLobbySettings();

                    if (PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("StartGameForAll", RpcTarget.AllBufferedViaServer);
                    }
                }
                else
                {
                    Debug.Log("Not all players are in yet...");
                }
            }
            else
            {
                bothPlayersReady = true;
                FindObjectOfType<AudioManager>().StopMusic("Connecting");
                

                photonView.RPC("StartGameForAll", RpcTarget.AllBufferedViaServer);
            }
            
        }
    }

    // hope it works this time
    private void GetLobbySettings()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("RoundTime", out object matchTime))
        {
            timeLimit = (float)System.Convert.ToDouble(matchTime);
            Debug.Log("matchTime = " + matchTime);
            
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("McGuffinAmount", out object mcguffins))
        {
            
            maxMcGuffinCount = (float)System.Convert.ToDouble(mcguffins);
            if (maxMcGuffinCount < 1)
            {
                maxMcGuffinCount = 1;
            }
            Debug.Log("mcguffins = " + mcguffins);
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("WinAmount", out object winAmount))
        {
            maxNumberOfWins = (float)System.Convert.ToDouble(winAmount);
            Debug.Log("winAmount = " + winAmount);
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("LifeAmount", out object lives))
        {
            maxLiveCount = (float)System.Convert.ToDouble(lives);
            Debug.Log("lives = " + lives);
        }
    }

    public void SpawnNewMcGuffin()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            mcguffinSpawned = true;
            GameObject temp = PhotonNetwork.InstantiateRoomObject("McGuffin", currentMapData.McguffinSpawnLocations[UnityEngine.Random.Range(0, currentMapData.McguffinSpawnLocations.Count)], Quaternion.identity);
            temp.GetComponent<McGuffin>().Type = McGuffinType.FRESH;
            mcguffinSpawnTimer = 0;
            mcguffinSpawnTimerLength = UnityEngine.Random.Range(15, 20);
        }

        photonView.RPC("McGuffinSpawnAlert", RpcTarget.All);
    }

    [PunRPC]
    private void StartGameForAll()
    {
        StartCoroutine(RoundBeginningSequence());
    }


    [PunRPC]
    public void McGuffinSpawnAlert()
    {
        // TODO: UI?
        FindObjectOfType<AudioManager>().Play("McGuffinSpawned");
    }

    // basically what gets done in ResetMapData but only the enemies
    private void InitialEnemySpawn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var obj in allEnemySpawns)
            {
                if (obj != null)
                {
                    GameObject temp = PhotonNetwork.InstantiateRoomObject(obj.EnemyToSpawn.name, obj.transform.position, Quaternion.identity);
                    temp.GetComponent<Enemy>().originalSpawnPoint = obj;
                    obj.hasSpawned = true;
                }
            }
        }
        
    }

    // for resseting things like broken blocks, coin positions, powerup blocks and enemies
    public void ResetMapData(bool moment)
    {
        if (!moment)
        {
            mcguffinSpawned = false;

            // reset all interactable elements
            // TODO: code for the rest of the resettable objects
            foreach (var obj in allBricks)
            {
                obj.gameObject.SetActive(true);
                obj.GetComponent<Collider2D>().enabled = true;
            }

            foreach (var obj in allCoins)
            {
                obj.gameObject.SetActive(true);
            }

            foreach (var obj in allPowerupBlocks)
            {
                obj.GetComponent<PowerupBlock>().HasBeenUsed = false;
                obj.GetComponent<PowerupBlock>().bruh = false;
                obj.gameObject.SetActive(true);
                obj.GetComponent<Collider2D>().enabled = true;

                if (obj.GetComponent<PowerupBlock>().IsHidden)
                {
                    obj.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
                    GetComponent<BoxCollider2D>().isTrigger = true;
                }
            }

            foreach (var obj in allEnemySpawns)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (!obj.hasSpawned)
                    {
                        GameObject temp = PhotonNetwork.InstantiateRoomObject(obj.EnemyToSpawn.name, obj.transform.position, Quaternion.identity);
                        temp.GetComponent<Enemy>().originalSpawnPoint = obj;
                        obj.hasSpawned = true;
                    }
                }              
                
            }


            //tileInteractive.Tilemap = tileInteractive.Replacement;
        }
        else
        {
            return;
        }
       
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

        }
        else
        {

        }
    }

    // start da music
    [PunRPC]
    public void BeginMultiplayerMusic()
    {
        int random = UnityEngine.Random.Range(1, 3);
        Debug.LogWarning("random = " + random);
        FindObjectOfType<AudioManager>().Play("Overworld" +  random);

        musicCreditText.SetActive(true);
        switch (random)
        {
            case 1:
                musicCreditText.GetComponent<TMP_Text>().text = "Now Playing: Bryngles - Alfie Bacon";
                break;
            case 2:
                musicCreditText.GetComponent<TMP_Text>().text = "Now Playing: Livin it Up - Alfie Bacon";
                break;
            case 3:
                musicCreditText.GetComponent<TMP_Text>().text = "Now Playing: Boss - Alfie Bacon";
                break;
        }
    }

    [PunRPC]
    // for when the player collects 8 coins
    public void SpawnPowerup(float progress, int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);

        if (view.IsMine)
        {
            FindObjectOfType<AudioManager>().Play("PowerupSpawn");
        }
        

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(PowerupSuspension(view, progress));
        }
        
              
    }

    private IEnumerator PowerupSuspension(PhotonView plrFollow, float progress)
    {
        // offset and spawnPos for powerup
        Vector3 offset = new Vector3(0, 8.5f, 0);
        Vector3 spawnPos = plrFollow.transform.position + offset;
        // selecting from pool
        float rarityMultiplier = Mathf.Lerp(0.5f, 1.0f, progress);

        // create weighted list
        List<GameObject> weightedPowerups = new List<GameObject>();

        foreach (var powerup in powerupPool)
        {
            int weight = Mathf.RoundToInt(powerup.GetComponent<Powerup>().SpawnProbability * rarityMultiplier * 100);

            for (int i = 0; i < weight; i++)
            {
                weightedPowerups.Add(powerup);
            }
        }

        // select from the weighted list
        int randomIndex = UnityEngine.Random.Range(0, weightedPowerups.Count);

        GameObject temp = PhotonNetwork.Instantiate(weightedPowerups[randomIndex].name, offset, Quaternion.identity);
        temp.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
        temp.GetComponent<Powerup>().SpawnedFromCoins = true;
        temp.GetComponent<Powerup>().Target = plrFollow.transform;
        temp.GetComponent<Collider2D>().enabled = false;
        temp.GetComponent<Rigidbody2D>().gravityScale = 0;

        photonView.RPC("SyncPowerup", RpcTarget.Others, temp.GetComponent<PhotonView>().ViewID, plrFollow.ViewID);

        yield return new WaitForSeconds(1.2f);

        if (temp != null)
        {
            temp.GetComponent<Collider2D>().enabled = true;
            temp.GetComponent<Rigidbody2D>().gravityScale = 2.3f;
            temp.GetComponent<Powerup>().SpawnedFromCoins = false;
        }       
        
    }

    [PunRPC]
    private void SyncPowerup(int viewID, int targetID)
    {
        PhotonView powerupView = PhotonView.Find(viewID);
        PhotonView targetView = PhotonView.Find(targetID);
        if (powerupView != null )
        {
            Powerup powerup = powerupView.GetComponent<Powerup>();
            powerup.GetComponent<Collider2D>().enabled = true;
            powerup.GetComponent<Rigidbody2D>().gravityScale = 2.3f;
            powerup.SpawnedFromCoins = false;
            powerup.Target = targetView.transform;
        }
        
    }

    private PhotonView FindAlivePlayer()
    {
        foreach (var current in allPlayerStats)
        {
            if (current.AmountOfLives > 0)
            {
                return current.photonView;
            }
        }

        return null;
    }

    private void CheckWin()
    {
        foreach (var current in allPlayerStats)
        {
            if (current.AmountOfMcGuffins >= maxMcGuffinCount)
            {
                hasEnded = true;
                Debug.LogWarning("MCGUFFIN WIN");
                photonView.RPC("EndRound", RpcTarget.All, current.photonView.ViewID, 0, "objective");
            }

            if (current.AmountOfLives <= 0)
            {
                if (!allPlayerStats[0].InfiniteLives)
                {
                    Debug.LogWarning("LIVES WIN");
                    hasEnded = true;
                    photonView.RPC("EndRound", RpcTarget.All, FindAlivePlayer().ViewID, 0, "Death");
                }
                /*
                if (PhotonNetwork.CurrentRoom.PlayerCount < 3)
                {
                    if (!allPlayerStats[0].InfiniteLives)
                    {
                        Debug.LogWarning("LIVES WIN");
                        hasEnded = true;
                        photonView.RPC("EndRound", RpcTarget.All, FindAlivePlayer().ViewID, 0, "Death");
                    }
                }
                else
                {
                    if (deadPlayers == PhotonNetwork.CurrentRoom.PlayerCount - 1)
                    {
                        Debug.LogWarning("LIVES WIN");
                        hasEnded = true;
                        photonView.RPC("EndRound", RpcTarget.All, FindAlivePlayer().ViewID, 0, "Death");
                    }
                } */
               
            }
        }

        if (roundTimer <= 0 && !hasEnded && hasTimeLimit)
        {
            hasEnded = true;
            // TODO: pretty sure this wont work lol
            Debug.LogWarning("TIMER WIN");
            if (CheckWhoWonViaTimeout() != null)
            {
                photonView.RPC("EndRound", RpcTarget.All, CheckWhoWonViaTimeout().ViewID, 0, "Timeout");
            }
            else
            {
                photonView.RPC("EndRound", RpcTarget.All, 0, 0, "Draw");
            }

        }
    }

    // gets the PhotonView of whoever wins via timeout (running out of time in a round)
    // TODO: handle a draw
    private PhotonView CheckWhoWonViaTimeout()
    {
        PlayerStatus[] statussy = FindObjectsOfType<PlayerStatus>();
        // TODO: find a better fuckin way of finding who has the most mcguffins cus this is diabolical
        if (statussy == null || statussy.Length == 0)
        {
            return null;
        }

        PlayerStatus max = statussy[0];

        for (int i = 1;  i < statussy.Length; i++)
        {
            if (statussy[i].AmountOfMcGuffins > max.AmountOfMcGuffins)
            {
                max = statussy[i];
            }
        }

        if (max.AmountOfMcGuffins > 0)
        {
            return max.photonView;
        }
        else
        {
            return null;
        }
       
    }

    // the reverse of the function above, because skibidi
    private PhotonView CheckWhoLostViaTimeout()
    {
        PlayerStatus[] statussy = FindObjectsOfType<PlayerStatus>();

        if (statussy[0].AmountOfMcGuffins > statussy[1].AmountOfMcGuffins)
        {
            return statussy[1].photonView;
        }
        else if (statussy[0].AmountOfMcGuffins < statussy[1].AmountOfMcGuffins)
        {
            return statussy[0].photonView;
        }
        else
        {
            return null;
        }
    }

    // will most likely need an argument of which player won
    [PunRPC]
    public void EndRound(int winningID, int losingID, string reason)
    {
        Debug.LogWarning(reason);
        Rigidbody2D[] allRB = FindObjectsOfType<Rigidbody2D>();
        foreach (var current in allRB)
        {
            current.simulated = false;
        }
        
        // TODO: UI stuff
        StartCoroutine(EndRoundEvents(winningID, losingID));
    }


    private IEnumerator EndRoundEvents(int winningID, int losingID)
    {
        if (!balls)
        {
            hasEnded = true;
            gameStarted = false;
            balls = true;
            Debug.LogWarning("START COROUTINE");            
            PhotonView winnerView = PhotonView.Find(winningID);
            PhotonView loserView = PhotonView.Find(losingID);
            FindObjectOfType<AudioManager>().StopMusic("Invincibility");
            FindObjectOfType<AudioManager>().StopMusic("Overworld1");
            FindObjectOfType<AudioManager>().StopMusic("Overworld2");
            yield return new WaitForSeconds(1f);
            
            if (winnerView != null)
            {
                if (winnerView.IsMine)
                {
                    FindObjectOfType<AudioManager>().Play("MatchWin");
                }
                else
                {
                    FindObjectOfType<AudioManager>().Play("MatchLose");
                }
            }
            else
            {
                // assume the match is a draw
                FindObjectOfType<AudioManager>().Play("MatchDraw");
            }
            

            winText.gameObject.SetActive(true);
            // this will need to be changed when character selecting is working
            if (winnerView != null)
            {
                switch (winnerView.GetComponent<PlayerStatus>().CharState)
                {
                    case PlayableCharacterState.LEW:
                        winText.color = Color.red;
                        break;
                    case PlayableCharacterState.BRYNGLES:
                        winText.color = Color.yellow;
                        break;
                    case PlayableCharacterState.CHUM:
                        winText.color = Color.white;
                        break;
                    case PlayableCharacterState.KING_CHOD:
                        winText.color = new Color32(0, 145, 141, 255);
                        break;
                }

                if (!PhotonNetwork.OfflineMode)
                {
                    winText.text = winnerView.Owner.NickName + " Wins!";
                }
                else
                {
                    winText.text = "Player Wins!";
                }
            }
            else
            {
                winText.color = Color.blue;
                winText.text = "Draw!";
            }
            
            

            yield return new WaitForSeconds(3f);

            if (PhotonNetwork.IsMasterClient)
            {
                hostOptions.SetActive(true);
            }
            else
            {
                waitingForHostLabel.SetActive(true);
            }


            // level gain
            if (!PhotonNetwork.OfflineMode)
            {
                float gainedExp = 0f;

                levellingObj.SetActive(true);
                levelLabel.text = "Level " + playerLevel.CurrentLevel;
                float currentFillAmount = (float)playerLevel.CurrentEXP / playerLevel.EXPToNext;
                progressBar.fillAmount = currentFillAmount;

                if (winnerView != null)
                {
                    float biggieCheese = (winnerView.GetComponent<PlayerStatus>().AmountOfMcGuffins / maxMcGuffinCount * 100);
                    if (biggieCheese <= 0)
                    {
                        biggieCheese = 1;
                    }

                    if (winnerView.IsMine)
                    {                        
                        gainedExp = (biggieCheese * 50) * 2;
                    }
                    else
                    {
                        gainedExp = (biggieCheese * 50);
                    }
                }
                else
                {
                    gainedExp = 0f;
                }
                

                yield return new WaitForSeconds(1.5f);

                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsSpectator", out object isSpectator))
                {
                    if (!(bool)isSpectator)
                    {
                        playerLevel.CurrentEXP += gainedExp;
                    }
                }              

                bool brug = false;
                if (playerLevel.CurrentEXP > playerLevel.EXPToNext)
                {
                    if (!brug)
                    {
                        playerLevel.CurrentLevel++;
                        playerLevel.EXPToNext += (playerLevel.CurrentEXP * 2);
                        brug = true;
                        levelLabel.text = "Level " + playerLevel.CurrentLevel;
                    }
                }

                float newFillAmount = (float)playerLevel.CurrentEXP / playerLevel.EXPToNext;
                progressBar.fillAmount = newFillAmount;

                JSONStorage.SaveDataToFile(playerLevel, path);
            }
            
        }
       
    }

    public void NextRound()
    {
        // if the map is the same then run the reset function, otherwise we can just load into the other scene
        if (!PhotonNetwork.OfflineMode)
        {
            if (mapDropdown.captionText.text == SceneManager.GetActiveScene().name)
            {
                photonView.RPC("ResetTheMatch", RpcTarget.All);
            }
            else
            {
                PhotonNetwork.LoadLevel(mapDropdown.captionText.text);
            }
        }
        else
        {
            photonView.RPC("ResetTheMatch", RpcTarget.All);
        }     
    }

    // called upon clicking the next button on the spectator UI
    public void SpectatorSetNextPlayer()
    {
        int p = spectatorPlayerList.IndexOf(spec_currentPlayerTarget);
        Debug.LogWarning("p = " + p);

        // if current is going to overflow, make it wrap back to the beginning of the list
        if ((p + 1) > spectatorPlayerList.Count)
        {
            p = 0;
        }
        else
        {
            p++;
        }

        spec_currentPlayerTarget = spectatorPlayerList[p];
    }

    // called upon clicking the previous button on the spectator UI
    public void SpectatorSetPreviousPlayer()
    {
        int p = spectatorPlayerList.IndexOf(spec_currentPlayerTarget);
        Debug.LogWarning("p = " + p);

        // same as the above function
        if ((p - 1) < 0)
        {
            p = spectatorPlayerList.Count;
        }
        else
        {
            p--;
        }

        spec_currentPlayerTarget = spectatorPlayerList[p];
    }


    // try to replicate the initial conditions of a match
    [PunRPC]
    private void ResetTheMatch()
    {
        balls = false;
        resettingScreen.SetActive(true);
        winText.gameObject.SetActive(false);
        hostOptions.SetActive(false);
        waitingForHostLabel.SetActive(false);
        levellingObj.SetActive(false);
        musicCreditText.SetActive(false);
        FindObjectOfType<AudioManager>().Play("Connecting");
        // put players back where they started
        
        
        for (int i = 0; i < allPlayerStats.Length; i++)
        {
            bool isP1 = (bool)allPlayerStats[i].photonView.Owner.CustomProperties["isPlayer1"];
            switch (isP1)
            {
                case true:
                    allPlayerStats[i].transform.position = new Vector3(currentMapData.Player1SpawnX, -5, allPlayerStats[i].transform.position.z);
                    break;
                case false:
                    allPlayerStats[i].transform.position = new Vector3(currentMapData.Player2SpawnX, -5, allPlayerStats[i].transform.position.z);
                    break;
            }         
        }

        

        for (int i = 0;  i < allPlayerStats.Length; i++)
        {
            allPlayerStats[i].AmountOfCoins = 0;
            allPlayerStats[i].AmountOfMcGuffins = 0;
            allPlayerStats[i].AmountOfLives = maxLiveCount;
            allPlayerStats[i].GetComponentInChildren<Animator>().SetBool("Death", false);
            allPlayerStats[i].GetComponent<PlayerMovement>().powerupState = CharPowerupState.REGULAR;
            allPlayerStats[i].GetComponent<PlayerMovement>().SetPowerupMaterial(0);
        }

        ResetMapData(false);

        mcguffinSpawned = true;
        mcguffinSpawnTimer = 0;
        roundTimer = timeLimit;
        hasTimeLimit = false;

        // kill enemies
        if (PhotonNetwork.IsMasterClient)
        {
            Enemy[] allEnemy = FindObjectsOfType<Enemy>();
            foreach (var current in allEnemy)
            {
                PhotonNetwork.Destroy(current.gameObject);
            }
        }

        // kill mcguffins
        if (PhotonNetwork.IsMasterClient)
        {
            McGuffin[] allMcguffins = FindObjectsOfType<McGuffin>();
            foreach (var current in allMcguffins)
            {
                PhotonNetwork.Destroy(current.gameObject);
            }
        }

        

        StartCoroutine(NewRoundStartSequence());
    }

    // like the initial starting sequence but we don't need to make the gameObjects this time
    private IEnumerator NewRoundStartSequence()
    {
        yield return new WaitForSeconds(3.5f);
        Rigidbody2D[] allRB = FindObjectsOfType<Rigidbody2D>();
        foreach (var current in allRB)
        {
            current.simulated = true;
        }
        hasEnded = false;
        resettingScreen.SetActive(false);
        FindObjectOfType<AudioManager>().StopMusic("Connecting");
        FindObjectOfType<AudioManager>().Play("GameStart");
        UIScript.Instance.StartIntro();

        yield return new WaitForSeconds(1.35f);

        PlayerMovement[] tempList = FindObjectsOfType<PlayerMovement>();

        if (timeLimit > 0)
        {
            hasTimeLimit = true;
            roundTimer = timeLimit;
        }



        tempList[0].transform.position = new Vector3(tempList[0].transform.position.x, currentMapData.Player1SpawnY, tempList[0].transform.position.z);
        tempList[0].GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        if (!PhotonNetwork.OfflineMode)
        {
            tempList[1].transform.position = new Vector3(tempList[1].transform.position.x, currentMapData.Player2SpawnY, tempList[1].transform.position.z);
            tempList[1].GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
        

        

        if (PhotonNetwork.IsMasterClient)
        {
            bool funky = false;
            if (!funky)
            {
                InitialEnemySpawn();
                funky = true;
            }
            PhotonNetwork.InstantiateRoomObject("McGuffin", currentMapData.McguffinSpawnLocations[0], Quaternion.identity);
            photonView.RPC("McGuffinSpawnAlert", RpcTarget.All);
            photonView.RPC("BeginMultiplayerMusic", RpcTarget.All);
            //yield return new WaitForSeconds(0.4f);
        }

        gameStarted = true;
    }


    public void EndgameQuit()
    {
        photonView.RPC("ByeBye", RpcTarget.All);
    }

    [PunRPC]
    private void ByeBye()
    {
        PhotonNetwork.LeaveRoom(false);
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }

    public void OfflineQuit()
    {
        FindObjectOfType<AudioManager>().StopAllMusic();
        SceneManager.LoadScene("MainMenu");
    }

}
