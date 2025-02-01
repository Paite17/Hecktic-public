using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum McGuffinType
{
    FRESH,  // a newly spawned mcguffin, stays stationary in the air
    SECOND_HAND   // a mcguffin dropped by the player, bounces on the ground
}

public class McGuffin : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private float bounceForce;
    [SerializeField] private float lifetimeLength;
    [SerializeField] private McGuffinType type;
    private Rigidbody2D rb;
    private PhotonView view;

    private float lifetimeTimer;

    public McGuffinType Type
    {
        get { return type; }
        set { type = value; }
    }

    public float BounceForce
    {
        get { return bounceForce; }
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        view = rb.GetComponent<PhotonView>();


        // ignore collision of all players
        PlayerStatus[] players = FindObjectsOfType<PlayerStatus>();

        foreach (var plr in players)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), plr.GetComponent<Collider2D>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case McGuffinType.FRESH:
                rb.angularVelocity = 0;
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
                break;
            case McGuffinType.SECOND_HAND:
                // something something bounce in a direction
                lifetimeTimer += Time.deltaTime;

                if (lifetimeTimer >= lifetimeLength)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(gameObject);
                    }
                    else
                    {
                        // to fix the issue of the non-host not being able to destory the object,
                        // we just gotta send a request to the master client to do it for them.
                        view.RPC("RequestDestroy", RpcTarget.MasterClient);
                    }
                }
                break;
        }
    }

    public void CollidingWithPlayer(PlayerMovement player)
    {
        if (player.photonView.IsMine)
        {
            bool isSecondHand = SecondHandCheck();
            player.photonView.RPC("CollectedMcGuffin", RpcTarget.All, isSecondHand);
            FindObjectOfType<AudioManager>().Play("PlayerGetsMcGuffin");
            // playing an sfx for the other player
            view.RPC("EnemyGotAMcGuffin", RpcTarget.Others);

            if (type == McGuffinType.FRESH)
            {
                GameManager.Instance.McguffinSpawned = false;
            }
            

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                // to fix the issue of the non-host not being able to destory the object,
                // we just gotta send a request to the master client to do it for them.
                view.RPC("RequestDestroy", RpcTarget.MasterClient);                
            }
        }
    }

    private bool SecondHandCheck()
    {
        if (type == McGuffinType.FRESH)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    [PunRPC]
    public void RequestDestroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    // cus ikt wasn't working normally
    [PunRPC]
    public void EnemyGotAMcGuffin()
    {
        FindObjectOfType<AudioManager>().Play("EnemyGetsMcGuffin");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(type);
        }
        else
        {
            type = (McGuffinType)stream.ReceiveNext();
        }
    }
}
