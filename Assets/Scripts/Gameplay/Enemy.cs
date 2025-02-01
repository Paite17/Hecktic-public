using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum EnemyType
{
    GOON,
    GLOOP
}

public enum GloopState
{
    WALK,
    SHELL,
    SLIDE
}

// an attempt at polymorphism instead of the usual this time
public class Enemy : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private EnemyType enemyType_;
    [SerializeField] private GloopState gloopState_;
    [SerializeField] private bool edgeConcious;  // will the enemy avoid walking off edges?
    [SerializeField] private float movementSpeed;
    [SerializeField] private float shellSpeed;
    [SerializeField] private float freezeTimerLength;
    [SerializeField] private GameObject iceBlock;

    public Transform groundDetection;

    public float detectionDistance;

    private bool movingRight = true;

    public EnemySpawnPoint originalSpawnPoint;

    private Rigidbody2D rb;

    private bool wasJumpedOn;

    private Animator anim;

    private float mapLoopCooldown = 0.5f;

    private float slideCooldown = 0.5f;

    private bool isFrozen;

    private float freezeTimer;

    public bool WasJumpedOn
    {
        get { return wasJumpedOn; }
        set { wasJumpedOn = value; }
    }

    public bool EdgeConcious
    {
        get { return edgeConcious; }
        set { edgeConcious = value; }
    }

    public float MovementSpeed
    {
        get { return movementSpeed; }
        set { movementSpeed = value; }
    }

    public GloopState GloopState_
    {
        get { return gloopState_; }
        set { gloopState_ = value; }
    }

    public EnemyType EnemyType_
    {
        get { return enemyType_; }
    }

    public float SlideCooldown
    {
        get { return slideCooldown; }
    }

    public bool IsFrozen
    {
        get { return isFrozen; }
        set { isFrozen = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        McGuffin[] mcguffs = FindObjectsOfType<McGuffin>();

        for (int i = 0;  i < mcguffs.Length; i++)
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
        if (!isFrozen)
        {
            switch (gloopState_)
            {
                case GloopState.WALK:
                    Patrol();
                    break;
                case GloopState.SLIDE:
                    if (enemyType_ == EnemyType.GLOOP)
                    {
                        Slide();
                    }
                    break;
            }
        }
        else
        {
            freezeTimer -= Time.deltaTime;
        }
       
        if (freezeTimer < 0)
        {
            isFrozen = false;
            GetComponentInChildren<Renderer>().material.color = new Color32(255, 255, 255, 255);
            photonView.RPC("RevertEnemyMaterial", RpcTarget.All);
            iceBlock.SetActive(false);
        }


        if (gloopState_ == GloopState.SLIDE)
        {
            slideCooldown -= Time.deltaTime;
        }
    }

    private void Patrol()
    {
        if (movingRight)
        {
            rb.velocity = new Vector2(MovementSpeed - movementSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(MovementSpeed - movementSpeed, rb.velocity.y);
        }


        // move enemy
        transform.Translate(Vector2.right * movementSpeed * Time.deltaTime);

        // raycast for detecting either no ground
        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, detectionDistance);

        // if theres no ground and the enemy is concious of edges, it walks away from them
        if (!groundInfo.collider && edgeConcious)
        {
            Flip();
        }
    }

    private void Slide()
    {
        rb.velocity = new Vector2(movingRight ? shellSpeed : -shellSpeed, rb.velocity.y);
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;

        MovementSpeed = -movementSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PhotonView plrView = collision.gameObject.GetComponent<PhotonView>();
        switch (collision.gameObject.tag)
        {
            case "Player":
                if (mapLoopCooldown <= 0)
                {
                   
                    // if invincible or giant then KILL

                    if (plrView.gameObject.GetComponent<PlayerMovement>().powerupState == CharPowerupState.GIANT || plrView.gameObject.GetComponent<PlayerMovement>().powerupState == CharPowerupState.INVINCIBLE)
                    {
                        Kill();
                    }

                    // did this happen from above
                    if (collision.relativeVelocity.y < 0)
                    {
                        switch (enemyType_)
                        {
                            case EnemyType.GOON:
                                JumpedOn(plrView.gameObject.GetComponent<Rigidbody2D>());
                                break;
                            case EnemyType.GLOOP:
                                switch (gloopState_)
                                {
                                    case GloopState.WALK:
                                        EnterShell(plrView.GetComponent<Rigidbody2D>());
                                        break;
                                    case GloopState.SHELL:
                                        Rigidbody2D plrRB = collision.gameObject.GetComponent<Rigidbody2D>();
                                        StartSliding(plrRB.velocity.x);
                                        break;
                                    case GloopState.SLIDE:
                                        // stop 
                                        rb.velocity = Vector2.zero;
                                        FindObjectOfType<AudioManager>().Play("Stomp");
                                        EnterShell(plrView.GetComponent<Rigidbody2D>());
                                        break;
                                }
                                JumpedOn(plrView.gameObject.GetComponent<Rigidbody2D>());
                                break;
                        }

                    }
                    else
                    {
                        if (gloopState_ == GloopState.SHELL)
                        {
                            Rigidbody2D plrRB = collision.gameObject.GetComponent<Rigidbody2D>();
                            StartSliding(plrRB.velocity.x);
                        }
                    }
                    
                }               
                break;
            case "Rock":
                Kill();
                break;
            case "IceBall":
                isFrozen = true;
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(collision.gameObject);
                }
                else
                {
                    //photonView.RPC("DestroyIceBallWhenNoHost", RpcTarget.MasterClient, collision.gameObject.GetComponent<PhotonView>().ViewID);
                }
                rb.velocity = Vector2.zero;
                freezeTimer = freezeTimerLength;
                FindObjectOfType<AudioManager>().Play("PlayerFrozen");
                GetComponentInChildren<Renderer>().material.color = new Color32(0, 247, 255, 255);
                photonView.RPC("SetEnemyToFrozen", RpcTarget.All);
                iceBlock.SetActive(true);
                break;
            case "Enemy":
                if (collision.gameObject.GetComponent<Enemy>().GloopState_ == GloopState.SLIDE && collision.gameObject.GetComponent<Enemy>().enemyType_ == EnemyType.GLOOP)
                {
                    collision.gameObject.GetComponent<Enemy>().Kill();
                }
                else
                {
                    return;
                }
                break;
            default:
                // if the enemy collides with an obstacle of any kind except the player
                if (collision.gameObject.tag != "LevelBorder")
                {
                    if (mapLoopCooldown <= 0)
                    {
                        Flip();
                    }                   
                }
                else
                {
                    return;
                }
                break;
        }
        
    }

    [PunRPC]
    private void DestroyIceBallWhenNotHost(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        PhotonNetwork.Destroy(view.gameObject);
    }

    private void EnterShell(Rigidbody2D player)
    {
        anim.SetBool("InShell", true);
        anim.SetBool("Sliding", false);
        player.velocity = new Vector2(player.velocity.x, 8f);
        FindObjectOfType<AudioManager>().Play("Stomp");
        gloopState_ = GloopState.SHELL;
        rb.velocity = Vector3.zero;
    }

    private void StartSliding(float playerVelocity)
    {
        anim.SetBool("Sliding", true);
        FindObjectOfType<AudioManager>().Play("Stomp");
        gloopState_ = GloopState.SLIDE;
        movingRight = playerVelocity > 0;

        rb.velocity = new Vector2(movingRight ? shellSpeed : -shellSpeed, rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "BellyFlop":
                // JumpedOn(collision.gameObject.GetComponentInParent<Rigidbody2D>());
                Kill();
                break;
            case "Electricity":
                Kill();
                break;
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
            case "KillHazard":
                Kill();
                break;
        }
    }

    public void JumpedOn(Rigidbody2D player)
    {
        switch (enemyType_)
        {
            case EnemyType.GOON:
                Debug.Log("Jumped on!!!!");
                FindObjectOfType<AudioManager>().Play("Stomp");
                player.velocity = new Vector2(player.velocity.x, 8f);
                if (PhotonNetwork.IsMasterClient)
                {
                    if (originalSpawnPoint != null)
                    {
                        originalSpawnPoint.hasSpawned = false;
                    }               
                    PhotonNetwork.Destroy(gameObject);                  
                }
                else
                {
                    photonView.RPC("RequestHostToDestroyGoon", RpcTarget.MasterClient);
                }
                break;
            case EnemyType.GLOOP:
                Debug.Log("Jumped on!!!!");
                FindObjectOfType<AudioManager>().Play("Stomp");
                player.velocity = new Vector2(player.velocity.x, 8f);
                break;
        }
        
    }


    public void Kill()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            FindObjectOfType<AudioManager>().Play("Stomp");
            if (PhotonNetwork.IsMasterClient)
            {
                if (originalSpawnPoint != null)
                {
                    originalSpawnPoint.hasSpawned = false;
                }
                PhotonNetwork.Destroy(gameObject);
                
            }
            else
            {
                photonView.RPC("RequestHostToDestroyGoon", RpcTarget.MasterClient);
            }
        }
    }

    [PunRPC]
    private void RequestHostToDestroyGoon()
    {
        if (originalSpawnPoint != null)
        {
            originalSpawnPoint.hasSpawned = false;
        }
        PhotonNetwork.Destroy(gameObject);
        
    }

    [PunRPC]
    private void SetEnemyToFrozen()
    {
        GetComponentInChildren<MeshRenderer>().material.color = new Color32(0, 247, 255, 255);
    }

    [PunRPC]
    private void RevertEnemyMaterial()
    {
        GetComponentInChildren<MeshRenderer>().material.color = new Color32(255, 255, 255, 255);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gloopState_);
            stream.SendNext(mapLoopCooldown);
            stream.SendNext(slideCooldown);
            stream.SendNext(isFrozen);
            stream.SendNext(iceBlock.activeSelf);
        }   
        else
        {
            gloopState_ = (GloopState)stream.ReceiveNext();
            mapLoopCooldown = (float)stream.ReceiveNext();
            slideCooldown = (float)stream.ReceiveNext();
            isFrozen = (bool)stream.ReceiveNext();
            iceBlock.SetActive((bool)stream.ReceiveNext());
        }
    }
}
