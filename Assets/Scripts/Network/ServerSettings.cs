using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// changable settings for the host
public class ServerSettings : MonoBehaviourPunCallbacks, IPunObservable
{
    //private PlayerCharState hostPlayerChar; // the character the host selected
    private string chosenMapName; // the name of the currently selected map
    private int playerCount = 2; // the player amount the host chose
    private int timeLimitPerRound; // the amount of time set for each round (0 will equal no timer)
    private int collectableAmountGoal; // the amount of collectables a player will need to win a round
    private int amountOfLives; // the amount of lives players get (0 will equal infinite lives)

    /*public PlayerCharState HostPlayerChar
    {
        get { return hostPlayerChar; }
        set { hostPlayerChar = value; }
    } */

    public string ChosenMapName
    {
        get { return chosenMapName; }
        set { chosenMapName = value; }
    }

    public int PlayerCount
    {
        get { return playerCount;}
        set { playerCount = value; }
    }

    public int TimeLimitPerRound
    {
        get { return timeLimitPerRound;}
        set { timeLimitPerRound = value; }
    }

    public int CollectableAmountGoal
    {
        get { return collectableAmountGoal;}
        set { collectableAmountGoal = value; }
    }

    public int AmountOfLives
    {
        get { return amountOfLives;}
        set { amountOfLives = value; }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.SendNext(hostPlayerChar);
            stream.SendNext(chosenMapName);
            stream.SendNext(timeLimitPerRound);
            stream.SendNext(collectableAmountGoal);
            stream.SendNext(amountOfLives);
        }
        else
        {

        }
    }
}
