using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBallBehaviour : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private float lifetime;

    private float mapLoopCooldown = 0.25f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyAfterTime());

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
        mapLoopCooldown -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Player":
                if (photonView.Owner.IsMasterClient)
                {
                    PhotonView targetPlayerView = collision.gameObject.GetComponent<PhotonView>();

                    targetPlayerView.RPC("FreezePlayer", targetPlayerView.Owner);
                    PhotonNetwork.Destroy(gameObject);
                }
                else
                {
                    photonView.RPC("RequestToDestroyIceBall", RpcTarget.MasterClient);
                }
                break;
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
    public void IgnoreOwnerCollision(int plrViewID)
    {
        PhotonView view = PhotonView.Find(plrViewID);
        
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), view.GetComponent<Collider2D>());
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        if (photonView.Owner.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            photonView.RPC("RequestToDestroyIceBall", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void RequestToDestroyIceBall()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       if (stream.IsWriting)
       {
            stream.SendNext(mapLoopCooldown);
            stream.SendNext(lifetime);
       }
       else
       {
            mapLoopCooldown = (float)stream.ReceiveNext();
            lifetime = (float)stream.ReceiveNext();
       }
    }
}
