using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

// TODO: NEED TO MAKE SURE THAT currentPlayerStats IS SET AS LIKE, PLAYER 1 BY DEFAULT OR SOMETHING
public class UIScript : MonoBehaviourPunCallbacks
{
    public static UIScript Instance;

    [Header("Menu UI")]
    [SerializeField] private GameObject ingameMenuObj;
    
    [SerializeField] private List<Sprite> charIcons;
    [SerializeField] private GameObject canvas;

    [Header("In-GameUI")]
    // intro stuff
    [SerializeField] private GameObject readyText;
    [SerializeField] private GameObject goText;
    [SerializeField] private TMP_Text pingText;
    // player details
    // ok i changed my approach halfway through making this
    // objects with names like thisPlayer will refer to player 1, while objects named stuff like otherPlayer refer to player 2
    [Header("Player Status UI")]
    [SerializeField] private GameObject playerStatusHolder;
    [SerializeField] private TMP_Text thisPlayerLivesCount;
    [SerializeField] private TMP_Text otherPlayerLivesCount;
    [SerializeField] private TMP_Text thisPlayerCoinCount;
    [SerializeField] private TMP_Text otherPlayerCoinCount;
    [SerializeField] private TMP_Text thisPlayerMcGuffinCount;
    [SerializeField] private TMP_Text otherPlayerMcGuffinCount;
    [SerializeField] private Image currentPlayerLifeIcon;
    [SerializeField] private Image otherPlayerLifeIcon;

    [Header("Round UI")]
    [SerializeField] private TMP_Text timerLabel;

    [Header("Spectator UI")]
    [SerializeField] private GameObject spectatorUI;
    [SerializeField] private TMP_Text currentPlayerLabel;
    [SerializeField] private GameObject spectatorLeaveNotif;
    [SerializeField] private TMP_Text spectatorLeaveText;


    [Header("Misc")]
    [SerializeField] private PlayerStatus currentPlayerStats;
    [SerializeField] private PlayerStatus otherPlayerStats;
    [SerializeField] private PhotonView player1View;
    [SerializeField] private PhotonView player2View;

    private bool roundStarted;

    private bool menuActive;

    private Animator spectatorLeaveAnim;

