using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Powerup : MonoBehaviourPunCallbacks, IPunObservable
{
    
    [SerializeField] private CharPowerupState givenPowerup;
    [SerializeField] private Material powerupMatLewis;
    [SerializeField] private Material powerupMatBryn;
    [Header("Coin spawn")]
    [SerializeField] private float spawnProbability;
    [SerializeField] private Vector3 offset;

    private bool spawnedFromCoins;

    private Transform target;
    // TODO: alternative material that gets applied to the player

    public CharPowerupState GivenPowerup
    {
        get { return givenPowerup; }
    }

    public Material PowerupMatLewis
    {
        get { return powerupMatLewis; }
    }

    public Material PowerupMatBryn
    {
        get { return powerupMatBryn; }
    }

    public float SpawnProbability
    {
        get { return spawnProbability; }
    }

    public bool SpawnedFromCoins
    {
        get { return spawnedFromCoins; }
        set { spawnedFromCoins = value; }
    }

    public Transform Target
    {
        get { return target; }
        set { target = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }
        // ignore collision of all players
        PlayerStatus[] players = FindObjectsOfType<PlayerStatus>();

        foreach (var plr in players)
        {
            Debug.Log(plr.gameObject.name);
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), plr.GetComponent<Collider2D>());
        }

        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (var enemy in enemies)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>());
        }
    }

    
    void LateUpdate()
    {
        if (spawnedFromCoins)
        {
            transform.position = new Vector3(target.position.x, offset.y, target.position.z);
        }
    }

    public void CollidingWithPlayer(PlayerMovement player)
    {
        if (player.photonView.IsMine)
        {
            if (givenPowerup != CharPowerupState.GIANT)
            {
                FindObjectOfType<AudioManager>().Play("Powerup");
                if (givenPowerup != CharPowerupState.INVINCIBLE)
                {
                    player.powerupState = givenPowerup;
                    // TODO: change material
                }
                else
                {
                    player.powerupState = givenPowerup;
                    // TODO: add an invincibility shader?
                }
            }
            else
            {
                player.powerupState = givenPowerup;
                // TODO: change XY scale of player
            }

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                GetComponent<PhotonView>().RPC("RequestOwnerToDestroy", RpcTarget.MasterClient, photonView.ViewID);
            }
        }
    }

    public void DestroyWithoutGivingPower()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            GetComponent<PhotonView>().RPC("RequestOwnerToDestroy", RpcTarget.MasterClient, photonView.ViewID);
        }
    }

    [PunRPC]
    public void RequestOwnerToDestroy(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);

        if (targetView != null && targetView.IsMine)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(spawnedFromCoins);
        }
        else
        {
            spawnedFromCoins = (bool)stream.ReceiveNext();
        }
    }
}
