using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

// TODO: include a reference to the photon 'Player' Class and make it set to whichever player matches its data
public class UIPlayerEntry : MonoBehaviourPunCallbacks
{
    public TMP_Text playerNameText;
    public Image playerCharacterIcon;
    public TMP_Text playerPingText;
    public Player thisPlayer;

    private void Update()
    {
        //GetComponentInChildren<Animator>().SetFloat("Char", thisPlayer.ActorNumber);
    }
}
