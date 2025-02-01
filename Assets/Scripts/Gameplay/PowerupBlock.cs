using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PowerupBlock : MonoBehaviour, IPunObservable
{
    [SerializeField] private GameObject storedPowerup;
    [SerializeField] private Transform spawnYPos;
    [SerializeField] private bool isHidden;
    private Animator anim;

    [SerializeField] private bool hasBeenUsed;

    public bool bruh;

    public bool HasBeenUsed
    {
        get { return hasBeenUsed; }
        set { hasBeenUsed = value; }
    }

    public bool IsHidden
    {
        get { return isHidden; }
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        if (isHidden)
        {
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);
            GetComponent<BoxCollider2D>().isTrigger = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && isHidden)
        {
            Vector2 collisionDir = collision.GetContact(0).normal;
            if (collisionDir.y > 0)
            {
                if (!hasBeenUsed)
                {
                    Vector3 sPos = new Vector3(transform.position.x, spawnYPos.position.y, transform.position.z);
                    Vector3 sRot = new Vector3(1, 180, 1);
                    if (storedPowerup.tag == "Coin")
                    {
                        FindObjectOfType<AudioManager>().Play("CoinCollect");
                        StartCoroutine(CoinSequence(collision.gameObject.GetComponent<PlayerStatus>(), sPos));
                    }
                    else
                    {
                        if (!bruh)
                        {
                            if (collision.gameObject.GetComponent<PhotonView>().IsMine)
                            {
                                if (isHidden)
                                {
                                    GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
                                    GetComponent<BoxCollider2D>().isTrigger = false;
                                }
                                FindObjectOfType<AudioManager>().Play("PowerupBlock");

                                Quaternion please = new Quaternion(sRot.x, sRot.y, sRot.z, 1f);
                                PhotonNetwork.Instantiate(storedPowerup.name, sPos, Quaternion.identity);
                            }
                            bruh = true;
                        }
                    }
                    GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
                    GetComponent<BoxCollider2D>().isTrigger = false;     
                }
                hasBeenUsed = true;
            }
        }

        if (collision.gameObject.tag == "Player" && collision.gameObject.GetComponent<PlayerMovement>().powerupState == CharPowerupState.GIANT)
        {
            
            GetComponent<Collider2D>().enabled = false;
            gameObject.SetActive(false);
            FindObjectOfType<AudioManager>().Play("BrickBreak");

            // fuckin hell
            if (collision.gameObject.GetComponent<PlayerMovement>().IsFacingRight)
            {
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(6f, collision.gameObject.GetComponent<Rigidbody2D>().velocity.y);
            }
            else
            {
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(-6f, collision.gameObject.GetComponent<Rigidbody2D>().velocity.y);
            }
            
        }

        // copied over from breakable bricks script
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // detect if the top of the player's collider is hitting the block
            if (contact.normal.y > 0.9f && collision.gameObject.tag == "Player")
            {
                Vector3 sPos = new Vector3(transform.position.x, spawnYPos.position.y, transform.position.z);
                Vector3 sRot = new Vector3(1, 180, 1);

                if (storedPowerup.tag == "Coin")
                {
                    if (collision.gameObject.GetComponent<PhotonView>().IsMine)
                    {  
                        if (!hasBeenUsed)
                        {
                            FindObjectOfType<AudioManager>().Play("CoinCollect");
                            StartCoroutine(CoinSequence(collision.gameObject.GetComponent<PlayerStatus>(), sPos));
                        }
                        
                    }
                }
                else
                {
                    if (storedPowerup.tag != "Coin")
                    {
                        if (!bruh)
                        {
                            if (collision.gameObject.GetComponent<PhotonView>().IsMine)
                            {
                                if (isHidden)
                                {
                                    GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
                                    GetComponent<BoxCollider2D>().isTrigger = false;
                                }
                                FindObjectOfType<AudioManager>().Play("PowerupBlock");
                                /*if (storedPowerup.GetComponent<Powerup>().GivenPowerup != CharPowerupState.GIANT)
                                {
                                    FindObjectOfType<AudioManager>().Play("PowerupBlock");
                                }
                                else
                                {
                                    FindObjectOfType<AudioManager>().Play("GiantAppearFromBlock");
                                } */
                                // how do quaternions work
                                Quaternion please = new Quaternion(sRot.x, sRot.y, sRot.z, 1f);
                                PhotonNetwork.Instantiate(storedPowerup.name, sPos, Quaternion.identity);
                            }                       
                            bruh = true;
                        }
                        
                    }
                } 
                hasBeenUsed = true;
            }

            if (collision.gameObject.tag == "Rock")
            {
                Vector3 sPos = new Vector3(transform.position.x, spawnYPos.position.y, transform.position.z);
                Vector3 sRot = new Vector3(1, 180, 1);

                if (storedPowerup.tag == "Coin")
                {
                    if (collision.gameObject.GetComponent<PhotonView>().IsMine)
                    {
                        if (!hasBeenUsed)
                        {
                            if (isHidden)
                            {
                                return;
                            }
                            FindObjectOfType<AudioManager>().Play("CoinCollect");
                            // absolutely diabolical this is
                            // DOES NOT WORK PROPERLY FOR THE NON-HOST
                            StartCoroutine(CoinSequence(PhotonView.Find(collision.gameObject.GetComponent<ThrowableRock>().OriginalOwnerID).GetComponent<PlayerStatus>(), sPos));
                        }
                    }
                }
                else
                {
                    if (storedPowerup.tag != "Coin")
                    {
                        if (!bruh)
                        {
                            if (collision.gameObject.GetComponent<PhotonView>().IsMine)
                            {
                                if (isHidden)
                                {
                                    return;
                                }
                                FindObjectOfType<AudioManager>().Play("PowerupBlock");
                                /*if (storedPowerup.GetComponent<Powerup>().GivenPowerup != CharPowerupState.GIANT)
                                {
                                    FindObjectOfType<AudioManager>().Play("PowerupBlock");
                                }
                                else
                                {
                                    FindObjectOfType<AudioManager>().Play("GiantAppearFromBlock");
                                } */
                                // how do quaternions work
                                Quaternion please = new Quaternion(sRot.x, sRot.y, sRot.z, 1f);
                                PhotonNetwork.Instantiate(storedPowerup.name, sPos, Quaternion.identity);
                            }
                            bruh = true;
                        }

                    }
                }
                hasBeenUsed = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "BellyFlop":
                Vector3 sPos = new Vector3(transform.position.x, spawnYPos.position.y, transform.position.z);
                if (storedPowerup.tag == "Coin")
                {
                    if (collision.gameObject.GetComponentInParent<PhotonView>().IsMine)
                    {
                        if (!hasBeenUsed)
                        {
                            FindObjectOfType<AudioManager>().Play("CoinCollect");
                            StartCoroutine(CoinSequence(collision.gameObject.GetComponentInParent<PlayerStatus>(), sPos));
                        }
                        
                    }
                }
                else
                {
                    if (storedPowerup.tag != "Coin")
                    {
                        if (!bruh)
                        {
                            if (collision.gameObject.GetComponentInParent<PhotonView>().IsMine)
                            {
                                FindObjectOfType<AudioManager>().Play("PowerupBlock");
                                /*if (storedPowerup.GetComponent<Powerup>().GivenPowerup != CharPowerupState.GIANT)
                                {
                                    FindObjectOfType<AudioManager>().Play("PowerupBlock");
                                }
                                else
                                {
                                    FindObjectOfType<AudioManager>().Play("GiantAppearFromBlock");
                                } */
                                // how do quaternions work
                                PhotonNetwork.Instantiate(storedPowerup.name, sPos, Quaternion.identity);
                            }
                            bruh = true;
                        }

                    }
                }
                hasBeenUsed = true;
                break;
        }
    }

    private IEnumerator CoinSequence(PlayerStatus thePlayer, Vector3 p)
    {
        if (hasBeenUsed)
        {
            yield return null;
        }
        //GameObject temp = Instantiate(storedPowerup, p, Quaternion.identity);
        bool bruh = false;
        if (!bruh)
        {
            thePlayer.AmountOfCoins++;
            bruh = true;
        }

        if (thePlayer.AmountOfCoins >= 8)
        {
            thePlayer.AmountOfCoins = 0;
            float percentage = (0.5f + ((100f * thePlayer.AmountOfMcGuffins) / GameManager.Instance.MaxMcGuffinCount));
            //GameManager.Instance.SpawnPowerup(thePlayer.transform, percentage);
            GameManager.Instance.photonView.RPC("SpawnPowerup", RpcTarget.All, percentage, thePlayer.photonView.ViewID);
        }

        hasBeenUsed = true;
        yield return new WaitForSeconds(0.7f);
        //Destroy(temp);
    }

    private void Update()
    {
        anim.SetBool("Used", hasBeenUsed);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(hasBeenUsed);
            stream.SendNext(bruh);
        }
        else
        {
            hasBeenUsed = (bool)stream.ReceiveNext();
            bruh = (bool)stream.ReceiveNext();
        }
    }
}
