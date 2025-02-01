using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class ThrowableRock : MonoBehaviourPunCallbacks, IPunObservable
{
    private PlayerMovement thisPlayer;

    private int originalOwnerID;

    private float rotateSpeed;

    private float mapLoopCooldown = 0.25f;

    public PlayerMovement ThisPlayer
    {
        get { return thisPlayer; }
    }

    public int OriginalOwnerID
    {
        get { return originalOwnerID; }
    }

    // Start is called before the first frame update
    void Start()
    {
        rotateSpeed = Random.Range(1.5f, 4.5f);

        McGuffin[] mcguffs = FindObjectsOfType<McGuffin>();

        for (int i = 0; i < mcguffs.Length; i++)
        {
            if (mcguffs[i] != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), mcguffs[i].GetComponent<Collider2D>());
            }           
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.0f, 0.0f, rotateSpeed);
        mapLoopCooldown -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        


        if (PhotonNetwork.IsMasterClient)
        {
            if (collision.gameObject.tag != "LevelBorder")
            {
                if (collision.gameObject.tag == "Player")
                {
                    PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
                    targetView.RPC("RockMoment", targetView.Owner);
                }
                FindObjectOfType<AudioManager>().Play("BoulderSmash");
                PhotonNetwork.Destroy(gameObject);
            }           
        }     

        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "LeftSideLevelLoop":
                if (mapLoopCooldown <= 0f)
                {
                    Debug.Log("left side collision");
                    if (!GameManager.Instance.CurrentMapData.StageisVertical)
                    {
                        transform.position = new Vector2(GameManager.Instance.CurrentMapData.RightSideEdge, transform.position.y);
                    }
                    else
                    {
                        transform.position = new Vector2(transform.position.x, GameManager.Instance.CurrentMapData.RightSideEdge);
                    }
                    mapLoopCooldown = 0.5f;

                }

                break;
            case "RightSideLevelLoop":
                if (mapLoopCooldown <= 0f)
                {
                    Debug.Log("right side collision");
                    if (!GameManager.Instance.CurrentMapData.StageisVertical)
                    {
                        transform.position = new Vector2(GameManager.Instance.CurrentMapData.LeftSideEdge, transform.position.y);
                    }
                    else
                    {
                        transform.position = new Vector2(transform.position.x, GameManager.Instance.CurrentMapData.LeftSideEdge);
                    }

                    mapLoopCooldown = 0.5f;
                }
                break;
        }
    }

    [PunRPC]
    public void InitialiseRock(int ownerID)
    {
        originalOwnerID = ownerID;
        thisPlayer = PhotonView.Find(ownerID).GetComponent<PlayerMovement>();
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), thisPlayer.GetComponent<Collider2D>());
        // checking if this was spawned in by the non-host, and then making it belong to the host so they can destroy it
        if (!PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(originalOwnerID);
            stream.SendNext(mapLoopCooldown);
        }
        else
        {
            originalOwnerID = (int)stream.ReceiveNext();
            mapLoopCooldown = (float)stream.ReceiveNext();
        }
    }
}
