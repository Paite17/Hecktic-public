using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayableCharacterState
{
    LEW,
    BRYNGLES,
    CHUM,
    KING_CHOD
}

// this class is for keeping track of what character the player is over the network, and probably a few other
// things as well
public class PlayerStatus : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private PlayableCharacterState charState;
    [SerializeField] private float amountOfMcGuffins;
    [SerializeField] private float amountOfLives;
    [SerializeField] private float amountOfCoins;
    [SerializeField] private bool infiniteLives;
    [SerializeField] private int numberOfWins;

    private float pickleQuest_Objective1Count;
    private float pickleQuest_Objective2Count;
    private float pickleQuest_Objective3Count;

    public PlayableCharacterState CharState
    {
        get { return charState; }
        set { charState = value; }
    }

    public float AmountOfMcGuffins
    {
        get { return amountOfMcGuffins; }
        set { amountOfMcGuffins = value; }
    }

    public float AmountOfLives
    {
        get { return amountOfLives; }
        set { amountOfLives = value; }
    }

    public float AmountOfCoins
    {
        get { return amountOfCoins; }
        set { amountOfCoins = value; }
    }

    public bool InfiniteLives
    {
        get { return infiniteLives; }
        set { infiniteLives = value; }
    }

    public int NumberOfWins
    {
        get { return numberOfWins; }
        set { numberOfWins = value; }
    }

    public float PickleQuest_Objective1Count
    {
        get { return pickleQuest_Objective1Count; }
        set { pickleQuest_Objective1Count = value; }
    }

    public float PickleQuest_Objective2Count
    {
        get { return pickleQuest_Objective2Count; }
        set { pickleQuest_Objective2Count = value; }
    }

    public float PickleQuest_Objective3Count
    {
        get { return pickleQuest_Objective3Count; }
        set { pickleQuest_Objective3Count = value; }
    }


    private void Update()
    {
        if (amountOfMcGuffins < 0)
        {
            amountOfMcGuffins = 0;
        }
    }

    [PunRPC]
    public void CollectedMcGuffin(bool isSecondHand)
    {
        amountOfMcGuffins++;
        if (!isSecondHand)
        {
            GameManager.Instance.ResetMapData(false);
        }
        else
        {
            GameManager.Instance.ResetMapData(true);
        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(amountOfMcGuffins);
            stream.SendNext(amountOfLives);
            stream.SendNext(amountOfCoins);
            stream.SendNext(charState);
            stream.SendNext(infiniteLives);
            stream.SendNext(numberOfWins);
        }
        else
        {
            amountOfMcGuffins = (float)stream.ReceiveNext();
            amountOfLives = (float)stream.ReceiveNext();
            amountOfCoins = (float)stream.ReceiveNext();
            charState = (PlayableCharacterState)stream.ReceiveNext();
            infiniteLives = (bool)stream.ReceiveNext();
            numberOfWins = (int)stream.ReceiveNext();
        }
        
    }
}