    public bool MenuActive
    {
        get { return menuActive; } 
        set { menuActive = value; }
    }
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        canvas.SetActive(true);
        if (spectatorLeaveNotif != null)
        {
            spectatorLeaveAnim = spectatorLeaveNotif.GetComponent<Animator>();
        }
        

    }

    // getting the players and making references for them as 'this player' and 'other player'
    public void GetPlayers()
    {
        
        if (!PhotonNetwork.OfflineMode)
        {
            
            PlayerMovement[] allStatus = FindObjectsOfType<PlayerMovement>();

            // need to decide at some point if this is how i want the ui to work
            // (guarantees that the left side of the ui will be for Player 1, and the right side Player 2)
            /*
            foreach (PlayerMovement player in allStatus)
            {
                // Check player custom properties to see if they are Player 1 or Player 2
                if (player.photonView.Owner.CustomProperties.ContainsKey("isPlayer1"))
                {
                    bool isPlayer1 = (bool)player.photonView.Owner.CustomProperties["isPlayer1"];

                    if (isPlayer1)
                    {
                        player1View = player.photonView;
                    }
                    else
                    {
                        player2View = player.photonView;
                    }
                }
            } */

            // puts YOUR player on the left side of the ui and the opposing player on the right
            foreach (PlayerMovement player in allStatus)
            {
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsSpectator", out object isSpectator))
                {
                    if (!(bool)isSpectator)
                    {
                        if (player.photonView.Owner == PhotonNetwork.LocalPlayer)
                        {
                            player1View = player.photonView;
                        }
                        else
                        {
                            player2View = player.photonView;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Player is a spectator");
                        if (player.photonView.Owner.CustomProperties.TryGetValue("PlayerNumber", out object plrNum))
                        {
                            switch ((int)plrNum)
                            {
                                case 1:
                                    player1View = player.photonView;
                                    break;
                                case 2:
                                    player2View = player.photonView;
                                    break;
                            }
                        }
                    }
                }
               
                
            }


            if (player1View != null)
            {
                currentPlayerStats = player1View.GetComponent<PlayerStatus>();
            }
            if (player2View != null)
            {
                otherPlayerStats = player2View.GetComponent<PlayerStatus>();
            }


            playerStatusHolder.SetActive(true);
            UpdateLifeIcons();
            roundStarted = true; 



        }
        else
        {
            player1View = FindObjectOfType<PlayerMovement>().photonView;
            currentPlayerStats = player1View.GetComponent<PlayerStatus>();
            playerStatusHolder.SetActive(true);
            UpdateLifeIcons();
            roundStarted = true;
        }
    }

    public void SetSpectatorUI()
    {
        spectatorUI.SetActive(true);
    }

    private void UpdateLifeIcons()
    {
        // current player life icon
        switch (currentPlayerStats.CharState)
        {
            case PlayableCharacterState.LEW:
                currentPlayerLifeIcon.sprite = charIcons[0];
                break;
            case PlayableCharacterState.BRYNGLES:
                currentPlayerLifeIcon.sprite = charIcons[1];
                break;
            case PlayableCharacterState.CHUM:
                currentPlayerLifeIcon.sprite = charIcons[2];
                break;
            case PlayableCharacterState.KING_CHOD:
                currentPlayerLifeIcon.sprite = charIcons[3];
                break;
        }

        if (!PhotonNetwork.OfflineMode)
        {
            switch (otherPlayerStats.CharState)
            {
                case PlayableCharacterState.LEW:
                    otherPlayerLifeIcon.sprite = charIcons[0];
                    break;
                case PlayableCharacterState.BRYNGLES:
                    otherPlayerLifeIcon.sprite = charIcons[1];
                    break;
                case PlayableCharacterState.CHUM:
                    otherPlayerLifeIcon.sprite = charIcons[2];
                    break;
                case PlayableCharacterState.KING_CHOD:
                    otherPlayerLifeIcon.sprite = charIcons[3];
                    break;
            }
            
        }    
    }
    // Update is called once per frame
    void Update()
    {
        if (roundStarted)
        {
            // stats for this player
            thisPlayerCoinCount.text = ": " + currentPlayerStats.AmountOfCoins + "/8";
            if (!currentPlayerStats.InfiniteLives)
            {
                thisPlayerLivesCount.text = ": " + currentPlayerStats.AmountOfLives;
            }
            else
            {
                thisPlayerLivesCount.text = ": Infinite";
            }
            thisPlayerMcGuffinCount.text = ": " + currentPlayerStats.AmountOfMcGuffins + "/" + GameManager.Instance.MaxMcGuffinCount;
            
            
            if (!PhotonNetwork.OfflineMode)
            {
                // stats for other player
                otherPlayerCoinCount.text = otherPlayerStats.AmountOfCoins + "/8" + " :";
                if (!currentPlayerStats.InfiniteLives)
                {
                    otherPlayerLivesCount.text = otherPlayerStats.AmountOfLives + " :";
                }
                else
                {
                    otherPlayerLivesCount.text = "Infinite :";
                }
                otherPlayerMcGuffinCount.text = otherPlayerStats.AmountOfMcGuffins + "/" + GameManager.Instance.MaxMcGuffinCount + " :";
            }
            

            // timer
            if (GameManager.Instance.HasTimeLimit)
            {
                timerLabel.text = "Time:\n" + GameManager.Instance.RoundTimer.ToString("F0");
            }


            // spectator
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsSpectator", out object isSpectator))
            {
                // set the label text of the player you're spectating
                if ((bool)isSpectator)
                {
                    currentPlayerLabel.text = GameManager.Instance.spec_CurrentPlayerTarget.NickName;
                }
            }
        }

        if (!PhotonNetwork.OfflineMode)
        {
            pingText.text = "Ping: " + PhotonNetwork.GetPing() + "ms";
        }
        else
        {
            pingText.gameObject.SetActive(false);
        }
    }

    // upon pressing the leave button on the in-game menu
    public void LeaveRoom()
    {
        FindObjectOfType<AudioManager>().StopAllMusic();
        FindObjectOfType<CameraFollow>().hasConnected = false;
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }

    public void ToggleMenu()
    {
        Debug.Log("Toggle Menu");
        if (menuActive)
        {
            ingameMenuObj.SetActive(false);
            menuActive = false;

            // worlds most convoluted way of setting canMove on the player to true
            PlayerStatus[] plrSt = FindObjectsOfType<PlayerStatus>();
            foreach (var current in plrSt)
            {
                if (current.photonView.IsMine)
                {
                    current.GetComponent<PlayerMovement>().CanMove = true;
                }
            }
        }
        else
        {
            ingameMenuObj.SetActive(true);
            menuActive = true;

        }
    }

    public void StartIntro()
    {
        StartCoroutine(GameIntroUI());
    }

    private IEnumerator GameIntroUI()
    {
        readyText.SetActive(true);

        yield return new WaitForSeconds(1.35f);

        readyText.SetActive(false);
        goText.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        goText.SetActive(false);
    }

    public void SpectatorLeaveNotifDisplay(Player leaver)
    {
        spectatorLeaveNotif.SetActive(true);
        spectatorLeaveText.text = "Spectator " + leaver.NickName + " has left the game";
        StartCoroutine(HideLeaveNotifOnDelay());
    }

    private IEnumerator HideLeaveNotifOnDelay()
    {
        yield return new WaitForSeconds(1.5f);

        spectatorLeaveAnim.SetBool("Done", true);
    }
}
